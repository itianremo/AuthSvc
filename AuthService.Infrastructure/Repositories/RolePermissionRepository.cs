using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using AuthService.Domain.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
    {
        public RolePermissionRepository(AuthDbContext context) : base(context) { }
    }
}
