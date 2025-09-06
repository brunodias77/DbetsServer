using Dbets.Application.Commands.Users.CreateUserCommand;
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
}