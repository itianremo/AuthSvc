using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class LoginDto
    {
        public string EmailOrPhone { get; set; }
        public string Password { get; set; }
        public Guid AppId { get; set; }
        //public string DeviceId { get; set; }
        //public string[] LongLat { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}
