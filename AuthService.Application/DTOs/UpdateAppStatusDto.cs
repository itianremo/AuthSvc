using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class UpdateAppStatusDto
    {
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }
        public string NewStatus { get; set; }
    }

}
