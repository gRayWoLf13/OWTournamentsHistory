using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.PlayerHistory
{
    public class PlayerHistoryInfo
    {
        public string Name { get; set; }
        public ICollection<string> BattleTags { get; set; }
        public string TwitchId { get; set; }
        public int TournamentsPlayed { get; set; }
        public int TournamentsWon { get; set; }
        public int TournamentsWith0Wins { get; set; }
        public decimal GlobalWinRate { get; set; }
        public int BestResultPlace { get; set; }
        public int BestResultTournamentNumber { get; set; }
        public string BestResultCaptainName { get; set; }
        public decimal? AverageCloseness { get; set; }
        public int MapsWon { get; set; }
        public int MapsPlayed { get; set; }
        public int AveragePlace { get; set; }
        public IDictionary<PlayerRole, PlayerRoleInfo> RoleInfos { get; set; }
        public ICollection<TeamInfo> Teams { get; set; }

        #region Charts data
        public ICollection<Point2D> TankPriceData { get; set; }
        public ICollection<Point2D> DpsPriceData { get; set; }
        public ICollection<Point2D> SupportPriceData { get; set; }
        public ICollection<Point2D> PlaceData { get; set; }

        public ICollection<Point2DWithLabel> StandardDeviation { get; set; }
        #endregion

        #region Combinations data

        public ICollection<NameCount> MostWinsAgainst { get; set; }
        public ICollection<NameCount> MostLossesAgainst { get; set; }

        #endregion
    }
}
