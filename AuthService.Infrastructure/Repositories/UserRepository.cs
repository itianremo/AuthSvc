using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AuthDbContext context) : base(context) { }

        public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .Include(u => u.AppStatuses)
                .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.PhoneNumber == emailOrPhone, cancellationToken);
        }

        public async Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.AppStatuses.Any(ua => ua.AppId == appId))
                .CountAsync(cancellationToken);
        }

        public async Task<int> CountByStatusAsync(AppAccountStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.AppStatuses.Any(ua => ua.Status == status))
                .CountAsync(cancellationToken);
        }

        public async Task<User?> GetWithRolesAndStatusesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .Include(u => u.AppStatuses)
                .ThenInclude(ua => ua.App)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

        public async Task<int> CountVerifiedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.IsEmailVerified && u.IsPhoneVerified)
                .CountAsync(cancellationToken);
        }

        public async Task<int> CountUnverifiedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => !u.IsEmailVerified || !u.IsPhoneVerified)
                .CountAsync(cancellationToken);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles) // navigation property
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

    }
}
