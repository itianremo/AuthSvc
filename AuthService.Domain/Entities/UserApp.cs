using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class UserApp
    {
        public Guid UserAppId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }

        public string AccountStatus { get; set; } // active, suspended, etc.
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiry { get; set; }

        public DateTime? LastLogin { get; set; }

        public User User { get; set; }
        public App App { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
