using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Data
{
    using AuthService.Domain.Configs;
    using AuthService.Domain.Entities;
    using AuthService.Infrastructure.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public interface IDbInitializer
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly AuthDbContext _db;
        private readonly InitConfig _config;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(AuthDbContext db, IOptions<InitConfig> options, ILogger<DbInitializer> logger)
        {
            _db = db;
            _config = options.Value;
            _logger = logger;
        }

        //public async Task SeedAsync(CancellationToken cancellationToken = default)
        //{
        //    //await _db.SaveChangesAsync(cancellationToken);
        //    _logger.LogInformation("Runtime seeding is paused successfully.");
        //}
        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var s = _config.Seeds;
            var permsConfig = _config.Permissions;

            if (s == null)
            {
                _logger.LogWarning("Init:Seeds section missing; skipping runtime seeding.");
                return;
            }

            // === App ===
            var app = await _db.Apps.FirstOrDefaultAsync(a => a.AppId == s.AppId, cancellationToken);
            if (app == null)
            {
                app = new App
                {
                    AppId = s.AppId,
                    AppName = s.AppName,
                    RedirectUrls = s.AppRedirectUrls,
                    Scopes = s.AppScopes,
                    AutoApproveUsers = false,
                    IsCoreApp = true
                };
                _db.Apps.Add(app);
            }

            // === User ===
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == s.UserId, cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    UserId = s.UserId,
                    Email = s.UserEmail,
                    PhoneNumber = s.UserPhoneNumber,
                    HashedPassword = s.UserPasswordHash,
                    CreatedAt = s.UserCreatedAt == default ? new DateTime(2024, 1, 1) : s.UserCreatedAt,
                    UpdatedAt = s.UserCreatedAt == default ? new DateTime(2024, 1, 1) : s.UserCreatedAt,
                    IsEmailVerified = s.UserIsEmailVerified,
                    IsPhoneVerified = s.UserIsPhoneVerified,
                    IsDeleted = false
                };
                _db.Users.Add(user);
            }

            // === Role ===
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == s.RoleId, cancellationToken);
            if (role == null)
            {
                role = new Role
                {
                    RoleId = s.RoleId,
                    RoleName = s.RoleName,
                    IsSystemDefined = true,
                    AppId = s.AppId
                };
                _db.Roles.Add(role);
            }

            // === Permissions (multiple from InitPermissions) ===
            if (permsConfig != null)
            {
                var allPerms = new[]
                {
                permsConfig.SuperAccess,
                permsConfig.ManageApps,
                permsConfig.ManageAssign,
                permsConfig.ManageUsers,
                permsConfig.ManageRoles,
                permsConfig.ManagePermissions
            };

                foreach (var permName in allPerms.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    var existing = await _db.Permissions.FirstOrDefaultAsync(p => p.PermissionName == permName, cancellationToken);
                    if (existing == null)
                    {
                        _db.Permissions.Add(new Permission
                        {
                            PermissionId = Guid.NewGuid(),
                            PermissionName = permName,
                            IsSystemDefined = true
                        });
                    }
                }
            }

            // === UserAppStatus ===
            if (!await _db.UserAppStatuses.AnyAsync(x => x.UserId == s.UserId && x.AppId == s.AppId, cancellationToken))
            {
                _db.UserAppStatuses.Add(new UserAppStatus
                {
                    UserAppId = s.AdminUserAppId != Guid.Empty ? s.AdminUserAppId : Guid.NewGuid(),
                    UserId = s.UserId,
                    AppId = s.AppId,
                    Status = s.UserGlobalAccountStatus == default ? AppAccountStatus.Active : s.UserGlobalAccountStatus,
                    RefreshToken = "",
                    RefreshTokenExpiry = null,
                    LastLogin = null
                });
            }

            // === Join: UsersRoles ===
            var hasUserRole = await _db.Set<Dictionary<string, object>>("UsersRoles")
                .AnyAsync(x =>
                    EF.Property<Guid>(x, "UserId") == s.UserId &&
                    EF.Property<Guid>(x, "RoleId") == s.RoleId, cancellationToken);

            if (!hasUserRole)
            {
                _db.Set<Dictionary<string, object>>("UsersRoles")
                   .Add(new Dictionary<string, object>
                   {
                       ["UserId"] = s.UserId,
                       ["RoleId"] = s.RoleId
                   });
            }

            // === Join: RolesPermissions (link role to all permissions) ===
            if (permsConfig != null)
            {
                var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == s.RoleId, cancellationToken);
                var permissions = await _db.Permissions.ToListAsync(cancellationToken);

                foreach (var perm in permissions)
                {
                    var hasLink = await _db.Set<Dictionary<string, object>>("RolesPermissions")
                        .AnyAsync(x =>
                            EF.Property<Guid>(x, "RoleId") == s.RoleId &&
                            EF.Property<Guid>(x, "PermissionId") == perm.PermissionId, cancellationToken);

                    if (!hasLink)
                    {
                        _db.Set<Dictionary<string, object>>("RolesPermissions")
                           .Add(new Dictionary<string, object>
                           {
                               ["RoleId"] = s.RoleId,
                               ["PermissionId"] = perm.PermissionId
                           });
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Runtime seeding completed successfully.");
        }
    }
}
