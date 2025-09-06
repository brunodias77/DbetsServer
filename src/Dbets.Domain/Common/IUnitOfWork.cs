using System.Data;

namespace Dbets.Domain.Common;

public interface IUnitOfWork
{
    /// <summary>
    /// Gets the active database connection
    /// </summary>
    IDbConnection Connection { get; }
    
    /// <summary>
    /// Gets the active transaction (if any)
    /// </summary>
    IDbTransaction? Transaction { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}