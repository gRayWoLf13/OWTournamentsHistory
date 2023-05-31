using Coravel.Scheduling.Schedule.Interfaces;
using OWTournamentsHistory.Tasks;

namespace OWTournamentsHistory.Api
{
    internal static class SchedulerProfile
    {
        public static Action<IScheduler> SchedulerTasks => AddSchedulerActions;

        private static void AddSchedulerActions(IScheduler scheduler)
        {
            scheduler
                .Schedule<ExportDatabaseToDropboxInvocable>()
                .DailyAtHour(2);

            //scheduler
            //   .Schedule<ExportDatabaseToDropboxInvocable>()
            //   .EveryMinute();
        }
    }
}
