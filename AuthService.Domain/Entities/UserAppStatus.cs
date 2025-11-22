using AuthService.Domain.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class UserAppStatus
    {
        public Guid UserAppId { get; set; } = Guid.NewGuid();

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime? LastLogin { get; set; } = DateTime.UtcNow;

        // Changed from string to enum
        public AppAccountStatus Status { get; set; }

        public Guid UserId { get; set; } = Guid.Empty;
        public User User { get; set; }
        public Guid AppId { get; set; } = Guid.Empty;
        public App App { get; set; }
    }

}
