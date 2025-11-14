using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class AppRepository : Repository<App>, IAppRepository
    {
        public AppRepository(AuthDbContext context) : base(context) { }

        public async Task<App?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AppName == name);
        }
    }
}
