using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class ReactivateDto
    {
        public Guid UserId { get; set; }
        public string Otp { get; set; }
    }

}
