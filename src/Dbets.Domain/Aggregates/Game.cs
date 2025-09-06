using Dbets.Domain.Common;
using Dbets.Domain.Enums;
using Dbets.Domain.Validations;

namespace Dbets.Domain.Aggregates;

public class Game : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string HomeTeam { get; private set; }
    public string AwayTeam { get; private set; }
    public string? Championship { get; private set; }
    public DateTime GameDate { get; private set; }
    public GameStatus GameStatus { get; private set; }
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }
    public string Sport { get; private set; }

    private Game() { }
    
    public static Game Create(Guid userId, string homeTeam, string awayTeam, DateTime gameDate, string sport = "Football", string? championship = null)
    {
        return new Game
        {
            UserId = userId,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            GameDate = gameDate,
            Sport = sport,
            Championship = championship,
            GameStatus = GameStatus.Scheduled
        };
    }

    public override void Validate(IValidationHandler handler) 
    {
        // if(string.IsNullOrWhiteSpace(HomeTeam))
        //     handler.AddError("'HomeTeam' cannot be empty.");
        // if(string.IsNullOrWhiteSpace(AwayTeam))
        //     handler.AddError("'AwayTeam' cannot be empty.");
        // if(GameDate < DateTime.UtcNow.AddMinutes(-120)) // Exemplo: Jogo não pode ser cadastrado no passado
        //     handler.AddError("'GameDate' cannot be in the past.");
    }
}