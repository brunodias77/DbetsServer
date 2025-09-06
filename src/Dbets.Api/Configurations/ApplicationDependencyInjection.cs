using Dbets.Infrastructure.Mediator;
using Dbets.Infrastructure.Mediator.Behaviors;

namespace Dbets.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        
    }
    
    private static void AddMediator(IServiceCollection services)
    {
        // Register our custom Mediator implementation
        // services.AddMediator(
        //     typeof(Dbets.Application.Commands.Users.CreateUserCommand).Assembly, // Application assembly
        //     typeof(Dbets.Domain.Common.IMediator).Assembly, // Domain assembly
        //     typeof(Dbets.Infrastructure.Mediator.Mediator).Assembly // Infrastructure assembly
        // );
    }
    
    private static void AddPipelineBehaviors(IServiceCollection services)
    {
        // Register pipeline behaviors in the order they should execute
        // The order matters - behaviors are executed in reverse order of registration
        
        // 1. Exception handling (outermost - catches all exceptions)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        
        // 2. Logging (logs request/response and timing)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        // 3. Performance monitoring
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        // 4. Caching (for queries)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        
        // 5. Validation (validates before processing)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // 6. Transaction management (innermost - wraps actual handler)
        // Note: Only register if you want automatic transaction management
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
    }
}