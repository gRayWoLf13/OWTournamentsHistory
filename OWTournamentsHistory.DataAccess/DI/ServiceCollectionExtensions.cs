#define TraceDatabase
#undef TraceDatabase

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OWTournamentsHistory.Common.Settings;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Implementation;
#if TraceDatabase
using System.Diagnostics;
using MongoDB.Driver.Core.Configuration;
#endif
namespace OWTournamentsHistory.DataAccess.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDatabaseService(this IServiceCollection collection, IConfiguration configuration)
        {
            var databaseSettings = configuration
                .GetSection(nameof(OWTournamentsHistoryDatabaseSettings))
                .Get<OWTournamentsHistoryDatabaseSettings>()
                ?? throw new Exception($"{nameof(OWTournamentsHistoryDatabaseSettings)} not found in application configuration");

            var client = CreateSharedClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);
            collection.AddSingleton(client);
            collection.AddSingleton(database);
        }

        public static void AddDataAccess(this IServiceCollection collection)
        {
            collection.AddScoped<IPlayerRepository, PlayerRepository>();
            collection.AddScoped<ITeamRepository, TeamRepository>();
            collection.AddScoped<IMatchRepository, MatchRepository>();
            collection.AddScoped<IPlayerDuosRepository, PlayerDuosRepository>();
            collection.AddScoped<IPlayerOpponentsRepository, PlayerOpponentsRepository>();
            collection.AddScoped<IGeneralTournamentStatsRepository, GeneralTournamentStatsRepository>();
        }

        private static IMongoClient CreateSharedClient(string connectionString)
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.MaxConnectionIdleTime = TimeSpan.FromSeconds(30);
            settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(30);
            settings.MaxConnecting = 10;
#if TraceDatabase
            settings.ClusterConfigurator = cb =>
            {
                var traceSource = new TraceSource("OWTournamentsHistory.DataAccess", SourceLevels.Verbose);
                var queryTraceSource = new TraceSource("OWTournamentsHistory.DataAccess.Query", SourceLevels.Verbose);
                cb.TraceWith(traceSource);
                cb.TraceCommandsWith(queryTraceSource);
            };
#endif
            return new MongoClient(settings);
        }
    }
}
