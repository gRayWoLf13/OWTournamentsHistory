using OWTournamentsHistory.DataAccess.Model;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IPlayerDuosRepository : IRepository<PlayerPair>
    {
        Task<IReadOnlyCollection<PlayerPair>> GetByWinRate(
            Expression<Func<PlayerPair, bool>>? predicate = null,
            bool sortAscending = false,
            int limit = 20,
            CancellationToken cancellationToken = default);
    }
}
