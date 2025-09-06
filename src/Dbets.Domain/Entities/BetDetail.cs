using Dbets.Domain.Common;
using Dbets.Domain.Enums;
using Dbets.Domain.Validations;
using Dbets.Domain.ValueObjects;

namespace Dbets.Domain.Entities;

public class BetDetail : Entity
{
    public Guid BetId { get; private set; }
    public Guid GameId { get; private set; }
    public Guid MarketId { get; private set; }
    public string Selection { get; private set; }
    public Odd Odd { get; private set; }
    public BetDetailResult? Result { get; private set; }

    private BetDetail() { }

    public static BetDetail Create(Guid gameId, Guid marketId, string selection, Odd odd)
    {
        return new BetDetail
        {
            GameId = gameId,
            MarketId = marketId,
            Selection = selection,
            Odd = odd
        };
    }

    public override void Validate(IValidationHandler handler) 
    { 
        // if(string.IsNullOrWhiteSpace(Selection))
        //     handler.AddError("'Selection' cannot be empty.");
        //
        // Odd.Validate(handler);
    }
}