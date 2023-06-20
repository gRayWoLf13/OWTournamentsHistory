using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Contract
{
    public interface IRepository<T> : IReadOnlyRepository<T>
        where T : MongoCollectionEntry
    {
        Task<long> AddAsync(T entry, CancellationToken cancellationToken = default);
        Task RemoveAsync(T entry, CancellationToken cancellationToken = default);
        Task RemoveAsync(long entryId, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default);
        Task RemoveRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entry, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default);
        Task Import(string jsonData, CancellationToken cancellationToken = default);
        Task Clear(CancellationToken cancellationToken = default);
    }
}
