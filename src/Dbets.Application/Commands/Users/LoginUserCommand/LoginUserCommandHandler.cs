using Dbets.Domain.Common;
using Dbets.Domain.Mediator;
using Dbets.Domain.Repositories;
using Dbets.Domain.Services;
using Dbets.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.Commands.Users.LoginUserCommand;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tentativa de login para e-mail: {Email}", request.Email);
        
        try
        {
            // 1. Validar entrada
            var validationResult = ValidateRequest(request);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 2. Buscar usuário por e-mail
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Tentativa de login com e-mail inexistente: {Email}", request.Email);
                return new LoginUserResult(false, "Credenciais inválidas.");
            }

            // 3. Verificar se a conta está ativa
            if (!user.Active)
            {
                _logger.LogWarning("Tentativa de login com conta inativa: {Email}", request.Email);
                return new LoginUserResult(false, "Conta desativada.");
            }

            // 4. Verificar se a conta está bloqueada
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Tentativa de login com conta bloqueada: {Email}", request.Email);
                return new LoginUserResult(false, $"Conta bloqueada até {user.LockedUntil:dd/MM/yyyy HH:mm}.");
            }

            // 5. Verificar senha
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Tentativa de login com senha incorreta: {Email}", request.Email);
                
                // Incrementar tentativas de login (com transação própria)
                await IncrementLoginAttempts(user, cancellationToken);
                
                return new LoginUserResult(false, "Credenciais inválidas.");
            }

            // 6. Iniciar transação para login bem-sucedido
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 7. Reset login attempts on successful login
            await ResetLoginAttempts(user, cancellationToken);

            // 8. Atualizar último login
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            // 9. Commit da transação
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 10. Gerar tokens (após commit para evitar problemas)
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation("Login realizado com sucesso para usuário: {UserId}", user.Id);

            return new LoginUserResult(
                true,
                "Login realizado com sucesso.",
                accessToken,
                refreshToken,
                user.Id,
                user.Name,
                user.Email,
                user.EmailConfirmed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o login para e-mail: {Email}", request.Email);
            
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação");
            }
            
            return new LoginUserResult(false, "Erro interno do servidor.");
        }
    }

    private static LoginUserResult ValidateRequest(LoginUserCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new LoginUserResult(false, "E-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginUserResult(false, "Senha é obrigatória.");
        }

        if (!IsValidEmail(request.Email))
        {
            return new LoginUserResult(false, "Formato de e-mail inválido.");
        }

        return new LoginUserResult(true, "Validação bem-sucedida.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task IncrementLoginAttempts(Domain.Aggregates.User user, CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;
        const int lockoutMinutes = 30;

        try
        {
            // Iniciar transação própria para incrementar tentativas
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            user.IncrementLoginAttempts();

            if (user.LoginAttempts >= maxAttempts)
            {
                user.LockAccount(DateTime.UtcNow.AddMinutes(lockoutMinutes));
                _logger.LogWarning("Conta bloqueada por excesso de tentativas: {Email}", user.Email);
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao incrementar tentativas de login para: {Email}", user.Email);
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback ao incrementar tentativas");
            }
        }
    }

    private async Task ResetLoginAttempts(Domain.Aggregates.User user, CancellationToken cancellationToken)
    {
        if (user.LoginAttempts > 0 || user.LockedUntil.HasValue)
        {
            user.ResetLoginAttempts();
            // Não precisa de commit aqui pois será feito no método principal
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }
}