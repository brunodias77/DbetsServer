using Dbets.Application.Commands.Users.ConfirmEmailCommand;
using Dbets.Application.Commands.Users.CreateUserCommand;
using Dbets.Application.Commands.Users.LoginUserCommand;
using Dbets.Domain.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Dbets.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    [HttpPost("create-user")]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o registro do usuário");
            return BadRequest(new { message = "O registro falhou", error = ex.Message });
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a confirmação do email");
            return BadRequest(new { message = "A confirmação falhou", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return Unauthorized(new { message = "Login failed", error = ex.Message });
        }
    }
}