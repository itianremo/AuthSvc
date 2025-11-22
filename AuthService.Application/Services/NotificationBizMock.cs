using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Notifications;
using AuthService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    // Mock implementation until the real microservice is attached
    public class NotificationBizMock : INotificationBiz
    {
        public Task<NotificationResultDto> SendEmailAsync(EmailMessageDto email, CancellationToken cancellationToken = default)
        {
            // Simulate success and generate a mock provider message id
            var result = new NotificationResultDto
            {
                Success = true,
                ProviderMessageId = $"mock-email-{Guid.NewGuid():N}"
            };
            return Task.FromResult(result);
        }

        public Task<NotificationResultDto> SendSmsAsync(SmsMessageDto sms, CancellationToken cancellationToken = default)
        {
            var result = new NotificationResultDto
            {
                Success = true,
                ProviderMessageId = $"mock-sms-{Guid.NewGuid():N}"
            };
            return Task.FromResult(result);
        }
    }
}
