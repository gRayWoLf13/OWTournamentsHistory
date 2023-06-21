using OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.DataAccess.Model.Statistics
{
    public class GeneralTournamentChampionInfo
    {
        public int ChampionsCount { get; set; }
        public required TeamPlayerRole[] Roles { get; set; }
    }
}
