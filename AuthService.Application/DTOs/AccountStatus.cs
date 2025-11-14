using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public static class AccountStatus
    {
        public const string Active = "active";
        public const string Suspended = "suspended";
        public const string Pending = "pending";
        public const string NeedEmailVerify = "need email verify";
        public const string NeedPhoneVerify = "need phone verify";
        public const string Deleted = "deleted";
        public const string Approved = "approved";
    }
}
