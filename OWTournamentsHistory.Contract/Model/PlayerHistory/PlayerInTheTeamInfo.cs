using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.PlayerHistory
{
    public class PlayerInTheTeamInfo
    {
        public string Name { get; set; }
        public string BattleTag { get; set; }
        public PlayerRole Role { get; set; }
        public PlayerSubRole? SubRole { get; set; }
        public int? Division { get; set; }
        public string? DisplayWeight { get; set; }
    }
}
