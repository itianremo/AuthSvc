using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuthService.Domain.Interfaces;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UserAppStatusRepository : Repository<UserAppStatus>, IUserAppStatusRepository
    {
        public UserAppStatusRepository(AuthDbContext context) : base(context) { }
    }
}
