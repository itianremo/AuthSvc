using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class App
    {
        public Guid AppId { get; set; } = Guid.NewGuid();
        [Required]
        public string AppName { get; set; }
        public string RedirectUrls { get; set; }
        public string Scopes { get; set; }
        public bool AutoApproveUsers { get; set; } = false;
        public bool IsCoreApp { get; set; } = false;

        public ICollection<UserAppStatus> UserStatuses { get; set; }
        public ICollection<Role> Roles { get; set; }

        public App()
        {
            UserStatuses = new List<UserAppStatus>();
            Roles = new List<Role>();

        }
    }

}
