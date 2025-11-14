using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class AppBiz : Biz<App>, IAppBiz
    {
        public AppBiz(IServiceProvider provider) : base(provider) { }

        public Task<App?> GetByNameAsync(string name)
        {
            return AppRepository.GetByNameAsync(name);
        }
    }
}
