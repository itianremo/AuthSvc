using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IUserBiz : IBiz<User>
    {
        // Add user-specific business signatures here
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone);
    }
}
