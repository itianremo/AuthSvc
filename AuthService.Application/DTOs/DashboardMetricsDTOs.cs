using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class DashboardMetricsDto
    {
        public int UsersTotal { get; set; }
        public int AppsTotal { get; set; }
        public int RolesTotal { get; set; }
        public int PermissionsTotal { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int UnverifiedUsers { get; set; }
        public int SystemRoles { get; set; }
        public int ExpiredPasswordTokens { get; set; }
    }

}
