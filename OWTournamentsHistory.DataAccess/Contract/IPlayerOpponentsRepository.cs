using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IPlayerOpponentsRepository : IRepository<PlayerPair>
    {
        Task<ICollection<PlayerPair>> GetTopWinsAgainst(string playerName, int limit = 10, bool ignoreCase = true);
        Task<ICollection<PlayerPair>> GetTopLossesAgainst(string playerName, int limit = 10, bool ignoreCase = true);
    }
}
