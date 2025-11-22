using AuthService.Application.DTOs;
using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using System;
using System.Linq;

namespace AuthService.Application.Helpers
{
    public static class AccountStatusHelper
    {
        public static AppAccountStatus GetAppStatus(User user, Guid appId)
        {
            if (user.IsDeleted)
                return AppAccountStatus.Deleted;

            if (!user.IsEmailVerified)
                return AppAccountStatus.NeedEmailVerify;

            if (!user.IsPhoneVerified)
                return AppAccountStatus.NeedPhoneVerify;

            var appStatus = user.AppStatuses.FirstOrDefault(s => s.AppId == appId);
            return appStatus?.Status ?? AppAccountStatus.Pending;
        }
    }

    public static class StatusHelper
    {
        public static bool IsLoginAllowed(AppAccountStatus status)
        {
            return status == AppAccountStatus.Active
                || status == AppAccountStatus.Approved;
        }

        public static AppAccountStatus GetDisplayStatus(User user, Guid appId)
        {
            if (user.IsDeleted) return AppAccountStatus.Deleted;
            if (!user.IsEmailVerified) return AppAccountStatus.NeedEmailVerify;
            if (!user.IsPhoneVerified) return AppAccountStatus.NeedPhoneVerify;

            var appStatus = user.AppStatuses.FirstOrDefault(s => s.AppId == appId);
            return appStatus?.Status ?? AppAccountStatus.Pending;
        }
    }
}
