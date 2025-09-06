using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Entities;

public class PasswordReset : Entity
{
    public Guid UserId { get; private set; }
    public Guid Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Used { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public string? IpAddress { get; private set; }
    
    private PasswordReset() { }

    public override void Validate(IValidationHandler handler) { /* Implementar validação */ }
}