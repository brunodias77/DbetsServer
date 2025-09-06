using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.ValueObjects;

public class Odd(decimal value) : ValueObject
{
    public decimal Value { get; } = value;

    public override bool Equals(ValueObject? other)
        => other is Odd odd && Value == odd.Value;

    protected override int GetCustomHashCode() => Value.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        // if (Value <= 1.0M) // Odds geralmente são maiores que 1.0
        // {
        //     handler.AddError("'Odd Value' must be greater than 1.0.");
        // }
    }
}