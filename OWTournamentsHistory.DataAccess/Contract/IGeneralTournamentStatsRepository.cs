using OWTournamentsHistory.DataAccess.Model.Statistics;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IGeneralTournamentStatsRepository : IReadOnlyRepository<GeneralTournamentStats>
    {
        Task UpdateStats(GeneralTournamentStats stats, CancellationToken cancellationToken = default);
        Task<GeneralTournamentStats> GetStats(CancellationToken cancellationToken = default);
    }
}
