using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task<ICollection<Team>> FindTeamsByPlayerName(string playerName, bool ignoreCase = true);
    }
}
