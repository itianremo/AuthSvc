using AuthService.Application.DTOs;
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

    public static class StatusHelper
    {
        public static bool IsLoginAllowed(string status)
        {
            return status == AccountStatus.Active
                || status == AccountStatus.Approved;
        }

        public static string GetDisplayStatus(User user, App app)
        {
            if (user.IsDeleted) return AccountStatus.Deleted;
            if (!user.IsEmailVerified) return AccountStatus.NeedEmailVerify;
            if (!user.IsPhoneVerified) return AccountStatus.NeedPhoneVerify;



            return user.GlobalAccountStatus.ToString();
        }
    }


}
