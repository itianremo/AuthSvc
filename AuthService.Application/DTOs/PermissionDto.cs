using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class PermissionListItemDto
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool IsSystemDefined { get; set; }
    }

    public class PermissionDto
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool IsSystemDefined { get; set; }
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    public class CreatePermissionDto
    {
        public string PermissionName { get; set; }
        public Guid RoleId { get; set; }
    }
}
