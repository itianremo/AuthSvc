using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        // add signatures as needed
    }
}
