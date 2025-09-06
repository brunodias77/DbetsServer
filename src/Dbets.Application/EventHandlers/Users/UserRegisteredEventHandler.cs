using Dbets.Domain.Common;
using Dbets.Domain.Events.Users;
using Dbets.Domain.Mediator;
using Dbets.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.EventHandlers.Users;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserRegisteredEventHandler(
        ILogger<UserRegisteredEventHandler> logger,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User registered: {UserId} with email {Email}", 
            notification.UserId, 
            notification.Email);

        try
        {
            // Gerar token de confirmação de email
            var token = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddHours(24); // Token válido por 24 horas
            
            await _userRepository.CreateEmailConfirmationAsync(
                notification.UserId, 
                token, 
                expiresAt, 
                cancellationToken);
            
            _logger.LogInformation("Token de confirmação criado para usuário {UserId}: {Token}", 
                notification.UserId, token);
            
            // Aqui você poderia enviar o email com o token
            // await _emailService.SendConfirmationEmailAsync(notification.Email, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar token de confirmação para usuário {UserId}", 
                notification.UserId);
        }
    }
}