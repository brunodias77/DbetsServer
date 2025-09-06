using Dbets.Domain.Common;
using Dbets.Domain.Mediator;
using Dbets.Domain.Repositories;
using Dbets.Domain.Services;
using Dbets.Domain.Validations;
using Microsoft.Extensions.Logging;

namespace Dbets.Application.Commands.Users.CreateUserCommand;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, IPublisher publisher, ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o registro do usuário para e-mail: {Email}", request.Email);
        
        // 1. Validate the command
        await ValidateCommand(request, cancellationToken);
        // 2. Check if user already exists
        // 3. Hash the password
        // 4. Create the user aggregate
        // 5. Update profile if phone is provided
        // 6. Generate email confirmation token
        // 7. Save to repository within transaction
        // 8. Dispatch domain events
        throw new NotImplementedException();
    }

    private async Task ValidateCommand(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var notification = new Notification();
        
    }

    
    
}