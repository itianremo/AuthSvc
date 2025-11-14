using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class RolePermissionDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
    }

    public class RolePermissionIdsDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }
}
