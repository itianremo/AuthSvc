using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class Biz<T> : IBiz<T> where T : class
    {
        private readonly IServiceProvider _provider;

        public Biz(IServiceProvider provider)
        {
            _provider = provider;
        }

        // Core repository for the generic T
        public IRepository<T> Repository => _provider.GetRequiredService<IRepository<T>>();

        // Expose the concrete repositories via the IServiceProvider
        public IUserRepository UserRepository => _provider.GetRequiredService<IUserRepository>();
        public IAppRepository AppRepository => _provider.GetRequiredService<IAppRepository>();
        public IRoleRepository RoleRepository => _provider.GetRequiredService<IRoleRepository>();
        public IPermissionRepository PermissionRepository => _provider.GetRequiredService<IPermissionRepository>();
        public IUserAppRepository UserAppRepository => _provider.GetRequiredService<IUserAppRepository>();
        public IUserRoleRepository UserRoleRepository => _provider.GetRequiredService<IUserRoleRepository>();
        public IRolePermissionRepository RolePermissionRepository => _provider.GetRequiredService<IRolePermissionRepository>();
        public IUserAppStatusRepository UserAppStatusRepository => _provider.GetRequiredService<IUserAppStatusRepository>();

        public Task<int> SaveChangesAsync()
        {
            return Repository.SaveChangesAsync();
        }
    }
}
