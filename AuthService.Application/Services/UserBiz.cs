using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserBiz : Biz<User>, IUserBiz
    {
        public UserBiz(IServiceProvider provider) : base(provider) { }

        public Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            // delegating to repository convenience method
            return UserRepository.GetByEmailOrPhoneAsync(emailOrPhone);
        }

        // Add more user-specific business methods that use UserRepository, UserAppRepository etc.
    }
}
