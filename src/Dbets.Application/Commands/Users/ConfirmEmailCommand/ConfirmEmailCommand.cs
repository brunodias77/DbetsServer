using Dbets.Domain.Mediator;

namespace Dbets.Application.Commands.Users.ConfirmEmailCommand;

public record ConfirmEmailCommand(
    Guid Token
) : IRequest<ConfirmEmailResult>;

public record ConfirmEmailResult(
    bool Success,
    string Message,
    Guid? UserId = null
);