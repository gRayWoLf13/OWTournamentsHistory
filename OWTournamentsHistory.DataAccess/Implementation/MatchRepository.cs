using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class MatchRepository : Repository<Match>, IMatchRepository
    {
        public MatchRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings)
            : base(mongoClient, mongoDatabase, databaseSettings.Value.MatchesCollectionName)
        {
        }

        public async Task<IReadOnlyCollection<Match>> Find(int tournamentNumber, string captainName, bool ignoreCase = true)
        {
            return await _entries.Find(match => match.TournamentNumber == tournamentNumber && (match.Team1CaptainName == captainName || match.Team2CaptainName == captainName),
                    _ignoreCaseFindOptionsSelector(ignoreCase))
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Match>> Find(IReadOnlyCollection<(int tournamentNumber, string captainName)> captains, bool ignoreCase = true)
        {
            var results = new List<Match>();
            foreach (var (tournamentNumber, captainName) in captains)
            {
                results.AddRange(await Find(tournamentNumber, captainName, ignoreCase));
            }
            return results;
        }

        protected override UpdateDefinition<Match> CreateFullUpdateDefinition(Match entry) => throw new NotImplementedException();
    }
}
