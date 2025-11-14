using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class RoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid AppId { get; set; }
        public string AppName { get; set; }
        public List<string> Permissions { get; set; }
        public bool IsSystemDefined { get; set; } = false;
    }

    public class CreateRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid AppId { get; set; }
    }

    public class UpdateRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        //public Guid AppId { get; set; }
    }

}
