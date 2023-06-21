namespace OWTournamentsHistory.Contract.Model.PlayerStatistics;

public class PlayerTeamInfo
{
    public required string CaptainName { get; set; }
    public int TournamentNumber { get; set; }
    public int Place { get; set; }
    public int MapsWon { get; set; }
    public int MapsPlayed { get; set; }
    public int MatchesPlayed { get; set; }
    public decimal? AverageMatchesCloseScore { get; set; }
    public required ICollection<PlayerInTheTeamInfo> TeamMembers { get; set; }
    public required ICollection<MatchInfo> TeamMatches { get; set; }

    public decimal WinRate => (decimal)MapsWon / MapsPlayed * 100;
}
