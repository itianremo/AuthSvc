using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserAppStatusBiz : Biz<UserAppStatus>, IUserAppStatusBiz
    {
        public UserAppStatusBiz(IServiceProvider provider) : base(provider) { }

        // Add user-app-status-specific logic as needed
    }
}
