using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuthService.Domain.Interfaces;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AuthDbContext context) : base(context) { }
    }
}
