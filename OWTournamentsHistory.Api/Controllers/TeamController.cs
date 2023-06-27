using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Api.Controllers.Helpers;
using OWTournamentsHistory.Api.Services;
using OWTournamentsHistory.Contract.Model;
using System.Text;

namespace OWTournamentsHistory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
#if DEBUG
    [AllowAnonymous]
#endif
    public class TeamController : Controller
    {
        private readonly TeamsService _teamService;
        private readonly ILogger<Team> _logger;

        public TeamController(TeamsService teamService, ILogger<Team> logger)
        {
            _teamService = teamService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Team>>> GetMany([FromQuery] int? skip = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
        {
            return await Converters.WrapApiCall(async () => await _teamService.GetMany(skip, limit, cancellationToken));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Team>> Get(long id, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _teamService.Get(id, cancellationToken));
        }

        [HttpPut]
        [Route("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Add([FromBody] Team team, CancellationToken cancellationToken)
        {
            var generatedId = await Converters.WrapApiCall(async () => await _teamService.Add(team, cancellationToken));
            return CreatedAtAction(nameof(Get), new { id = generatedId }, null);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _teamService.Delete(id, cancellationToken), successResult: NoContent());
        }

        [HttpPut]
        [Route("import")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ImportFromHtml(CancellationToken cancellationToken)
        {
            string html;
            using (var streamReader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                html = await streamReader.ReadToEndAsync(cancellationToken);
            }

            return await Converters.WrapApiCall(async () => await _teamService.ImportFromHtml(html, cancellationToken), successResult: NoContent());
        }

        [HttpPut]
        [Route("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ValidateTeams(CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _teamService.ValidateTeams(cancellationToken), successResult: Ok());
        }
    }
}
