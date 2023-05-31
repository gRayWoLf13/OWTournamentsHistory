using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class PlayerDuosRepository : Repository<PlayerPair>, IPlayerDuosRepository
    {
        public PlayerDuosRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
           : base(mongoClient, mongoDatabase, databaseSettings.Value.PlayerDuosCollectionName)
        {
        }
        protected override UpdateDefinition<PlayerPair> CreateFullUpdateDefinition(PlayerPair entry) => throw new NotImplementedException();
    }
}
