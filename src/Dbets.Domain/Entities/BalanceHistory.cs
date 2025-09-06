using Dbets.Domain.Common;
using Dbets.Domain.Enums;
using Dbets.Domain.Validations;
using Dbets.Domain.ValueObjects;

namespace Dbets.Domain.Entities;

public class BalanceHistory : Entity
{
    public Guid UserId { get; private set; }
    public Guid? BetId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public Money Amount { get; private set; }
    public Money BalanceBefore { get; private set; }
    public Money BalanceAfter { get; private set; }
    public string? Description { get; private set; }
    
    private BalanceHistory() { }

    public static BalanceHistory Create(Guid userId, TransactionType type, Money amount, Money balanceBefore, string? description = null, Guid? betId = null)
    {
        return new BalanceHistory
        {
            UserId = userId,
            TransactionType = type,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = new Money(balanceBefore.Amount + amount.Amount), // Lógica simples de cálculo
            Description = description,
            BetId = betId
        };
    }
    
    public override void Validate(IValidationHandler handler)
    {
        
    }
}