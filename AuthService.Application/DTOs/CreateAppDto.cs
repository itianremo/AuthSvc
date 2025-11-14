using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{

    namespace AuthService.Application.DTOs
    {
        public class CreateAppDto
        {
            [Required]
            public string AppName { get; set; }

            public string RedirectUrls { get; set; }

            public string Scopes { get; set; }

            public bool AutoApproveUsers { get; set; } = false;
        }
    }
}
