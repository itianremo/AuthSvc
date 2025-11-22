using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class Role
    {
        public Guid RoleId { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; }
        public bool IsSystemDefined { get; set; } = false;

        public Guid AppId { get; set; } = Guid.NewGuid();
        public App App { get; set; }
        public ICollection<Permission> Permissions { get; set; }
        public ICollection<User> Users { get; set; }

        public Role()
        {
            Permissions = new HashSet<Permission>();
            Users = new HashSet<User>();
        }
    }

}
