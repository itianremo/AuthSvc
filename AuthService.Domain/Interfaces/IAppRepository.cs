using AuthService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IAppRepository : IRepository<App>
    {
        /// <summary>
        /// Retrieve an app by its unique name.
        /// </summary>
        Task<App?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if an app name already exists (case-insensitive).
        /// </summary>
        Task<bool> ExistsByNameAsync(string appName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve an app with its roles fully loaded.
        /// </summary>
        Task<App?> GetWithRolesAsync(Guid appId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve an app with its user statuses fully loaded.
        /// </summary>
        Task<App?> GetWithStatusesAsync(Guid appId, CancellationToken cancellationToken = default);
    }
}
