using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs.Notifications
{
    public class SmsMessageDto
    {
        public string ToPhoneE164 { get; set; } = string.Empty;
        public string BodyText { get; set; } = string.Empty;
        public IDictionary<string, string>? Metadata { get; set; }
        public string? TemplateKey { get; set; }
    }
}
