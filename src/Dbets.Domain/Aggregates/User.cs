using System.Collections.ObjectModel;
using Dbets.Domain.Common;
using Dbets.Domain.Entities;
using Dbets.Domain.Enums;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Aggregates;

public class User : AggregateRoot
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? Phone { get; private set; }
    public string? ProfilePicture { get; private set; }
    public bool Active { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public int LoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public Theme Theme { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    
    // Construtor privado para uso do ORM e do Factory Method.
    private User() : base() { } 

    // Factory Method para garantir a criação de um objeto válido.
    public static User Create(string name, string email, string passwordHash)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Active = true,
            EmailConfirmed = false,
            LoginAttempts = 0,
            Theme = Theme.Light
        };

        user.RaiseEvent(new UserCreatedEvent(user.Id, user.Name, user.Email));
        return user;
    }
    
    public void UpdateProfile(string name, string? phone) 
    {
        Name = name;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        MarkAsDeleted();
    }

    public override void Validate(IValidationHandler handler)
    {
        // // Exemplo: new UserValidator().Validate(this, handler);
        // if (string.IsNullOrWhiteSpace(Name))
        //     handler.AddError("'Name' must not be empty.");
        // if (string.IsNullOrWhiteSpace(Email))
        //     handler.AddError("'Email' must not be empty.");
    }
}

public record UserCreatedEvent(Guid UserId, string Name, string Email) : DomainEvent;
