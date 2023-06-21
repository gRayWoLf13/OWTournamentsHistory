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
    public class CalculationsController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IQueue _queue;

        public CalculationsController(IMongoDatabase mongoDatabase, IQueue queue)
        {
            _mongoDatabase = mongoDatabase;
            _queue = queue;
        }


        [HttpPost]
        [Route("Combinations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateCombinations()
        {
            var taskId = _queue.QueueInvocable<CalculatePlayerCombinationsInvocable>();
            return Ok(taskId);
        }

        [HttpPost]
        [Route("TournamentStats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateGeneralTournamentStats()
        {
            var taskId = _queue.QueueInvocable<CalculateGeneralTournamentStatsInvocable>();
            return Ok(taskId);
        }
    }
}
