using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserAppBiz : Biz<UserApp>, IUserAppBiz
    {
        public UserAppBiz(IServiceProvider provider) : base(provider) { }

        public Task<UserApp?> GetByRefreshTokenAsync(string refreshToken)
        {
            return UserAppRepository.GetByRefreshTokenAsync(refreshToken);
        }
    }
}
