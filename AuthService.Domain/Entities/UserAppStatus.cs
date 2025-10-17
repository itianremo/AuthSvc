using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class UserAppStatus
    {
        public Guid UserAppStatusId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }
        public string Status { get; set; } // e.g., "active", "blocked", "pending-approval"
        public User User { get; set; }
        
    }
}
