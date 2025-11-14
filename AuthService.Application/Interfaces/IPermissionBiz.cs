using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IPermissionBiz : IBiz<Permission>
    {
        // Add permission-specific business methods if needed
    }
}
