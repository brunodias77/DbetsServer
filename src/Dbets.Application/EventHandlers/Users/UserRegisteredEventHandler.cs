using Dbets.Domain.Events.Users;
using Dbets.Domain.Mediator;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.EventHandlers.Users;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User registered: {UserId} with email {Email}", 
            notification.UserId, 
            notification.Email);

        // Here you could add additional logic such as:
        // - Send welcome email
        // - Create user profile
        // - Initialize user preferences
        // - Send notification to admin
        // - Analytics tracking
        
        await Task.CompletedTask;
    }
}