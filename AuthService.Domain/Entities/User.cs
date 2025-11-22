using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{

    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        [Required, MaxLength(256)]
        public string Email { get; set; }
        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        public string HashedPassword { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        public ICollection<Role> Roles { get; set; }
        public ICollection<UserAppStatus> AppStatuses { get; set; } // Per-app status

        //public string GlobalAccountStatus { get; set; } // e.g., "active", "blocked", "email-verification-needed"

        //[NotMapped]
        //public string Status
        //{
        //    get
        //    {
        //        if (IsDeleted)
        //            return "Deleted";
        //        if (GlobalAccountStatus == "suspended")
        //            return "Suspended";
        //        if (!IsEmailVerified)
        //            return "Need Email Verify";
        //        if (!IsPhoneVerified)
        //            return "Need Phone Verify";
        //        if (GlobalAccountStatus == "pending")
        //            return "Pending";
        //        return GlobalAccountStatus ?? "N/A";
        //    }
        //}

        public User()
        {
            Roles = new HashSet<Role>();
            AppStatuses = new HashSet<UserAppStatus>();
        }
    }
}
