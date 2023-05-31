using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model
{
    public class TeamPlayerInfo
    {
        public string Name { get; set; }
        public string BattleTag { get; set; }
        public PlayerRole Role { get; set; }
        public decimal? Weight { get; set; }
        public int? Division { get; set; }
        public string? DisplayWeight { get; set; }
        public PlayerSubRole? SubRole { get; set; }
        public int TournamentsPlayed { get; set; }
        public bool IsNewPlayer { get;set; }
        public bool IsNewRole { get; set; }
        public decimal WeightShift { get; set; }
    }
}
