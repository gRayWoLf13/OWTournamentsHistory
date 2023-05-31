using OWTournamentsHistory.DataAccess.Model;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IReadOnlyRepository<T>
        where T : MongoCollectionEntry
    {
        Task<IReadOnlyCollection<T>> GetAsync(CancellationToken cancellationToken = default);
        Task<T> GetAsync(long externalId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<T>> GetSortedAsync(Expression<Func<T, object>> orderingKey, bool sortAscending = true, Expression<Func<T, bool>>? predicate = null, int? skip = null, int? limit = null, bool ignoreCase = true, CancellationToken cancellationToken = default);
    }
}
