using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Aggregates;

public class BetType : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool Active { get; private set; }

    private BetType() { }

    public override void Validate(IValidationHandler handler) 
    {
      
    }
}