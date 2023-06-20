using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;
using System.Linq.Expressions;
using OWTournamentsHistory.DataAccess.Extensions;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class PlayerDuosRepository : Repository<PlayerPair>, IPlayerDuosRepository
    {
        public PlayerDuosRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
           : base(mongoClient, mongoDatabase, databaseSettings.Value.PlayerDuosCollectionName)
        {
        }

        public async Task<IReadOnlyCollection<PlayerPair>> GetByWinRate(
            Expression<Func<PlayerPair, bool>>? predicate = null,
            bool sortAscending = false,
            int limit = 20,
            CancellationToken cancellationToken = default) =>
            await _entries
                .Aggregate()
                .Match(predicate ?? _allPredicate)
                .Project(Builders<PlayerPair>.Projection.Expression(pp => new 
                    { 
                        Source = pp, 
                        WinRate =  pp.MapsWon / (decimal)pp.MapsPlayed 
                    }))
                .SortByAny(pp => pp.WinRate, sortAscending)
                .Project(ext => ext.Source)
                .Limit(limit)
                .ToListAsync(cancellationToken);        

        protected override UpdateDefinition<PlayerPair> CreateFullUpdateDefinition(PlayerPair entry) => throw new NotImplementedException();
    }
}
