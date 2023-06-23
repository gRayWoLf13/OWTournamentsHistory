using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        StatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            ILogger<StatisticsController> logger,
            StatisticsService statisticsService)
        {
            _logger = logger;
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("Player/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PlayerStatisticsInfo>> GetPlayerStatistics(string name, CancellationToken cancellationToken)
        {
            try
            {
                return await _statisticsService.GetPlayerStatistics(name);
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpGet]
        [Route("Tournaments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralTournamentStatisticsInfo>> GetGeneralTournamentStatistics(CancellationToken cancellationToken)
        {
            try
            {
                return await _statisticsService.GetGeneralTournamentStatisticsInfo();
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpGet]
        [Route("Tournament/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tournament.TournamentStatisticsInfo>> GetTournamentStatistics(int number, CancellationToken cancellationToken)
        {
            try
            {
                return await _statisticsService.GetTournamentStatistics(number);
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        private static ObjectResult WrapException(Exception ex)
            => new(ex.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
    }
}
