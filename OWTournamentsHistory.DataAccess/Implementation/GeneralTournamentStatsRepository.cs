using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation.Base;
using OWTournamentsHistory.DataAccess.Model.Statistics;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Implementation
{
    internal class GeneralTournamentStatsRepository : ReadOnlyRepository<GeneralTournamentStats>, IGeneralTournamentStatsRepository
    {
        private const int STATS_ID = 1;
        public GeneralTournamentStatsRepository(IMongoClient mongoClient, IMongoDatabase mongoDatabase, IOptions<OWTournamentsHistoryDatabaseSettings> databaseSettings) 
            : base(mongoClient, mongoDatabase, databaseSettings.Value.GeneralTournamentStatsCollectionName)
        {}

        public override Task<IReadOnlyCollection<GeneralTournamentStats>> GetAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<GeneralTournamentStats> GetAsync(long externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IReadOnlyCollection<GeneralTournamentStats>> GetAsync(Expression<Func<GeneralTournamentStats, bool>> predicate, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
        public override Task<IReadOnlyCollection<GeneralTournamentStats>> GetSortedAsync(
            Expression<Func<GeneralTournamentStats, object>> orderingKey,
            bool sortAscending = true,
            Expression<Func<GeneralTournamentStats, bool>>? predicate = null,
            int? skip = null,
            int? limit = null,
            bool ignoreCase = true,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public async Task<GeneralTournamentStats> GetStats(CancellationToken cancellationToken = default) =>
            await _entries.Find(CreateFilterByIdDefinition(STATS_ID))
            .SingleAsync(cancellationToken);

        public async Task UpdateStats(GeneralTournamentStats stats, CancellationToken cancellationToken = default)
        {
            stats.ExternalId = STATS_ID;
            await _entries.DeleteOneAsync(CreateFilterByIdDefinition(STATS_ID));
            await _entries.InsertOneAsync(stats, new(), cancellationToken);
        }
    }
}
