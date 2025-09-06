using Dbets.Domain.Common;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Aggregates;

public class Market : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public bool Active { get; private set; }
    
    private Market() { }

    public override void Validate(IValidationHandler handler) 
    {
        // if(string.IsNullOrWhiteSpace(Name))
        //     handler.AddError("'Name' cannot be empty.");
    }
}