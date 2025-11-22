using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    /// <summary>
    /// Lightweight role info for permission listings.
    /// </summary>
    public class RoleListItemDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSystemDefined { get; set; }
    }

    /// <summary>
    /// Lightweight permission info for listings.
    /// </summary>
    public class PermissionListItemDto
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsSystemDefined { get; set; }
        public List<RoleListItemDto> Roles { get; set; } = new();
    }

    /// <summary>
    /// Detailed permission info for single permission retrieval.
    /// </summary>
    public class PermissionDto
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsSystemDefined { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    /// <summary>
    /// DTO for creating or updating a permission.
    /// </summary>
    public class CreatePermissionDto
    {
        public string PermissionName { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
    }
}
