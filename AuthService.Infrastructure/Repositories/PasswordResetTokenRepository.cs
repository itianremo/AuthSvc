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
    public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(AuthDbContext context) : base(context) { }

        /// <summary>
        /// Retrieve a token by its string value, including the associated user.
        /// </summary>
        public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all expired tokens before a cutoff date.
        /// </summary>
        public async Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(t => t.Expiry < cutoff).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Remove all expired tokens before a cutoff date.
        /// </summary>
        public async Task RemoveExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            var expired = await _dbSet.Where(t => t.Expiry < cutoff).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(expired);
        }

        /// <summary>
        /// Remove all tokens for a given user.
        /// </summary>
        public async Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbSet.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(tokens);
        }

        /// <summary>
        /// Create a new password reset token for a user.
        /// </summary>
        public async Task<PasswordResetToken> CreateTokenAsync(Guid userId, string token, DateTime expiry, CancellationToken cancellationToken = default)
        {
            var prt = new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                Expiry = expiry
            };

            await _context.PasswordResetTokens.AddAsync(prt, cancellationToken);
            return prt;
        }

        /// <summary>
        /// Invalidate (remove) a token by its string value.
        /// </summary>
        public async Task InvalidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var prt = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
            if (prt != null)
            {
                _context.PasswordResetTokens.Remove(prt);
            }
        }

        // === New helper ===

        /// <summary>
        /// Retrieve all active (non-expired) tokens for a given user.
        /// Useful for OTP flows.
        /// </summary>
        public async Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.UserId == userId && t.Expiry > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}
