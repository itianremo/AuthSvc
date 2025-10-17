using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class RolePermission
    {
        public Guid RolePermissionId { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }

}
