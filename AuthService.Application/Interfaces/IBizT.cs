using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Interfaces
{
    public interface IBiz<T> where T : class
    {
        IRepository<T> Repository { get; }

        // Expose the typed repositories you registered earlier
        IUserRepository UserRepository { get; }
        IAppRepository AppRepository { get; }
        IRoleRepository RoleRepository { get; }
        IPermissionRepository PermissionRepository { get; }
        IUserAppRepository UserAppRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IRolePermissionRepository RolePermissionRepository { get; }
        IUserAppStatusRepository UserAppStatusRepository { get; }

        Task<int> SaveChangesAsync();
    }
}
