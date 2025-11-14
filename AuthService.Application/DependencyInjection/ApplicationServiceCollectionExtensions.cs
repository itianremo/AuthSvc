using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Biz services
            services.AddScoped(typeof(IBiz<>), typeof(Biz<>));
            services.AddScoped<IUserBiz, UserBiz>();
            services.AddScoped<IAppBiz, AppBiz>();
            services.AddScoped<IRoleBiz, RoleBiz>();
            services.AddScoped<IPermissionBiz, PermissionBiz>();
            services.AddScoped<IUserAppBiz, UserAppBiz>();
            services.AddScoped<IUserRoleBiz, UserRoleBiz>();
            services.AddScoped<IRolePermissionBiz, RolePermissionBiz>();
            services.AddScoped<IUserAppStatusBiz, UserAppStatusBiz>();

            // Identity helpers / configuration used by application logic
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
            });

            // HttpClient used across application services
            services.AddHttpClient();

            return services;
        }
    }
}