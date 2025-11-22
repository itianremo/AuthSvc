using AuthService.Application.DTOs.Notifications;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class PasswordResetTokenBiz : Biz<PasswordResetToken>, IPasswordResetTokenBiz
    {
        private readonly INotificationBiz _notificationBiz;
        private readonly IPasswordHasher<User> _hasher;

        public PasswordResetTokenBiz(
            IServiceProvider provider,
            IUnitOfWork unitOfWork,
            INotificationBiz notificationBiz,
            IPasswordHasher<User> hasher)
            : base(provider, unitOfWork)
        {
            _notificationBiz = notificationBiz;
            _hasher = hasher;
        }

        // === Retrieval ===
        public Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
            => PasswordResetTokenRepository.GetByTokenAsync(token, cancellationToken);

        public Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default)
            => PasswordResetTokenRepository.GetExpiredTokensAsync(cutoff, cancellationToken);

        public Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => PasswordResetTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);

        // === Creation ===
        public async Task<PasswordResetToken> CreateTokenAsync(Guid userId, string tokenValue, DateTime expiry, CancellationToken cancellationToken = default)
        {
            var token = await PasswordResetTokenRepository.CreateTokenAsync(userId, tokenValue, expiry, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return token;
        }

        // === Invalidation ===
        public async Task<bool> InvalidateTokenAsync(string tokenValue, CancellationToken cancellationToken = default)
        {
            await PasswordResetTokenRepository.InvalidateTokenAsync(tokenValue, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Cleanup ===
        public async Task<int> RemoveExpiredTokensAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            await PasswordResetTokenRepository.RemoveExpiredTokensAsync(cutoff, cancellationToken);
            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<int> RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await PasswordResetTokenRepository.RemoveByUserIdAsync(userId, cancellationToken);
            return await SaveChangesAsync(cancellationToken);
        }

        // === Verification ===
        public async Task<bool> VerifyOtpTokenAsync(Guid userId, string tokenValue, CancellationToken cancellationToken = default)
        {
            var record = await PasswordResetTokenRepository.GetByTokenAsync(tokenValue, cancellationToken)
                ?? throw new InvalidOperationException("Invalid OTP token.");

            if (record.UserId != userId)
                throw new InvalidOperationException("OTP does not belong to this user.");

            if (record.Expiry <= DateTime.UtcNow)
                throw new InvalidOperationException("OTP token has expired.");

            await PasswordResetTokenRepository.InvalidateTokenAsync(tokenValue, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Forgot Password (with notification) ===
        public async Task<string> InitiateForgotPasswordOtpAsync(User user, CancellationToken cancellationToken = default)
        {
            var otpToken = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(10);

            await PasswordResetTokenRepository.CreateTokenAsync(user.UserId, otpToken, expiry, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailDto = new EmailMessageDto
                {
                    To = user.Email,
                    Subject = "Password Reset Code",
                    BodyText = $"Your reset code is {otpToken}. It expires in 10 minutes.",
                    BodyHtml = $"<p>Your reset code is <strong>{otpToken}</strong>.</p><p>It expires in 10 minutes.</p>",
                    TemplateKey = "password_reset_otp"
                };

                var result = await _notificationBiz.SendEmailAsync(emailDto, cancellationToken);
                if (!result.Success)
                    throw new InvalidOperationException($"Failed to send OTP email: {result.ErrorMessage}");
            }
            else if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var smsDto = new SmsMessageDto
                {
                    ToPhoneE164 = user.PhoneNumber,
                    BodyText = $"Your reset code is {otpToken}. It expires in 10 minutes.",
                    TemplateKey = "password_reset_otp"
                };

                var result = await _notificationBiz.SendSmsAsync(smsDto, cancellationToken);
                if (!result.Success)
                    throw new InvalidOperationException($"Failed to send OTP SMS: {result.ErrorMessage}");
            }

            return otpToken;
        }

        public async Task<(bool Success, string Message, string Data)> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByEmailOrPhoneAsync(email, cancellationToken);
            if (user == null)
                return (true, "If email exists, a reset link has been sent.", string.Empty);

            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            await CreateTokenAsync(user.UserId, resetToken, DateTime.UtcNow.AddHours(1), cancellationToken);
            return (true, "Reset token generated.", resetToken);
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
        {
            var tokenEntry = await GetByTokenAsync(token, cancellationToken);
            if (tokenEntry == null || tokenEntry.Expiry < DateTime.UtcNow)
                return (false, "Invalid or expired token.");

            var user = await UserRepository.GetByIdAsync(tokenEntry.UserId, cancellationToken);
            if (user == null)
                return (false, "User not found.");

            user.HashedPassword = _hasher.HashPassword(user, newPassword);
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);

            return (true, "Password reset successful.");
        }
    }
}
