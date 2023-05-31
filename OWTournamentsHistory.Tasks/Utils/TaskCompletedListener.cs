using Coravel.Events.Interfaces;
using Coravel.Queuing.Broadcast;
using System.Diagnostics;

namespace OWTournamentsHistory.Tasks.Utils
{
    internal class TaskCompletedListener : IListener<QueueTaskCompleted>
    {
        public Task HandleAsync(QueueTaskCompleted broadcasted)
        {
            Debug.WriteLine($"Task {broadcasted.Guid} completed at {broadcasted.CompletedAtUtc}");
            return Task.CompletedTask;
        }
    }
}
