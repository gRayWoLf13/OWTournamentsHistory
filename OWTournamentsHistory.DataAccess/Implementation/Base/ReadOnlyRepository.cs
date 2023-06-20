using MongoDB.Driver;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Model;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Implementation.Base
{
    internal class ReadOnlyRepository<T> : IReadOnlyRepository<T>
        where T : MongoCollectionEntry
    {
        private static readonly Collation _ignoreCaseCollation = new ("en", strength: CollationStrength.Primary, caseLevel: false);
        protected static readonly Expression<Func<T, bool>> _allPredicate = _ => true;
        protected readonly Func<bool, FindOptions?> _ignoreCaseFindOptionsSelector = ignoreCase => ignoreCase
        ? new()
        {
            Collation = _ignoreCaseCollation
        }
        : null;

        protected readonly Func<bool, AggregateOptions?> _ignoreCaseAggregateOptionsSelector = ignoreCase => ignoreCase
        ? new()
        {
            Collation = _ignoreCaseCollation
        }
        : null;

        protected readonly IMongoClient _mongoClient;
        protected readonly IMongoDatabase _database;
        protected readonly IMongoCollection<T> _entries;
        protected readonly string _mongoCollectionName;

        protected ReadOnlyRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, string mongoCollectionName)
        {
            _mongoClient = mongoClient;
            _database = mongoDatabase;
            _mongoCollectionName = mongoCollectionName;
            _entries = _database.GetCollection<T>(mongoCollectionName);
        }

        public virtual async Task<IReadOnlyCollection<T>> GetAsync(CancellationToken cancellationToken = default) =>
            await _entries.Find(_allPredicate).ToListAsync(cancellationToken);

        public virtual async Task<T> GetAsync(long externalId, CancellationToken cancellationToken = default) =>
            await _entries.Find(entry => entry.ExternalId == externalId).SingleAsync(cancellationToken);

        public virtual async Task<IReadOnlyCollection<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
            await _entries.Find(predicate).ToListAsync(cancellationToken);

        public virtual async Task<IReadOnlyCollection<T>> GetSortedAsync(
            Expression<Func<T, object>> orderingKey,
            bool sortAscending = true,
            Expression<Func<T, bool>>? predicate = null,
            int? skip = null,
            int? limit = null,
            bool ignoreCase = true,
            CancellationToken cancellationToken = default) =>
            await _entries
                .Find(predicate ?? _allPredicate, _ignoreCaseFindOptionsSelector(ignoreCase))
                .Sort(CreateSortByKeyDefinition(orderingKey, sortAscending))
                .Skip(skip)
                .Limit(limit)
                .ToListAsync(cancellationToken);


        protected virtual SortDefinition<T> CreateSortByKeyDefinition(Expression<Func<T, object>> orderingKey, bool ascending) =>
            ascending ? Builders<T>.Sort.Ascending(orderingKey) : Builders<T>.Sort.Descending(orderingKey);

        protected FilterDefinition<T> CreateFilterByIdDefinition(T entry) =>
            Builders<T>.Filter.Eq(e => e.ExternalId, entry.ExternalId);

        protected FilterDefinition<T> CreateFilterByIdDefinition(long entryId) =>
            Builders<T>.Filter.Eq(e => e.ExternalId, entryId);

        protected FilterDefinition<T> CreateFilterByIdDefinition(IReadOnlyCollection<T> entries) =>
            Builders<T>.Filter.In(e => e.ExternalId, entries.Select(x => x.ExternalId));
    }
}
