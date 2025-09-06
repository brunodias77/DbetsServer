using Dbets.Domain.Common;
using Dbets.Domain.Mediator;
using Dbets.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.Commands.Users.ConfirmEmailCommand;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando confirmação de email para token: {Token}", request.Token);
        
        try
        {
            // 1. Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            // 2. Buscar confirmação de email pelo token
            var emailConfirmation = await _userRepository.GetEmailConfirmationByTokenAsync(request.Token, cancellationToken);
            
            if (emailConfirmation == null)
            {
                _logger.LogWarning("Token de confirmação não encontrado: {Token}", request.Token);
                return new ConfirmEmailResult(false, "Token de confirmação inválido.");
            }
            
            // 3. Verificar se já foi confirmado
            if (emailConfirmation.Confirmed)
            {
                _logger.LogWarning("Email já confirmado para token: {Token}", request.Token);
                return new ConfirmEmailResult(false, "Email já foi confirmado anteriormente.");
            }
            
            // 4. Verificar se o token expirou
            if (emailConfirmation.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Token de confirmação expirado: {Token}", request.Token);
                return new ConfirmEmailResult(false, "Token de confirmação expirado.");
            }
            
            // 5. Buscar o usuário
            var user = await _userRepository.GetByIdAsync(emailConfirmation.UserId, cancellationToken);
            
            if (user == null)
            {
                _logger.LogError("Usuário não encontrado para confirmação: {UserId}", emailConfirmation.UserId);
                return new ConfirmEmailResult(false, "Usuário não encontrado.");
            }
            
            // 6. Confirmar email do usuário
            user.ConfirmEmail();
            
            // 7. Atualizar usuário no repositório
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            // 8. Marcar confirmação como usada
            await _userRepository.MarkEmailConfirmationAsUsedAsync(request.Token, cancellationToken);
            
            // 9. Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            _logger.LogInformation("Email confirmado com sucesso para usuário: {UserId}", user.Id);
            
            return new ConfirmEmailResult(true, "Email confirmado com sucesso!", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar email para token: {Token}", request.Token);
            
            // Rollback transaction if it was started
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação");
            }
            
            return new ConfirmEmailResult(false, "Erro interno do servidor.");
        }
    }
}