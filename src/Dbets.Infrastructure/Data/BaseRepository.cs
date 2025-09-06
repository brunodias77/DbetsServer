using System.Data;
using Dbets.Domain.Common;

namespace Dbets.Infrastructure.Data;

/// <summary>
/// Classe base para repositórios que utilizam Dapper
/// Fornece acesso à conexão e transação através do UnitOfWork
/// </summary>
public abstract class BaseRepository : IRepository
{
    protected readonly UnitOfWork _unitOfWork;

    protected BaseRepository(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Conexão do banco de dados
    /// </summary>
    public IDbConnection Connection => _unitOfWork.Connection;

    /// <summary>
    /// Transação ativa (se houver)
    /// </summary>
    public IDbTransaction? Transaction => _unitOfWork.Transaction;
}