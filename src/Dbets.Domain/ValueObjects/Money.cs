using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.ValueObjects;

public class Money(decimal amount) : ValueObject
{
    public decimal Amount { get; } = amount;

    public override bool Equals(ValueObject? other)
        => other is Money money && Amount == money.Amount;

    protected override int GetCustomHashCode() => Amount.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        // if (Amount < 0)
        // {
        //     handler.Add("","'Amount' cannot be negative.");
        // }
    }
}