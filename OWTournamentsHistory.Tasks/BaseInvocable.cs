using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace OWTournamentsHistory.Tasks
{
    public abstract class BaseInvocable<T> : IInvocable
    {
        protected readonly ILogger<T> _logger;

        protected BaseInvocable(ILogger<T> logger)
        {
            _logger = logger;
        }

        public async Task Invoke()
        {
            try
            {
                await InvokeInternal();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Invocable {nameof(T)} failed to execute");
            }
            finally
            {
                GC.Collect();
            }
        }

        protected abstract Task InvokeInternal();
    }
}
