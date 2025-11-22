using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Configs
{
    public enum AppAccountStatus
    {
        NeedEmailVerify,
        NeedPhoneVerify,
        Pending,
        Suspended,
        Deleted,
        Approved,
        Active
    }

}
