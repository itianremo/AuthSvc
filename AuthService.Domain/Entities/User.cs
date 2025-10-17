using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{

    public class User
    {
        public Guid UserId { get; set; }
        [Required, MaxLength(256)]
        public string Email { get; set; }
        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        public string HashedPassword { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserApp> UserApps { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }

        public string GlobalAccountStatus { get; set; } // e.g., "active", "blocked", "email-verification-needed"
        public ICollection<UserAppStatus> AppStatuses { get; set; } // Per-app status

    }
}
