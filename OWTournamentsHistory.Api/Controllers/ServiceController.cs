using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OWTournamentsHistory.Tasks;

namespace OWTournamentsHistory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
#if DEBUG
    [AllowAnonymous]
#endif
    public class ServiceController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IQueue _queue;

        public ServiceController(IMongoDatabase mongoDatabase, IQueue queue)
        {
            _mongoDatabase = mongoDatabase;
            _queue = queue;
        }

        [HttpPost]
        [Route("BackupDatabase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ScheduleDatabaseBackup()
        {
            var taskId = _queue.QueueInvocable<ExportDatabaseToDropboxInvocable>();
            return Ok(taskId);
        }

        [HttpPost]
        [Route("GetTwitchInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTwitchInfo()
        {
            var taskId = _queue.QueueInvocable<TwitchApiTestInvocable>();
            return Ok(taskId);
        }
    }
}
