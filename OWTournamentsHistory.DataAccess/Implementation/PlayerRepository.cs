using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
            : base(mongoClient, mongoDatabase, databaseSettings.Value.PlayersCollectionName)
        {
        }

        public async Task<Player> FindPlayer(string name) =>
            await _entries.Find(player => player.Name == name).SingleOrDefaultAsync();

        protected override UpdateDefinition<Player> CreateFullUpdateDefinition(Player entry) => throw new NotImplementedException();
    }
}
