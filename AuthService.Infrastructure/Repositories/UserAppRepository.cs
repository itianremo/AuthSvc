using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using AuthService.Domain.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UserAppRepository : Repository<UserApp>, IUserAppRepository
    {
        public UserAppRepository(AuthDbContext context) : base(context) { }

        public async Task<UserApp?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet.Include(ua => ua.User).Include(ua => ua.App)
                .FirstOrDefaultAsync(ua => ua.RefreshToken == refreshToken);
        }
    }
}
