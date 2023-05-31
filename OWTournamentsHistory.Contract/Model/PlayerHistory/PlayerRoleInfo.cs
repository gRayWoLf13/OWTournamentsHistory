using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.PlayerHistory
{
    public class PlayerRoleInfo
    {
        public int TournamentsPlayed { get; set; }
        public decimal? AverageWinrate { get; set; }
        public int? LastDivision { get; set; }
        public string? LastDisplayWeight { get; set; }
    }
}
