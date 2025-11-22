using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    /// <summary>
    /// Detailed role info for listings and single role retrieval.
    /// </summary>
    public class RoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Guid AppId { get; set; }
        public string AppName { get; set; } = string.Empty;
        public bool IsSystemDefined { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a new role.
    /// </summary>
    public class CreateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public Guid AppId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing role.
    /// </summary>
    public class UpdateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }
}
