using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class PermissionBiz : Biz<Permission>, IPermissionBiz
    {
        public PermissionBiz(IServiceProvider provider) : base(provider) { }

        // Add permission-specific business logic delegating to PermissionRepository if needed
    }
}
