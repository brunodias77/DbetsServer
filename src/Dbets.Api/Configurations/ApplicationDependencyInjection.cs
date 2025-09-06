using Dbets.Infrastructure.Mediator;
using Dbets.Infrastructure.Mediator.Behaviors;

namespace Dbets.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddMediator(services);
    }
    
    private static void AddMediator(IServiceCollection services)
    {
        // Register our custom Mediator implementation
        services.AddMediator(
            typeof(Dbets.Application.Commands.Users.CreateUserCommand.CreateUserCommand).Assembly, // Application assembly
            typeof(Dbets.Domain.Mediator.IMediator).Assembly, // Domain assembly
            typeof(Dbets.Infrastructure.Mediator.Mediator).Assembly // Infrastructure assembly
        );
    }
}