using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Revoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private RefreshToken() { }
    
    public override void Validate(IValidationHandler handler) { /* Implementar validação */ }
}