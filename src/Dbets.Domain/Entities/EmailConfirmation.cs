using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Entities;

public class EmailConfirmation : Entity
{
    public Guid UserId { get; private set; }
    public Guid Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Confirmed { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    
    private EmailConfirmation() { }

    public override void Validate(IValidationHandler handler) { /* Implementar validação */ }
}