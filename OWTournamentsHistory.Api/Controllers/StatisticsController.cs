using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Api.Controllers.Helpers;
using OWTournamentsHistory.Api.Services;
using OWTournamentsHistory.Contract.Model.GeneralTournamentStatistics;
using OWTournamentsHistory.Contract.Model.PlayerStatistics;
using Tournament = OWTournamentsHistory.Contract.Model.TournamentStatistics;

namespace OWTournamentsHistory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
#if DEBUG
    [AllowAnonymous]
#endif
    public class StatisticsController : Controller
    {
        private readonly StatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            StatisticsService statisticsService,
            ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Player/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PlayerStatisticsInfo>> GetPlayerStatistics(string name, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _statisticsService.GetPlayerStatistics(name));
        }

        [HttpGet]
        [Route("Tournaments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralTournamentStatisticsInfo>> GetGeneralTournamentStatistics(CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _statisticsService.GetGeneralTournamentStatisticsInfo());
        }

        [HttpGet]
        [Route("Tournament/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tournament.TournamentStatisticsInfo>> GetTournamentStatistics(int number, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _statisticsService.GetTournamentStatistics(number));
        }      
    }
}
