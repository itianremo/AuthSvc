using AuthService.Application.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IDashboardBiz
    {
        Task<DashboardMetricsDto> GetMetricsAsync(Guid appId, CancellationToken cancellationToken = default);
    }
}
