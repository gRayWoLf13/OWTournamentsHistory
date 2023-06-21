namespace OWTournamentsHistory.Contract.Model.PlayerStatistics;

public class PlayerRoleInfo
{
    public int TournamentsPlayed { get; set; }
    public decimal? AverageWinrate { get; set; }
    public int? LastDivision { get; set; }
    public string? LastDisplayWeight { get; set; }
}
