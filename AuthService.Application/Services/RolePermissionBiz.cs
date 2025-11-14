using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class RolePermissionBiz : Biz<RolePermission>, IRolePermissionBiz
    {
        public RolePermissionBiz(IServiceProvider provider) : base(provider) { }

        // Add role-permission-specific logic as needed
    }
}
