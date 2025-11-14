using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class PasswordResetBiz : Biz<PasswordResetToken>, IPasswordResetBiz
    {
        public PasswordResetBiz(IServiceProvider provider) : base(provider) { }

    }
}
