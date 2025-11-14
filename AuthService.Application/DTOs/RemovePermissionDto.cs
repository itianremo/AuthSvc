using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class RemovePermissionDto
    {
        public Guid RoleId { get; set; }
        public string PermissionName { get; set; }
    }

}
