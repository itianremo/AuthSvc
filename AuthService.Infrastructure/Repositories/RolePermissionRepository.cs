using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly AuthDbContext _context;

        public RolePermissionRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task AddRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles.Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

            if (role != null && permission != null && !role.Permissions.Contains(permission))
            {
                role.Permissions.Add(permission);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles.Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            if (role != null)
            {
                var permission = role.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);
                if (permission != null)
                {
                    role.Permissions.Remove(permission);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            return role?.Permissions ?? Enumerable.Empty<Permission>();
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            var permission = await _context.Permissions
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

            return permission?.Roles ?? Enumerable.Empty<Role>();
        }

        public async Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => r.AppId == appId)
                .SelectMany(r => r.Permissions)
                .CountAsync(cancellationToken);
        }

        public async Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            if (role == null) return;

            // Clear existing
            role.Permissions.Clear();

            // Attach new permissions
            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.PermissionId))
                .ToListAsync(cancellationToken);

            foreach (var p in permissions)
            {
                role.Permissions.Add(p);
            }
        }

    }
}
