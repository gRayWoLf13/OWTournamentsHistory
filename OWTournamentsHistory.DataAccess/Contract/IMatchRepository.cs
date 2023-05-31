using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<IReadOnlyCollection<Match>> Find(int tournamentNumber, string captainName, bool ignoreCase = true);
        Task<IReadOnlyCollection<Match>> Find(IReadOnlyCollection<(int tournamentNumber, string captainName)> captains, bool ignoreCase = true);
    }
}
