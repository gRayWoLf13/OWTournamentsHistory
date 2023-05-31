using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class PlayerOpponentsRepository : Repository<PlayerPair>, IPlayerOpponentsRepository
    {
        public PlayerOpponentsRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
           : base(mongoClient, mongoDatabase, databaseSettings.Value.PlayerOpponentsCollectionName)
        {
        }

        public async Task<ICollection<PlayerPair>> GetTopLossesAgainst(string playerName, int limit = 10, bool ignoreCase = true) =>
            await GetTopLossesAggregation(pp => pp.Player2 == playerName, ignoreCase)
                .Limit(limit)
                .ToListAsync();

        public async Task<ICollection<PlayerPair>> GetTopWinsAgainst(string playerName, int limit = 10, bool ignoreCase = true) =>
            await GetTopLossesAggregation(pp => pp.Player1 == playerName, ignoreCase)
                .Limit(limit)
                .ToListAsync();

        private IAggregateFluent<PlayerPair> GetTopLossesAggregation(Expression<Func<PlayerPair, bool>> predicate, bool ignoreCase) =>
            _entries
                .Aggregate(_ignoreCaseAggregateOptionsSelector(ignoreCase))
                .Match(predicate)
                .Project(Builders<PlayerPair>.Projection.Expression(pp => new
                {
                    Source = pp,
                    MapsLost = pp.MapsPlayed - pp.MapsWon,
                }))
                .SortByDescending(ext => ext.MapsLost)
                .Project(ext => ext.Source);

        protected override UpdateDefinition<PlayerPair> CreateFullUpdateDefinition(PlayerPair entry) => throw new NotImplementedException();
    }
}