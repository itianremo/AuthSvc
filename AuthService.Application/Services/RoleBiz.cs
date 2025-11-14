using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class RoleBiz : Biz<Role>, IRoleBiz
    {
        public RoleBiz(IServiceProvider provider) : base(provider) { }

        public Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId)
        {
            return RoleRepository.GetRoleNamesByUserIdAsync(userId);
        }
    }
}
