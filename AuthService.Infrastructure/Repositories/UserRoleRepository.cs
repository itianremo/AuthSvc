using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AuthDbContext _context;

        public UserRoleRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            if (user != null && role != null && !user.Roles.Contains(role))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            if (user != null)
            {
                var role = user.Roles.FirstOrDefault(r => r.RoleId == roleId);
                if (role != null)
                {
                    user.Roles.Remove(role);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return user?.Roles ?? Enumerable.Empty<Role>();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

            return role?.Users ?? Enumerable.Empty<User>();
        }

        public async Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => r.AppId == appId)
                .SelectMany(r => r.Users)
                .CountAsync(cancellationToken);
        }

    }
}
