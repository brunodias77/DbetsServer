using Dbets.Domain.Mediator;

namespace Dbets.Application.Commands.Users.CreateUserCommand;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    Guid TimezoneId,
    Guid CurrencyId,
    string? Phone = null
) : IRequest<CreateUserResult>;

public record CreateUserResult(
    Guid UserId,
    string Name,
    string Email,
    bool EmailConfirmed
);