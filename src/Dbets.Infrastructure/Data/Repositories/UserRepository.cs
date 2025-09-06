using Dapper;
using Dbets.Domain.Aggregates;
using Dbets.Domain.Entities;
using Dbets.Domain.Repositories;

namespace Dbets.Infrastructure.Data.Repositories;

/// <summary>
/// Repositório para operações com usuários usando Dapper
/// </summary>
public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, name, email, password_hash as PasswordHash, phone, 
                   profile_picture as ProfilePicture, active, email_confirmed as EmailConfirmed,
                   login_attempts as LoginAttempts, locked_until as LockedUntil, 
                   last_login as LastLogin, theme, created_at as CreatedAt, 
                   updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM users 
            WHERE id = @Id AND deleted_at IS NULL";

        return await Connection.QueryFirstOrDefaultAsync<User>(
            sql, 
            new { Id = id }, 
            Transaction);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, name, email, password_hash as PasswordHash, phone, 
                   profile_picture as ProfilePicture, active, email_confirmed as EmailConfirmed,
                   login_attempts as LoginAttempts, locked_until as LockedUntil, 
                   last_login as LastLogin, theme, created_at as CreatedAt, 
                   updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM users 
            WHERE email = @Email AND deleted_at IS NULL";

        return await Connection.QueryFirstOrDefaultAsync<User>(
            sql, 
            new { Email = email }, 
            Transaction);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO users (id, name, email, password_hash, phone, profile_picture, 
                             active, email_confirmed, login_attempts, locked_until, 
                             last_login, theme, created_at, updated_at)
            VALUES (@Id, @Name, @Email, @PasswordHash, @Phone, @ProfilePicture, 
                   @Active, @EmailConfirmed, @LoginAttempts, @LockedUntil, 
                   @LastLogin, @Theme, @CreatedAt, @UpdatedAt)
            RETURNING id";
    
        var parameters = new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordHash,
            user.Phone,
            user.ProfilePicture,
            user.Active,
            user.EmailConfirmed,
            user.LoginAttempts,
            user.LockedUntil,
            user.LastLogin,
            Theme = user.Theme.ToString().ToLower(), // Conversão explícita
            user.CreatedAt,
            user.UpdatedAt
        };
    
        var id = await Connection.QuerySingleAsync<Guid>(
            sql, 
            parameters, 
            Transaction);
    
        return id;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE users 
            SET name = @Name, email = @Email, password_hash = @PasswordHash, 
                phone = @Phone, profile_picture = @ProfilePicture, active = @Active, 
                email_confirmed = @EmailConfirmed, login_attempts = @LoginAttempts, 
                locked_until = @LockedUntil, last_login = @LastLogin, 
                theme = @Theme, updated_at = @UpdatedAt
            WHERE id = @Id";
    
        await Connection.ExecuteAsync(
            sql, 
            new {
                user.Id,
                user.Name,
                user.Email,
                user.PasswordHash,
                user.Phone,
                user.ProfilePicture,
                user.Active,
                user.EmailConfirmed,
                user.LoginAttempts,
                user.LockedUntil,
                user.LastLogin,
                Theme = user.Theme.ToString().ToLower(), // Converte para minúscula
                UpdatedAt = DateTime.UtcNow
            }, 
            Transaction);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE users 
            SET deleted_at = @DeletedAt, updated_at = @UpdatedAt
            WHERE id = @Id";

        await Connection.ExecuteAsync(
            sql, 
            new { Id = id, DeletedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, 
            Transaction);
    }

    public async Task<EmailConfirmation?> GetEmailConfirmationByTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, user_id as UserId, token, expires_at as ExpiresAt, 
                   confirmed, confirmed_at as ConfirmedAt, created_at as CreatedAt
            FROM email_confirmations 
            WHERE token = @Token";
    
        return await Connection.QueryFirstOrDefaultAsync<EmailConfirmation>(
            sql, 
            new { Token = token }, 
            Transaction);
    }

    public async Task MarkEmailConfirmationAsUsedAsync(Guid token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE email_confirmations 
            SET confirmed = true, confirmed_at = @ConfirmedAt
            WHERE token = @Token";
    
        await Connection.ExecuteAsync(
            sql, 
            new { Token = token, ConfirmedAt = DateTime.UtcNow }, 
            Transaction);
    }

    public async Task CreateEmailConfirmationAsync(
        Guid userId, 
        Guid token, 
        DateTime expiresAt, 
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO email_confirmations (user_id, token, expires_at, confirmed, created_at)
            VALUES (@UserId, @Token, @ExpiresAt, false, @CreatedAt)";
    
        await Connection.ExecuteAsync(
            sql, 
            new { 
                UserId = userId, 
                Token = token, 
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow 
            }, 
            Transaction);
    }
}