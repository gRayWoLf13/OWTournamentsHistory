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
}
