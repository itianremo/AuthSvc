using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Helpers
{
    public static class AccountStatusHelper
    {
        public static string GetGlobalStatus(User user)
        {
            if (!user.IsEmailVerified)
                return "email-verification-needed";
            if (!user.IsPhoneVerified)
                return "phone-verification-needed";
            return user.GlobalAccountStatus;
        }
    }
}
