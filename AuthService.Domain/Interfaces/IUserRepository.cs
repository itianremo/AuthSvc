using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone);
        // add other user-specific signatures you need (e.g., GetWithRolesAsync)
    }
}
