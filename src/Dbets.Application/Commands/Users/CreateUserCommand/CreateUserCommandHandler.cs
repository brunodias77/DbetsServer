using Dbets.Domain.Aggregates;
using Dbets.Domain.Common;
using Dbets.Domain.Events.Users;
using Dbets.Domain.Mediator;
using Dbets.Domain.Repositories;
using Dbets.Domain.Services;
using Dbets.Domain.Validations;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.Commands.Users.CreateUserCommand;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o registro do usuário para e-mail: {Email}", request.Email);
        
        try
        {
            // 1. Validate the command
            await ValidateCommand(request, cancellationToken);
            
            // 2. Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Usuário com e-mail {request.Email} já existe.");
            }
            
            // 3. Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            // 4. Hash the password
            var passwordHash = _passwordHasher.HashPassword(request.Password);
            
            // 5. Create the user aggregate
            var user = User.Create(request.Name, request.Email, passwordHash);
            
            // 6. Update profile if phone is provided
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                user.UpdateProfile(request.Name, request.Phone);
            }
            
            // 7. Save to repository
            var userId = await _userRepository.CreateAsync(user, cancellationToken);
            
            // 8. Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            // 9. Dispatch domain events
            await _publisher.Publish(new UserRegisteredEvent(userId, user.Email, user.Name, DateTime.UtcNow), cancellationToken);  
            
            _logger.LogInformation("Usuário criado com sucesso. ID: {UserId}, Email: {Email}", userId, request.Email);
            
            return new CreateUserResult(
                userId,
                user.Name,
                user.Email,
                user.EmailConfirmed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário com e-mail: {Email}", request.Email);
            
            // Rollback transaction if it was started
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação");
            }
            
            throw;
        }
    }

    private async Task ValidateCommand(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var notification = new Notification();
        
        // Validações básicas
        if (string.IsNullOrWhiteSpace(command.Name))
            notification.Add(new Error("INVALID_NAME", "Nome é obrigatório."));
            
        if (string.IsNullOrWhiteSpace(command.Email))
            notification.Add(new Error("INVALID_EMAIL", "E-mail é obrigatório."));
        else if (!IsValidEmail(command.Email))
            notification.Add(new Error("INVALID_EMAIL_FORMAT", "Formato de e-mail inválido."));
            
        if (string.IsNullOrWhiteSpace(command.Password))
            notification.Add(new Error("INVALID_PASSWORD", "Senha é obrigatória."));
        else if (command.Password.Length < 6)
            notification.Add(new Error("WEAK_PASSWORD", "Senha deve ter pelo menos 6 caracteres."));
            
        if (command.TimezoneId == Guid.Empty)
            notification.Add(new Error("INVALID_TIMEZONE", "Timezone é obrigatório."));
            
        if (command.CurrencyId == Guid.Empty)
            notification.Add(new Error("INVALID_CURRENCY", "Moeda é obrigatória."));
        
        if (notification.HasErrors)
        {
            var errors = string.Join(", ", notification.Errors.Select(e => e.Message));
            throw new ValidationException($"Dados inválidos: {errors}");
        }
        
        await Task.CompletedTask;
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
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}