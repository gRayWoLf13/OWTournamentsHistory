using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Model;
using SharpCompress.Common;

namespace OWTournamentsHistory.DataAccess.Implementation.Base
{
    internal abstract class Repository<T> : ReadOnlyRepository<T>, IRepository<T>
        where T : MongoCollectionEntry
    {

        protected Repository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, string mongoCollectionName) 
            : base(mongoClient, mongoDatabase, mongoCollectionName)
        { }

        public virtual async Task<long> AddAsync(T entry, CancellationToken cancellationToken = default)
        {
           // GenerateExternalId(entry);
            await _entries.InsertOneAsync(entry, new(), cancellationToken);
            return entry.ExternalId;
        }

        public virtual async Task AddRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default)
        {
            //GenerateExternalIds(entries);
            await _entries.InsertManyAsync(entries, new(), cancellationToken);
        }

        public virtual async Task RemoveAsync(T entry, CancellationToken cancellationToken = default) =>
            await _entries.DeleteOneAsync(CreateFilterByIdDefinition(entry), cancellationToken);
        public async Task RemoveAsync(long entryId, CancellationToken cancellationToken = default) =>
            await _entries.DeleteOneAsync(CreateFilterByIdDefinition(entryId), cancellationToken);
        public virtual async Task RemoveRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default) =>
            await _entries.DeleteManyAsync(CreateFilterByIdDefinition(entries), cancellationToken);

        public async virtual Task UpdateAsync(T entry, CancellationToken cancellationToken = default) =>
            await _entries.UpdateOneAsync(CreateFilterByIdDefinition(entry), CreateFullUpdateDefinition(entry), cancellationToken: cancellationToken);

        public async virtual Task UpdateRangeAsync(IReadOnlyCollection<T> entries, CancellationToken cancellationToken = default)
        {
            using (var session = await _mongoClient.StartSessionAsync())
            {
                var result = await session.WithTransactionAsync(async (s, ct) =>
                {
                    foreach (var entry in entries)
                    {
                        await UpdateAsync(entry, ct);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);
            }
        }

        public virtual async Task Import(string jsonData, CancellationToken cancellationToken = default)
        {
            var documents = BsonSerializer.Deserialize<IReadOnlyCollection<T>>(jsonData);
            await AddRangeAsync(documents, cancellationToken);
        }

        public virtual async Task Clear(CancellationToken cancellationToken = default)
        {
            await _entries.DeleteManyAsync(_ => true, cancellationToken);
        }
       
        protected virtual void GenerateExternalId(T entry)
        {
            entry.ExternalId = Sequence.GetNextSequenceValue(_mongoCollectionName, _database);
        }

        protected virtual void GenerateExternalIds(IReadOnlyCollection<T> entries)
        {
            foreach(var  entry in entries)
            {
                GenerateExternalId(entry);
            }
        }

        protected FilterDefinition<T> CreateFilterByIdDefinition(T entry) =>
            Builders<T>.Filter.Eq(e => e.ExternalId, entry.ExternalId);

        protected FilterDefinition<T> CreateFilterByIdDefinition(long entryId) =>
            Builders<T>.Filter.Eq(e => e.ExternalId, entryId);

        protected FilterDefinition<T> CreateFilterByIdDefinition(IReadOnlyCollection<T> entries) =>
            Builders<T>.Filter.In(e => e.ExternalId, entries.Select(x => x.ExternalId));

        protected abstract UpdateDefinition<T> CreateFullUpdateDefinition(T entry);
    }
}
