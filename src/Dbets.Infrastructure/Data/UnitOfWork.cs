using System.Data;
using Dbets.Domain.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Dbets.Infrastructure.Data;

/// <summary>
/// Implementação do padrão Unit of Work usando Dapper
/// Gerencia transações e conexões com o banco de dados PostgreSQL
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWork(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>
    /// Obtém a conexão ativa, criando uma nova se necessário
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }
    }

    /// <summary>
    /// Obtém a transação ativa
    /// </summary>
    public IDbTransaction? Transaction => _transaction;

    /// <summary>
    /// Inicia uma nova transação
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Uma transação já está ativa.");
        }

        _transaction = Connection.BeginTransaction();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Confirma a transação atual
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para confirmar.");
        }

        try
        {
            _transaction.Commit();
            await Task.CompletedTask;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// Desfaz a transação atual
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para desfazer.");
        }

        try
        {
            _transaction.Rollback();
            await Task.CompletedTask;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// Salva as alterações (para compatibilidade com a interface)
    /// Com Dapper, as operações são executadas imediatamente
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Com Dapper, as operações são executadas imediatamente
        // Este método existe para compatibilidade com a interface
        // Retorna 0 indicando que não há alterações pendentes para salvar
        await Task.CompletedTask;
        return 0;
    }

    /// <summary>
    /// Libera os recursos utilizados
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}