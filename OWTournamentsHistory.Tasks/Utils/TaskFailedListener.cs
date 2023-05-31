using Coravel.Events.Interfaces;
using Coravel.Queuing.Broadcast;
using System.Diagnostics;

namespace OWTournamentsHistory.Tasks.Utils
{
    internal class TaskFailedListener : IListener<DequeuedTaskFailed>
    {
        public Task HandleAsync(DequeuedTaskFailed broadcasted)
        {
            Debug.WriteLine($"Task {broadcasted.Guid} failed at {broadcasted.FailedAtUtc}, {broadcasted.DequeuedTask}");
            return Task.CompletedTask;
        }
    }
}
