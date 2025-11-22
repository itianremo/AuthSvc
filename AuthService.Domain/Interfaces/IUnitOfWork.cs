using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IAppRepository Apps { get; }
        IUserAppStatusRepository UserAppStatuses { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }

        IUserRoleRepository UserRoles { get; }
        IRolePermissionRepository RolePermissions { get; }
    }

}
