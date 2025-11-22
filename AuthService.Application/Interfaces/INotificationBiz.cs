using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface INotificationBiz
    {
        Task<NotificationResultDto> SendEmailAsync(EmailMessageDto email, CancellationToken cancellationToken = default);
        Task<NotificationResultDto> SendSmsAsync(SmsMessageDto sms, CancellationToken cancellationToken = default);
    }
}
