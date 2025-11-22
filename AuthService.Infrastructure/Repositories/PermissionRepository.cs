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
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(AuthDbContext context) : base(context) { }

        /// <summary>
        /// Retrieve a permission by its unique name.
        /// </summary>
        public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PermissionName == name, cancellationToken);
        }

        /// <summary>
        /// Retrieve all system-defined permissions.
        /// </summary>
        public async Task<IEnumerable<Permission>> GetSystemDefinedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(p => p.IsSystemDefined).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve all permissions associated with a given role.
        /// </summary>
        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Roles.Any(r => r.RoleId == roleId))
                .ToListAsync(cancellationToken);
        }

        // === New helper ===

        /// <summary>
        /// Check if a permission name already exists (case-insensitive).
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string permissionName, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                p => p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase),
                cancellationToken
            );
        }

        public async Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => r.AppId == appId)
                .SelectMany(r => r.Permissions)
                .Select(p => p.PermissionId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

    }
}
