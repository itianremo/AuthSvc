using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserAppStatus> UserAppStatuses { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Unique Indexes ===
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();
            modelBuilder.Entity<Permission>().HasIndex(p => p.PermissionName).IsUnique();
            modelBuilder.Entity<App>().HasIndex(a => a.AppName).IsUnique();

            // === Relationships ===
            modelBuilder.Entity<App>()
                .HasMany(a => a.Roles)
                .WithOne(r => r.App)
                .HasForeignKey(r => r.AppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<App>()
                .HasMany(a => a.UserStatuses)
                .WithOne(ua => ua.App)
                .HasForeignKey(ua => ua.AppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    ur => ur.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    ur => ur.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    ur => ur.HasKey("UserId", "RoleId")
                );

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermissions",
                    rp => rp.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                    rp => rp.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    rp => rp.HasKey("RoleId", "PermissionId")
                );

            modelBuilder.Entity<User>()
                .HasMany(u => u.AppStatuses)
                .WithOne(ua => ua.User)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany<PasswordResetToken>()
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAppStatus>().HasKey(uas => uas.UserAppId);
            modelBuilder.Entity<UserAppStatus>()
                .HasIndex(uas => new { uas.UserId, uas.AppId })
                .IsUnique();

            // === NEW: Enum conversion for UserAppStatus.Status ===
            modelBuilder.Entity<UserAppStatus>()
                .Property(uas => uas.Status)
                .HasConversion<string>();

            // === Static baseline seed (hardcoded literals only) ===
            var appId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");
            var roleId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");
            var userId = Guid.Parse("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f");
            var permissionId = Guid.Parse("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c");
            var userAppStatusId = Guid.Parse("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a");

            modelBuilder.Entity<App>().HasData(new App
            {
                AppId = appId,
                AppName = "Global Admin App",
                RedirectUrls = "http://localhost:3000/auth/callback",
                Scopes = "openid profile email admin",
                AutoApproveUsers = false,
                IsCoreApp = true
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = userId,
                Email = "admin@authsvc.local",
                PhoneNumber = "+201144664441",
                HashedPassword = "AQAAAAIAAYagAAAAEOs3vT6YZ5lIVhQRS7xFq17KvA/u2g7qv+9s0MH2FXyXdvsHLg0rMqwL9KeGKXOvdw==",
                CreatedAt = new DateTime(2024, 1, 1),
                UpdatedAt = new DateTime(2024, 1, 1),
                IsEmailVerified = true,
                IsPhoneVerified = true,
                IsDeleted = false
            });

            modelBuilder.Entity<Role>().HasData(new Role
            {
                RoleId = roleId,
                RoleName = "Root Admin",
                IsSystemDefined = true,
                AppId = appId
            });

            modelBuilder.Entity<Permission>().HasData(new Permission
            {
                PermissionId = permissionId,
                PermissionName = "SuperAccess",
                IsSystemDefined = true
            });

            modelBuilder.Entity<UserAppStatus>().HasData(new UserAppStatus
            {
                UserAppId = userAppStatusId,
                UserId = userId,
                AppId = appId,
                Status = AppAccountStatus.Active,
                RefreshToken = "",
                RefreshTokenExpiry = null,
                LastLogin = null
            });

            modelBuilder.Entity("UserRoles").HasData(new { UserId = userId, RoleId = roleId });
            modelBuilder.Entity("RolePermissions").HasData(new { RoleId = roleId, PermissionId = permissionId });
        }


    }
}
