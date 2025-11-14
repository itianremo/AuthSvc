using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class Permission
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }

        public bool IsSystemDefined { get; set; } = false;

        public Permission()
        {
            RolePermissions = new HashSet<RolePermission>();
        }

    }

}
