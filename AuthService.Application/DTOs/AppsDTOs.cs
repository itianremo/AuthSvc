using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class CreateAppDto
    {
        public string AppName { get; set; } = string.Empty;
        public string? RedirectUrls { get; set; }
        public string? Scopes { get; set; }
        public bool AutoApproveUsers { get; set; }
    }

    public class UpdateAppDto
    {
        public string AppName { get; set; } = string.Empty;
        public string? RedirectUrls { get; set; }
        public string? Scopes { get; set; }
        public bool AutoApproveUsers { get; set; }
    }

    public class ApproveUserDto
    {
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }
    }

    public class UpdateUserAppStatusDto
    {
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}

