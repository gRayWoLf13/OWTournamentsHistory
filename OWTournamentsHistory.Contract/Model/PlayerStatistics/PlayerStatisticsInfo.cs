using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.PlayerStatistics;

public class PlayerStatisticsInfo
{    public required string Name { get; set; }
    public required ICollection<string> BattleTags { get; set; }
    public required string TwitchId { get; set; }
    public int TournamentsPlayed { get; set; }
    public int TournamentsWon { get; set; }
    public int TournamentsWith0Wins { get; set; }
    public decimal GlobalWinRate { get; set; }
    public int BestResultPlace { get; set; }
    public int BestResultTournamentNumber { get; set; }
    public required string BestResultCaptainName { get; set; }
    public decimal? AverageCloseness { get; set; }
    public int MapsWon { get; set; }
    public int MapsPlayed { get; set; }
    public int AveragePlace { get; set; }
    public required IDictionary<PlayerRole, PlayerRoleInfo> RoleInfos { get; set; }
    public required ICollection<PlayerTeamInfo> Teams { get; set; }

    #region Charts data
    public required ICollection<Point2D<decimal?>> TankPriceData { get; set; }
    public required ICollection<Point2D<decimal?>> DpsPriceData { get; set; }
    public required ICollection<Point2D<decimal?>> SupportPriceData { get; set; }
    public required ICollection<Point2D<decimal?>> PlaceData { get; set; }

    public required ICollection<Point2DWithLabel<decimal>> StandardDeviation { get; set; }
    #endregion

    #region Combinations data

    public required ICollection<NameCount> MostWinsAgainst { get; set; }
    public required ICollection<NameCount> MostLossesAgainst { get; set; }
    //public required ICollection<NameCount> MostKnockoutsTo { get; set; }
    //public required ICollection<NameCount> MostKnockoutsFrom { get; set; }

    #endregion
}

