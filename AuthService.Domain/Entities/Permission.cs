using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class Permission
    {
        public Guid PermissionId { get; set; } = Guid.NewGuid();
        public string PermissionName { get; set; }
        public bool IsSystemDefined { get; set; } = false;

        public ICollection<Role> Roles { get; set; }
        //public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public Permission()
        {
            Roles = new HashSet<Role>();
        }

    }

}
