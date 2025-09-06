using Dbets.Domain.Aggregates;
using Dbets.Domain.Services;
using Dbets.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dbets.Infrastructure.Services;

public class LoggedUser : ILoggedUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public LoggedUser(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public async Task<User> User()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        // Extrair o ID do usuário do token JWT
        var userIdClaim = httpContext.User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token JWT não contém user_id válido");
        }

        // Buscar o usuário no banco de dados
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException($"Usuário não encontrado: {userId}");
        }

        if (!user.Active)
        {
            throw new UnauthorizedAccessException("Conta de usuário inativa");
        }

        return user;
    }
}