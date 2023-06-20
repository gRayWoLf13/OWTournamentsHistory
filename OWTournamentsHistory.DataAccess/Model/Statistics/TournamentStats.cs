using OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.DataAccess.Model.Statistics
{
    public class TournamentStats
    {
        public decimal AverageCloseness { get; set; }
        public int TeamsCount { get; set; }
        public int PlayersCount { get; set; }
        public int MatchesCount { get; set; }
        public decimal? AverageTeamWeight { get; set; }
        public required ICollection<DivisionToRoleInfo> AverageDivisionToRole { get; set; }
    }

    public class DivisionToRoleInfo
    {
        public TeamPlayerRole Role { get; set; }
        public decimal? Division { get; set; }
    }
}
