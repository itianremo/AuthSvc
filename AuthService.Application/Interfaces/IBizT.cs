using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    /// <summary>
    /// Base Biz interface providing unit of work contract.
    /// </summary>
    public interface IBiz<T> where T : class
    {
        /// <summary>
        /// Persist changes to the database via UnitOfWork.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute an action inside a database transaction.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
