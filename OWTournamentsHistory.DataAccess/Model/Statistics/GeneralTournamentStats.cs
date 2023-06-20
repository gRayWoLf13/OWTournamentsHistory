using OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.DataAccess.Model.Statistics
{
    public class GeneralTournamentStats : MongoCollectionEntry
    {
        public int TournamentsCount { get; set; }
        public int OWALsCount { get; set; }
        public int ChampionsCount { get; set; }

        public decimal AverageWinRate { get; set; }
        public decimal AverageTournamentsPlayedPercentage { get; set; }

        public required ICollection<GeneralRoleWinRateInfo> RoleWinRates { get; set; }

        public required IDictionary<string, int> PlayersToDivisionsCount { get; set; }
        public required IDictionary<string, int> MatchesClosenessCount { get; set; }
        public required IDictionary<string, TournamentStats> StatsToTournaments { get; set; }
        public required IDictionary<string, int> TopPlayersByScore { get; set; }
        public required IDictionary<string, GeneralTournamentChampionInfo> TopChampions { get; set; }
        public required IDictionary<string, decimal> TopWinRate { get; set; }
        public required IDictionary<string, GeneralTournamentChampionInfo> Top0Wins { get; set; }
        public required IDictionary<string, GeneralRolesToDivisionsInfo> PlayerRolesToDivisions { get; set; }
    }

    public class GeneralRoleWinRateInfo
    {
        public TeamPlayerRole Role { get; set; }
        public decimal AverageWinRate { get; set; }
    }

    public class GeneralTournamentChampionInfo
    {
        public int ChampionsCount { get; set; }
        public required TeamPlayerRole[] Roles { get; set; }
    }

    public class GeneralRolesToDivisionsInfo
    {
        public int TanksCount { get; set; }
        public int DpsCount { get; set; }
        public int SupportsCount { get; set; }
        public int FlexCount { get; set; }

    }

}
