using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class TeamRepository : Repository<Team>, ITeamRepository
    {

        public TeamRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
            : base(mongoClient, mongoDatabase, databaseSettings.Value.TeamsCollectionName)
        {
        }

        public async Task<ICollection<Team>> FindTeamsByPlayerName(string playerName, bool ignoreCase = true)
        {
            var teams = await _entries.Find(team => team.Players.Any(player => player.Name == playerName),
                    _ignoreCaseFindOptionsSelector(ignoreCase))
                .ToListAsync();
            return teams;
        }

        protected override UpdateDefinition<Team> CreateFullUpdateDefinition(Team entry) => throw new NotImplementedException();
    }
}
