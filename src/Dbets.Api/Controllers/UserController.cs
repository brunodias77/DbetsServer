using System.Security.Claims;
using Dbets.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dbets.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize] // Requer autenticação para todas as rotas deste controller
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil do usuário autenticado
    /// </summary>
    /// <returns>Dados do perfil do usuário</returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            // Extrair o ID do usuário do token JWT
            var userIdClaim = User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Token JWT não contém user_id válido");
                return Unauthorized(new { message = "Token inválido" });
            }

            // Buscar o usuário no banco de dados
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado: {UserId}", userId);
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Retornar dados do perfil (sem informações sensíveis)
            var profile = new
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                EmailConfirmed = user.EmailConfirmed,
                Active = user.Active,
                Theme = user.Theme.ToString().ToLower(),
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };

            _logger.LogInformation("Perfil acessado pelo usuário: {UserId}", userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar perfil do usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza o perfil do usuário autenticado
    /// </summary>
    /// <param name="request">Dados para atualização</param>
    /// <returns>Perfil atualizado</returns>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Extrair o ID do usuário do token JWT
            var userIdClaim = User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            // Buscar o usuário no banco de dados
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Atualizar o perfil
            user.UpdateProfile(request.Name, request.Phone);
            await _userRepository.UpdateAsync(user);

            // Retornar perfil atualizado
            var updatedProfile = new
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                EmailConfirmed = user.EmailConfirmed,
                Active = user.Active,
                Theme = user.Theme.ToString().ToLower(),
                UpdatedAt = user.UpdatedAt
            };

            _logger.LogInformation("Perfil atualizado pelo usuário: {UserId}", userId);
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil do usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}

/// <summary>
/// Modelo para atualização de perfil
/// </summary>
public record UpdateProfileRequest(
    string Name,
    string? Phone = null
);