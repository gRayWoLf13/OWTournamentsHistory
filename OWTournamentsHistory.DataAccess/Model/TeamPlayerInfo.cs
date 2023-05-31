using OWTournamentsHistory.DataAccess.Model.Type;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace OWTournamentsHistory.DataAccess.Model
{
    public class TeamPlayerInfo
    {
        public string Name { get; set; }
        public string BattleTag { get; set; }
        public TeamPlayerRole Role { get; set; }
        public decimal? Weight { get; set; }
        public int? Division { get; set; }
        public string? DisplayWeight { get; set; }
        public TeamPlayerSubRole? SubRole { get; set; }
        public bool IsNewPlayer { get; set; }
        public bool IsNewRole { get; set; }
        public decimal WeightShift { get; set; }
    }
}
