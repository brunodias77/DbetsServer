using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Aggregates;

public class Bookmaker : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }
    public decimal CommissionRate { get; private set; }
    public bool Active { get; private set; }

    private Bookmaker() { }
    
    public static Bookmaker Create(Guid userId, string name, decimal commissionRate = 0.0m)
    {
        return new Bookmaker
        {
            UserId = userId,
            Name = name,
            CommissionRate = commissionRate,
            Active = true
        };
    }

    public override void Validate(IValidationHandler handler) 
    {
        // if(string.IsNullOrWhiteSpace(Name))
        //     handler.AddError("'Name' cannot be empty.");
        // if(CommissionRate < 0 || CommissionRate > 1)
        //     handler.AddError("'CommissionRate' must be between 0 and 1.");
    }
}