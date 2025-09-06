using Dbets.Domain.Common;

namespace Dbets.Domain.Events.Users;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    DateTime RegisteredAt
) : DomainEvent;