using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _context;

        public IUserRepository Users => new UserRepository(_context);
        public IRoleRepository Roles => new RoleRepository(_context);
        public IPermissionRepository Permissions => new PermissionRepository(_context);
        public IAppRepository Apps => new AppRepository(_context);
        public IUserAppStatusRepository UserAppStatuses => new UserAppStatusRepository(_context);
        public IPasswordResetTokenRepository PasswordResetTokens => new PasswordResetTokenRepository(_context);
        public IRolePermissionRepository RolePermissions => new RolePermissionRepository(_context);
        public IUserRoleRepository UserRoles => new UserRoleRepository(_context);

        public UnitOfWork(AuthDbContext context) => _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default)
        {
            await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await action();
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await action();
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
