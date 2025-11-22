using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IPasswordResetTokenBiz : IBiz<PasswordResetToken>
    {
        // Retrieval
        Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default);
        Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Creation
        Task<PasswordResetToken> CreateTokenAsync(Guid userId, string tokenValue, DateTime expiry, CancellationToken cancellationToken = default);

        // Invalidation
        Task<bool> InvalidateTokenAsync(string tokenValue, CancellationToken cancellationToken = default);

        // Cleanup
        Task<int> RemoveExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default);
        Task<int> RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Verification
        Task<bool> VerifyOtpTokenAsync(Guid userId, string tokenValue, CancellationToken cancellationToken = default);

        // === Auth flows ===
        Task<(bool Success, string Message, string Data)> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    }
}
