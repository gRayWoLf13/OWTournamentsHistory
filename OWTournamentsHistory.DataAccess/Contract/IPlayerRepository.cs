using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IPlayerRepository : IRepository<Player>
    {
        Task<Player> FindPlayer(string name);
    }
}
