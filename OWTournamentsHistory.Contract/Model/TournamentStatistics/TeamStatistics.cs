using OWTournamentsHistory.Contract.Model.PlayerStatistics;

namespace OWTournamentsHistory.Contract.Model.TournamentStatistics;

public class TeamStatistics
{
    public required ICollection<PlayerInTheTeamInfo> Players { get; set; }
    public decimal? AverageMatchesCloseScore { get; set; }
    public decimal? TotalAverageMatchesCloseScore { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWinRate { get; set; }
    public decimal TeamWeight { get; set; }
    public decimal TotalTeamWeight { get; set; }
    public int MatchesPlayed { get; set; }

}
