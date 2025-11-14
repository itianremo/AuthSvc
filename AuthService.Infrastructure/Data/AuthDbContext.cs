using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<UserApp> UserApps { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAppStatus> UserAppStatuses { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Unique Indexes ===
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber).IsUnique();

            // === Composite Key for RolePermission ===
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // === UserRole Relationships ===
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<UserRole>()
            //    .HasOne(ur => ur.App)
            //    .WithMany(a => a.UserRoles)
            //    .HasForeignKey(ur => ur.AppId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // === Seed Data ===
            #region Seed Admin
            var seeds = _configuration.GetSection("Init:Seeds");
            var permissions = _configuration.GetSection("Init:Permissions");

            // App
            var appId = Guid.Parse(seeds["appId"]);
            var appName = seeds["appName"];
            var appRedirectUrls = seeds["appRedirectUrls"];
            var appScopes = seeds["appScopes"];
            // Roles
            var roleId = Guid.Parse(seeds["roleId"]);
            var roleName = seeds["roleName"];
            // Admin User
            var userId = Guid.Parse(seeds["userId"]);
            var userEmail = seeds["userEmail"];
            var userPhoneNumber = seeds["userPhoneNumber"];
            var userPasswordHash = seeds["userPasswordHash"];
            var userGlobalAccountStatus = seeds["userGlobalAccountStatus"];
            var userIsEmailVerified = seeds["userIsEmailVerified"];
            var userIsPhoneVerified = seeds["userIsPhoneVerified"];
            var userCreatedAt = new DateTime(2024, 1, 1);
            //User App Access
            var adminUserAppId = Guid.Parse(seeds["adminUserAppId"]);
            //User Role Assignment
            var adminUserRoleId = Guid.Parse(seeds["adminUserRoleId"]);
            // Permission
            var permissionId = Guid.Parse(seeds["permissionId"]);
            var permissionName = seeds["permissionName"];
            // Permission Role assignment
            var rolePermissionId = Guid.Parse(seeds["rolePermissionId"]);

            // Apps
            modelBuilder.Entity<App>().HasData(
                new App
                {
                    AppId = appId,
                    AppName = appName,
                    RedirectUrls = appRedirectUrls,
                    Scopes = appScopes,
                    AutoApproveUsers = false,
                    IsCoreApp = true
                });
            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = roleId,
                    RoleName = roleName,
                    AppId = appId,
                    IsSystemDefined = true
                });
            // Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = userId,
                    Email = userEmail,
                    PhoneNumber = userPhoneNumber,
                    HashedPassword = userPasswordHash,
                    CreatedAt = userCreatedAt,
                    IsEmailVerified = bool.Parse(userIsEmailVerified),
                    IsPhoneVerified = bool.Parse(userIsPhoneVerified),
                    GlobalAccountStatus = userGlobalAccountStatus
                });
            // Admin's App Access
            modelBuilder.Entity<UserApp>().HasData(
                new UserApp
                {
                    UserAppId = adminUserAppId,
                    UserId = userId,
                    AppId = appId,
                    AccountStatus = userGlobalAccountStatus,
                    RefreshToken = string.Empty,
                    CreatedAt = userCreatedAt
                });
            // Admin's Role Assignment
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserRoleId = adminUserRoleId,
                    UserId = userId,
                    //AppId = appId,
                    RoleId = roleId
                });
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permissionId,
                    PermissionName = permissionName,
                    IsSystemDefined = true
                }
            );
            // Link Permission to Role
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    RolePermissionId = rolePermissionId
                }
            );
            #endregion

            #region Seed init permissions
            // Permission : CanManageApps
            var permIdManageApps = Guid.Parse(permissions["permIdManageApps"]);
            var permNameManageApps = permissions["permNameManageApps"];
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permIdManageApps,
                    PermissionName = permNameManageApps,
                    IsSystemDefined = true
                }
            );
            // Permission : CanManageUsers
            var permIdManageUsers = Guid.Parse(permissions["permIdManageUsers"]);
            var permNameManageUsers = permissions["permNameManageUsers"];
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permIdManageUsers,
                    PermissionName = permNameManageUsers,
                    IsSystemDefined = true
                }
            );
            // Permission : CanManageRoles
            var permIdManageRoles = Guid.Parse(permissions["permIdManageRoles"]);
            var permNameManageRoles = permissions["permNameManageRoles"];
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permIdManageRoles,
                    PermissionName = permNameManageRoles,
                    IsSystemDefined = true
                }
            );
            // Permission : CanManagePermissions
            var permIdManagePermissions = Guid.Parse(permissions["permIdManagePermissions"]);
            var permNameManagePermissions = permissions["permNameManagePermissions"];
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permIdManagePermissions,
                    PermissionName = permNameManagePermissions,
                    IsSystemDefined = true
                }
            );
            // Permission : CanManageAssigns
            var permIdManageAssigns = Guid.Parse(permissions["permIdManageAssigns"]);
            var permNameManageAssigns = permissions["permNameManageAssigns"];
            //Admin's Pemission Assignment
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    PermissionId = permIdManageAssigns,
                    PermissionName = permNameManageAssigns,
                    IsSystemDefined = true
                }
            );
            #endregion
        }

    }
}
