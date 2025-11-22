using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class AppBiz : Biz<App>, IAppBiz
    {
        public AppBiz(IServiceProvider provider, IUnitOfWork unitOfWork)
            : base(provider, unitOfWork) { }

        // === Step 1: Retrieval ===
        public Task<App?> GetByNameAsync(string appName, CancellationToken cancellationToken = default)
        {
            return AppRepository.GetByNameAsync(appName, cancellationToken);
        }

        public Task<App?> GetWithRolesAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return AppRepository.GetWithRolesAsync(appId, cancellationToken);
        }

        public Task<App?> GetWithStatusesAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return AppRepository.GetWithStatusesAsync(appId, cancellationToken);
        }

        public async Task<List<App>> GetAllAppsAsync(CancellationToken cancellationToken = default)
        {
            return await AppRepository.Query()
                .OrderBy(a => a.IsCoreApp)
                .ThenBy(a => a.AppName)
                .ToListAsync(cancellationToken);
        }

        // === Step 2: Creation ===
        public async Task<App> CreateAppAsync(string appName, string redirectUrls, string scopes, bool autoApproveUsers, CancellationToken cancellationToken = default)
        {
            // NEW CHANGE: enforce uniqueness
            var exists = await AppRepository.ExistsByNameAsync(appName, cancellationToken);
            if (exists)
                throw new InvalidOperationException("App name already exists.");

            var app = new App
            {
                AppId = Guid.NewGuid(),
                AppName = appName,
                RedirectUrls = redirectUrls,
                Scopes = scopes,
                AutoApproveUsers = autoApproveUsers,
                IsCoreApp = false // NEW CHANGE: default to non-core
            };

            await AppRepository.AddAsync(app, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return app;
        }

        // === Step 3: Update ===
        public async Task<App> UpdateAppAsync(Guid appId, string newName, string redirectUrls, string scopes, bool autoApproveUsers, CancellationToken cancellationToken = default)
        {
            var app = await AppRepository.GetByIdAsync(appId, cancellationToken)
                ?? throw new InvalidOperationException("App not found.");

            if (app.IsCoreApp)
                throw new InvalidOperationException("System-defined apps cannot be updated."); // NEW CHANGE

            var exists = await AppRepository.ExistsByNameAsync(newName, cancellationToken);
            if (exists && !string.Equals(app.AppName, newName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("App name already exists.");

            app.AppName = newName;
            app.RedirectUrls = redirectUrls;
            app.Scopes = scopes;
            app.AutoApproveUsers = autoApproveUsers;

            AppRepository.Update(app);
            await SaveChangesAsync(cancellationToken);
            return app;
        }

        // === Step 4: Deletion ===
        public async Task<bool> DeleteAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            var app = await AppRepository.GetByIdAsync(appId, cancellationToken)
                ?? throw new InvalidOperationException("App not found.");

            if (app.IsCoreApp)
                throw new InvalidOperationException("System-defined apps cannot be deleted."); // NEW CHANGE

            AppRepository.Remove(app);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Step 5: Undelete logic ===
        public async Task<App> UndeleteAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            var app = await AppRepository.GetWithStatusesAsync(appId, cancellationToken)
                ?? throw new InvalidOperationException("App not found.");

            foreach (var status in app.UserStatuses)
            {
                if (string.Equals(status.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
                {
                    status.Status = app.AutoApproveUsers ? "Approved" : "Pending"; // NEW CHANGE
                    UserAppStatusRepository.Update(status);
                }
            }

            await SaveChangesAsync(cancellationToken);
            return app;
        }
    }
}
