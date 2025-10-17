using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class RefreshTokenDto
    {
        public Guid AppId { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }

        //public string DeviceId { get; set; }
        //public string[] LongLat { get; set; }
    }
}
