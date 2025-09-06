using Dbets.Domain.Common;
using Dbets.Domain.Entities;
using Dbets.Domain.Enums;
using Dbets.Domain.Validations;
using Dbets.Domain.ValueObjects;

namespace Dbets.Domain.Aggregates;

public class Bet : AggregateRoot
{
    public Guid UserId { get; private set; }
    public DateTime BetDate { get; private set; }
    public Guid BookmakerId { get; private set; }
    public Guid BetTypeId { get; private set; }
    public Money Stake { get; private set; }
    public Odd TotalOdds { get; private set; }
    public BetStatus BetStatus { get; private set; }
    public Money? ReturnValue { get; private set; }
    public Money? GrossProfit { get; private set; }
    public Money? NetProfit { get; private set; }
    public decimal? ProfitUnits { get; private set; }
    public Money? Commission { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? SettledAt { get; private set; }

    private readonly List<BetDetail> _details = new();
    public IReadOnlyCollection<BetDetail> Details => _details.AsReadOnly();
    
    private Bet() { }

    public static Bet Create(Guid userId, Guid bookmakerId, Guid betTypeId, Money stake, List<BetDetail> details)
    {
        var bet = new Bet
        {
            UserId = userId,
            BookmakerId = bookmakerId,
            BetTypeId = betTypeId,
            Stake = stake,
            BetDate = DateTime.UtcNow,
            BetStatus = BetStatus.Pending
        };

        if (details is null || details.Count == 0)
        {
            // Lançar exceção ou tratar erro de validação
            throw new ArgumentException("A bet must have at least one detail.", nameof(details));
        }

        details.ForEach(bet.AddDetail);
        bet.CalculateTotalOdds();

        return bet;
    }

    public void AddDetail(BetDetail detail)
    {
        _details.Add(detail);
    }
    
    private void CalculateTotalOdds()
    {
        var total = _details.Aggregate(1.0m, (acc, d) => acc * d.Odd.Value);
        TotalOdds = new Odd(Math.Round(total, 3));
    }
    
    public override void Validate(IValidationHandler handler)
    {
        // if (Stake.Amount <= 0)
        //     handler.AddError("'Stake' must be positive.");
        // if (TotalOdds.Value <= 0)
        //     handler.AddError("'TotalOdds' must be positive.");
        //
        // Details.ToList().ForEach(d => d.Validate(handler));
    }
}