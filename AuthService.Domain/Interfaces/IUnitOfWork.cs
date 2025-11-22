using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IAppRepository Apps { get; }
        IUserAppStatusRepository UserAppStatuses { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }

        IUserRoleRepository UserRoles { get; }
        IRolePermissionRepository RolePermissions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

    }

}
