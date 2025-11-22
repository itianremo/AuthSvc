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
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AuthDbContext context) : base(context) { }

        public async Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.Users.Any(u => u.UserId == userId))
                .Select(r => r.RoleName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.RoleName == name, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(r => r.AppId == appId).ToListAsync(cancellationToken);
        }

        public async Task<Role?> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetSystemDefinedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(r => r.IsSystemDefined).ToListAsync(cancellationToken);
        }

        // === New helpers ===

        /// <summary>
        /// Check if a role name already exists within an app.
        /// </summary>
        public async Task<bool> ExistsByNameAsync(Guid appId, string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(r => r.AppId == appId && r.RoleName == roleName, cancellationToken);
        }

        /// <summary>
        /// Check if a role name is similar to any system-defined role.
        /// </summary>
        public async Task<bool> IsNameSimilarToSystemRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var systemRoles = await _dbSet
                .Where(r => r.IsSystemDefined)
                .Select(r => r.RoleName)
                .ToListAsync(cancellationToken);

            return systemRoles.Any(sr => sr.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Replace all permissions for a given role.
        /// </summary>
        public async Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            var role = await _dbSet.Include(r => r.Permissions)
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

            _dbSet.Update(role);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Assign a role to a user.
        /// </summary>
        public async Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.Include(u => u.Roles)
                                           .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            var role = await _dbSet.FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            if (user != null && role != null)
            {
                if (!user.Roles.Any(r => r.RoleId == roleId))
                {
                    user.Roles.Add(role);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Remove a permission from a role.
        /// </summary>
        public async Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await _dbSet.Include(r => r.Permissions)
                                   .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
            if (role == null) return;

            var permission = role.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);
            if (permission != null)
            {
                role.Permissions.Remove(permission);
                _dbSet.Update(role);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Role?> GetWithUsersAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.Users)
                    .ThenInclude(u => u.AppStatuses) 
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Where(r => r.Permissions.Any(p => p.PermissionId == permissionId))
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles.CountAsync(r => r.AppId == appId, cancellationToken);
        }

    }
}
