using Coravel.Events.Interfaces;
using Coravel.Queuing.Broadcast;
using Microsoft.Extensions.DependencyInjection;
using OWTournamentsHistory.Tasks.Utils;

namespace OWTournamentsHistory.Tasks.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTasks(this IServiceCollection collection)
        {
            collection.AddScoped<CalculatePlayerCombinationsInvocable>();
            collection.AddScoped<ExportDatabaseToDropboxInvocable>();
            collection.AddScoped<TwitchApiTestInvocable>();
            collection.AddScoped<CalculateGeneralTournamentStatsInvocable>();
        }

        public static void AddTaskListeners(this IServiceCollection collection)
        {
            collection.AddScoped<IListener<QueueTaskStarted>, TaskStartedListener>();
            collection.AddScoped<IListener<QueueTaskCompleted>, TaskCompletedListener>();
            collection.AddScoped<IListener<DequeuedTaskFailed>, TaskFailedListener>();
        }
    }
}
