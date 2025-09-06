namespace Dbets.Domain.Enums;

public enum Theme { light, dark }

public enum GameStatus { Scheduled, Live, Finished, Postponed, Cancelled }

public enum BetStatus { Pending, Won, Lost, Void, Partially_Won }

public enum BetDetailResult { Won, Lost, Void, Pushed }

public enum TransactionType { Deposit, Withdrawal, Bet_Placed, Bet_Won, Bet_Lost, Commission }