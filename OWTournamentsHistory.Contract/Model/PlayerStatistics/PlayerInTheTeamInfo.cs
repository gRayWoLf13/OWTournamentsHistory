using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.PlayerStatistics;

public class PlayerInTheTeamInfo
{
    public required string Name { get; set; }
    public required string BattleTag { get; set; }
    public PlayerRole Role { get; set; }
    public PlayerSubRole? SubRole { get; set; }
    public int? Division { get; set; }
    public string? DisplayWeight { get; set; }
}
