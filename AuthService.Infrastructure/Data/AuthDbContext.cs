using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<UserApp> UserApps { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAppStatus> UserAppStatuses { get; set; }

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

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.App)
                .WithMany(a => a.UserRoles)
                .HasForeignKey(ur => ur.AppId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Seed Data ===
            #region Seed Data
            var appId = Guid.Parse("28db6987-8066-438e-bb3c-53c6b6515c6c");

            var roleAdmin = Guid.Parse("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc");
            var roleEditor = Guid.Parse("a1b2c3d4-5678-90ab-cdef-112233445566");

            var permCreate = Guid.Parse("11111111-aaaa-bbbb-cccc-111111111111");
            var permEdit = Guid.Parse("22222222-aaaa-bbbb-cccc-222222222222");
            var permDelete = Guid.Parse("33333333-aaaa-bbbb-cccc-333333333333");

            var rp1 = Guid.Parse("e1a1f8d2-3b4c-4f6a-9a7b-1c2d3e4f5a6b");
            var rp2 = Guid.Parse("f2b2e9c3-4d5e-5a7b-8c9d-2e3f4a5b6c7d");
            var rp3 = Guid.Parse("a3c3d0e4-5f6a-6b8c-9d0e-3f4a5b6c7d8e");
            var rp4 = Guid.Parse("b4d4e1f5-6a7b-7c9d-0e1f-4a5b6c7d8e9f");
            var rp5 = Guid.Parse("c5e5f2a6-7b8c-8d0e-1f2a-5b6c7d8e9f0a");

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = roleAdmin, RoleName = "Admin", AppId = appId },
                new Role { RoleId = roleEditor, RoleName = "Editor", AppId = appId }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { PermissionId = permCreate, PermissionName = "Create" },
                new Permission { PermissionId = permEdit, PermissionName = "Edit" },
                new Permission { PermissionId = permDelete, PermissionName = "Delete" }
            );

            modelBuilder.Entity<RolePermission>().HasData(
                // Admin gets all
                new RolePermission { RolePermissionId = rp1, RoleId = roleAdmin, PermissionId = permCreate },
                new RolePermission { RolePermissionId = rp2, RoleId = roleAdmin, PermissionId = permEdit },
                new RolePermission { RolePermissionId = rp3, RoleId = roleAdmin, PermissionId = permDelete },

                // Editor gets Create + Edit
                new RolePermission { RolePermissionId = rp4, RoleId = roleEditor, PermissionId = permCreate },
                new RolePermission { RolePermissionId = rp5, RoleId = roleEditor, PermissionId = permEdit }
            );
            #endregion
        }

    }
}
