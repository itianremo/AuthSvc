using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        /// <summary>
        /// Create a new password reset token for a user.
        /// </summary>
        Task<PasswordResetToken> CreateTokenAsync(Guid userId, string token, DateTime expiry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve a token by its string value, including the associated user.
        /// </summary>
        Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate (remove) a token by its string value.
        /// </summary>
        Task InvalidateTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all expired tokens before a cutoff date.
        /// </summary>
        Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove all expired tokens before a cutoff date.
        /// </summary>
        Task RemoveExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove all tokens for a given user.
        /// </summary>
        Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all active (non-expired) tokens for a given user.
        /// Useful for OTP flows.
        /// </summary>
        Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
