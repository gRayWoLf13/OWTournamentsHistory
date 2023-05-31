using Coravel.Events.Interfaces;
using Coravel.Queuing.Broadcast;
using System.Diagnostics;

namespace OWTournamentsHistory.Tasks.Utils
{
    internal class TaskStartedListener : IListener<QueueTaskStarted>
    {
        public Task HandleAsync(QueueTaskStarted broadcasted)
        {
            Debug.WriteLine($"Task {broadcasted.Guid} started at {broadcasted.StartedAtUtc}");
            return Task.CompletedTask;
        }
    }
}
