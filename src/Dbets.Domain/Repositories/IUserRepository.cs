using Dbets.Domain.Aggregates;
using Dbets.Domain.Entities;

namespace Dbets.Domain.Repositories;

/// <summary>
/// Interface para repositório de usuários
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailConfirmation?> GetEmailConfirmationByTokenAsync(Guid token, CancellationToken cancellationToken = default);
    Task MarkEmailConfirmationAsUsedAsync(Guid token, CancellationToken cancellationToken = default);
    Task CreateEmailConfirmationAsync(Guid userId, Guid token, DateTime expiresAt, CancellationToken cancellationToken = default);
}