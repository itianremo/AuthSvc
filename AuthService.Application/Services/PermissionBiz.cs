using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class PermissionBiz : Biz<Permission>, IPermissionBiz
    {
        public PermissionBiz(IServiceProvider provider, IUnitOfWork unitOfWork)
            : base(provider, unitOfWork) { }

        public Task<Permission?> GetByNameAsync(string permissionName, CancellationToken cancellationToken = default)
            => PermissionRepository.GetByNameAsync(permissionName, cancellationToken);

        public Task<IEnumerable<Permission>> GetSystemDefinedAsync(CancellationToken cancellationToken = default)
            => PermissionRepository.GetSystemDefinedAsync(cancellationToken);

        public Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
            => PermissionRepository.GetByRoleIdAsync(roleId, cancellationToken);

        public async Task<Permission> CreatePermissionAsync(string permissionName, CancellationToken cancellationToken = default)
        {
            var exists = await PermissionRepository.ExistsByNameAsync(permissionName, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Permission name already exists.");

            var permission = new Permission
            {
                PermissionId = Guid.NewGuid(),
                PermissionName = permissionName,
                IsSystemDefined = false
            };

            await PermissionRepository.AddAsync(permission, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return permission;
        }

        public async Task<Permission> UpdatePermissionNameAsync(Guid permissionId, string newName, CancellationToken cancellationToken = default)
        {
            var permission = await PermissionRepository.GetByIdAsync(permissionId, cancellationToken)
                ?? throw new InvalidOperationException("Permission not found.");

            if (permission.IsSystemDefined)
                throw new InvalidOperationException("System-defined permissions cannot be updated.");

            var exists = await PermissionRepository.ExistsByNameAsync(newName, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Permission name already exists.");

            permission.PermissionName = newName;
            PermissionRepository.Update(permission);
            await SaveChangesAsync(cancellationToken);
            return permission;
        }

        public async Task<bool> DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            var permission = await PermissionRepository.GetByIdAsync(permissionId, cancellationToken)
                ?? throw new InvalidOperationException("Permission not found.");

            if (permission.IsSystemDefined)
                throw new InvalidOperationException("System-defined permissions cannot be deleted.");

            PermissionRepository.Remove(permission);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.RolePermissions.AddRolePermissionAsync(roleId, permissionId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task UnassignPermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.RolePermissions.RemoveRolePermissionAsync(roleId, permissionId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<PermissionListItemDto>> GetAllPermissionsAsync(Guid appId, bool isGlobalAdmin, CancellationToken cancellationToken = default)
        {
            if (isGlobalAdmin)
            {
                return await PermissionRepository.Query()
                    .Include(p => p.Roles)
                    .AsNoTracking()
                    .Select(p => new PermissionListItemDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        IsSystemDefined = p.IsSystemDefined,
                        Roles = p.Roles.Select(r => new RoleListItemDto
                        {
                            RoleId = r.RoleId,
                            RoleName = r.RoleName,
                            IsSystemDefined = r.IsSystemDefined
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);
            }

            // Scoped permissions: only roles in this app
            return await PermissionRepository.Query()
                .Where(p => p.Roles.Any(r => r.AppId == appId))
                .Include(p => p.Roles)
                .AsNoTracking()
                .Select(p => new PermissionListItemDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    IsSystemDefined = p.IsSystemDefined,
                    Roles = p.Roles
                        .Where(r => r.AppId == appId)
                        .Select(r => new RoleListItemDto
                        {
                            RoleId = r.RoleId,
                            RoleName = r.RoleName,
                            IsSystemDefined = r.IsSystemDefined
                        }).ToList()
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId, Guid appId, bool isGlobalAdmin, CancellationToken cancellationToken = default)
        {
            var permission = await PermissionRepository.Query()
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

            if (permission == null)
                return null;

            if (!isGlobalAdmin && !permission.Roles.Any(r => r.AppId == appId))
                throw new UnauthorizedAccessException("You are not authorized to access this permission.");

            return new PermissionDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined,
                RoleId = permission.Roles
                    .Where(r => r.AppId == appId)
                    .Select(r => r.RoleId)
                    .FirstOrDefault(),
                RoleName = permission.Roles
                    .Where(r => r.AppId == appId)
                    .Select(r => r.RoleName)
                    .FirstOrDefault()
            };
        }
    }
}
