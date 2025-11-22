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
    public class RoleBiz : Biz<Role>, IRoleBiz
    {
        public RoleBiz(IServiceProvider provider, IUnitOfWork unitOfWork)
            : base(provider, unitOfWork) { }

        // Retrieval
        public Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
            => RoleRepository.GetByNameAsync(roleName, cancellationToken);

        public Task<IEnumerable<Role>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default)
            => RoleRepository.GetByAppIdAsync(appId, cancellationToken);

        public Task<Role?> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
            => RoleRepository.GetWithPermissionsAsync(roleId, cancellationToken);

        public Task<IEnumerable<Role>> GetSystemDefinedAsync(CancellationToken cancellationToken = default)
            => RoleRepository.GetSystemDefinedAsync(cancellationToken);

        // NEW: Get all (global vs scoped)
        public async Task<IEnumerable<Role>> GetAllAsync(Guid? appId, bool isGlobalAdmin, CancellationToken cancellationToken = default)
        {
            var query = RoleRepository.Query()
                .Include(r => r.App)
                .Include(r => r.Permissions)
                .AsNoTracking();

            if (!isGlobalAdmin && appId.HasValue)
                query = query.Where(r => r.AppId == appId.Value);

            return await query
                .OrderBy(r => r.App.IsCoreApp)
                .ThenBy(r => r.IsSystemDefined)
                .ThenBy(r => r.RoleName)
                .ToListAsync(cancellationToken);
        }

        // NEW: Get by id with App & Permissions
        public async Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await RoleRepository.Query()
                .Include(r => r.App)
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
        }

        // NEW: Get with users (delegate to repo)
        public Task<Role?> GetWithUsersAsync(Guid roleId, CancellationToken cancellationToken = default)
            => RoleRepository.GetWithUsersAsync(roleId, cancellationToken);

        // Creation
        public async Task<Role> CreateRoleAsync(Guid appId, string roleName, CancellationToken cancellationToken = default)
        {
            var exists = await RoleRepository.ExistsByNameAsync(appId, roleName, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Role name already exists in this app.");

            var similar = await RoleRepository.IsNameSimilarToSystemRoleAsync(roleName, cancellationToken);
            if (similar)
                throw new InvalidOperationException("Role name is too similar to a system-defined role.");

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName,
                AppId = appId,
                IsSystemDefined = false
            };

            await RoleRepository.AddAsync(role, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return role;
        }

        // Update
        public async Task<Role> UpdateRoleNameAsync(Guid roleId, string newName, CancellationToken cancellationToken = default)
        {
            var role = await RoleRepository.GetByIdAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be updated.");

            var exists = await RoleRepository.ExistsByNameAsync(role.AppId, newName, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Role name already exists in this app.");

            role.RoleName = newName;
            RoleRepository.Update(role);
            await SaveChangesAsync(cancellationToken);
            return role;
        }

        // Permission management
        public async Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            var role = await RoleRepository.GetWithPermissionsAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be modified.");

            await RoleRepository.ReplaceRolePermissionsAsync(roleId, permissionIds, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await RoleRepository.GetWithPermissionsAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be modified.");

            await RoleRepository.RemoveRolePermissionAsync(roleId, permissionId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        // Deletion
        public async Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await RoleRepository.GetByIdAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be deleted.");

            RoleRepository.Remove(role);
            await SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
