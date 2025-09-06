using Dbets.Domain.Mediator;

namespace Dbets.Application.Commands.Users.LoginUserCommand;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResult>;

public record LoginUserResult(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    Guid? UserId = null,
    string? UserName = null,
    string? UserEmail = null,
    bool? EmailConfirmed = null
);