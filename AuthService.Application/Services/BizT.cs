using AuthService.Domain.Interfaces;
using AuthService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    /// <summary>
    /// Base Biz class providing repository access and unit of work handling.
    /// </summary>
    public abstract class Biz<T> : IBiz<T> where T : class
    {
        protected readonly IServiceProvider _provider;
        protected readonly IUnitOfWork _unitOfWork;

        protected Biz(IServiceProvider provider, IUnitOfWork unitOfWork)
        {
            _provider = provider;
            _unitOfWork = unitOfWork;
        }

        // === Expose SaveChangesAsync consistently ===
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // === Expose transaction execution ===
        public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            return _unitOfWork.ExecuteInTransactionAsync(action, cancellationToken);
        }

        // === Repository accessors for all core entities ===
        protected IUserRepository UserRepository => _unitOfWork.Users;
        protected IRoleRepository RoleRepository => _unitOfWork.Roles;
        protected IPermissionRepository PermissionRepository => _unitOfWork.Permissions;
        protected IAppRepository AppRepository => _unitOfWork.Apps;
        protected IUserAppStatusRepository UserAppStatusRepository => _unitOfWork.UserAppStatuses;
        protected IPasswordResetTokenRepository PasswordResetTokenRepository => _unitOfWork.PasswordResetTokens; 

    }
}
