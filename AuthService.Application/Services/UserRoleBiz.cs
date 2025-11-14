using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserRoleBiz : Biz<UserRole>, IUserRoleBiz
    {
        public UserRoleBiz(IServiceProvider provider) : base(provider) { }

        // Add user-role-specific business logic as needed
    }
}
