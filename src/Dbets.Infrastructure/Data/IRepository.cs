using System.Data;

namespace Dbets.Infrastructure.Data;

/// <summary>
/// Interface base para repositórios que utilizam Dapper
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Obtém a conexão do banco de dados
    /// </summary>
    IDbConnection Connection { get; }
    
    /// <summary>
    /// Obtém a transação ativa (se houver)
    /// </summary>
    IDbTransaction? Transaction { get; }
}