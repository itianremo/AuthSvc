using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public Guid AppId { get; set; } // App the user is registering for
        //public string DeviceId { get; set; }
        //public string[] LongLat { get; set; }

    }
}
