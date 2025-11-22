using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class DashboardBiz : IDashboardBiz
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Guid _adminAppId;

        public DashboardBiz(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;

            // Load the Admin Dashboard AppId from configuration (Init:Seeds:appId)
            var seeds = config.GetSection("Init:Seeds");
            _adminAppId = Guid.Parse(seeds["appId"]);
        }

        public async Task<DashboardMetricsDto> GetMetricsAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            // Parallelize queries for scalability
            var usersTotalTask = _unitOfWork.Users.CountByAppAsync(appId, cancellationToken);
            var rolesTotalTask = _unitOfWork.Roles.CountByAppAsync(appId, cancellationToken);
            var permissionsTotalTask = _unitOfWork.Permissions.CountByAppAsync(appId, cancellationToken);
            var activeUsersTask = _unitOfWork.Users.CountByStatusAsync(AppAccountStatus.Active, cancellationToken);
            var inactiveUsersTask = _unitOfWork.Users.CountByStatusAsync(AppAccountStatus.Suspended, cancellationToken);
            var verifiedUsersTask = _unitOfWork.Users.CountVerifiedAsync(cancellationToken);
            var unverifiedUsersTask = _unitOfWork.Users.CountUnverifiedAsync(cancellationToken);
            var systemRolesTask = _unitOfWork.Roles.GetSystemDefinedAsync(cancellationToken);
            var expiredTokensTask = _unitOfWork.PasswordResetTokens.GetExpiredTokensAsync(DateTime.UtcNow, cancellationToken);

            await Task.WhenAll(usersTotalTask, rolesTotalTask, permissionsTotalTask,
                               activeUsersTask, inactiveUsersTask, verifiedUsersTask,
                               unverifiedUsersTask, systemRolesTask, expiredTokensTask);

            return new DashboardMetricsDto
            {
                UsersTotal = usersTotalTask.Result,
                AppsTotal = 1, // Scoped to single appId
                RolesTotal = rolesTotalTask.Result,
                PermissionsTotal = permissionsTotalTask.Result,
                ActiveUsers = activeUsersTask.Result,
                InactiveUsers = inactiveUsersTask.Result,
                VerifiedUsers = verifiedUsersTask.Result,
                UnverifiedUsers = unverifiedUsersTask.Result,
                SystemRoles = systemRolesTask.Result.Count(),
                ExpiredPasswordTokens = expiredTokensTask.Result.Count()
            };
        }

    }
}
