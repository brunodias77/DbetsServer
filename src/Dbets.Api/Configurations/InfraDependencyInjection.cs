using Dbets.Domain.Common;
using Dbets.Domain.Repositories;
using Dbets.Infrastructure.Data;
using Dbets.Infrastructure.Data.Repositories;
using Dbets.Infrastructure.Services;

namespace Dbets.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services);
    }
    
    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        // Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
    }

}