using AuthService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IAppBiz : IBiz<App>
    {
        Task<App?> GetByNameAsync(string appName, CancellationToken cancellationToken = default);
        Task<App?> GetWithRolesAsync(Guid appId, CancellationToken cancellationToken = default);
        Task<App?> GetWithStatusesAsync(Guid appId, CancellationToken cancellationToken = default);

        Task<App> CreateAppAsync(string appName, string redirectUrls, string scopes, bool autoApproveUsers, CancellationToken cancellationToken = default);
        Task<App> UpdateAppAsync(Guid appId, string newName, string redirectUrls, string scopes, bool autoApproveUsers, CancellationToken cancellationToken = default);
        Task<bool> DeleteAppAsync(Guid appId, CancellationToken cancellationToken = default);
        Task<App> UndeleteAppAsync(Guid appId, CancellationToken cancellationToken = default);

        Task<List<App>> GetAllAppsAsync(CancellationToken cancellationToken = default);
    }
}
