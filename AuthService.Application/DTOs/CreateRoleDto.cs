using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class CreateRoleDto
    {
        public string RoleName { get; set; }
        public Guid AppId { get; set; }
    }

}
