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
    public class AppRepository : Repository<App>, IAppRepository
    {
        public AppRepository(AuthDbContext context) : base(context) { }

        /// <summary>
        /// Retrieve an app by its unique name.
        /// </summary>
        public async Task<App?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AppName == name, cancellationToken);
        }

        // === New helpers ===

        /// <summary>
        /// Check if an app name already exists (case-insensitive).
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string appName, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                a => a.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase),
                cancellationToken
            );
        }

        /// <summary>
        /// Retrieve an app with its roles fully loaded.
        /// </summary>
        public async Task<App?> GetWithRolesAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.Roles)
                .FirstOrDefaultAsync(a => a.AppId == appId, cancellationToken);
        }

        /// <summary>
        /// Retrieve an app with its user statuses fully loaded.
        /// </summary>
        public async Task<App?> GetWithStatusesAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.UserStatuses)
                .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(a => a.AppId == appId, cancellationToken);
        }
    }
}
