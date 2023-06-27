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
    public class MatchController : Controller
    {
        private readonly ILogger<MatchController> _logger;
        private readonly MatchesService _matchesService;

        public MatchController(MatchesService matchesService, ILogger<MatchController> logger)
        {
            _matchesService = matchesService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Match>>> GetMany([FromQuery] int? skip = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
        {
            return await Converters.WrapApiCall(async () => await _matchesService.GetMany(skip, limit, cancellationToken));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Match>> Get(int id, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _matchesService.Get(id, cancellationToken));
        }

        [HttpPut]
        [Route("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Add([FromBody] Match match, CancellationToken cancellationToken)
        {
            var generatedId = await Converters.WrapApiCall(async () => await _matchesService.Add(match, cancellationToken));
            return CreatedAtAction(nameof(Get), new { id = generatedId }, null);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            return await Converters.WrapApiCall(async () => await _matchesService.Delete(id, cancellationToken));
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

            return await Converters.WrapApiCall(async () => await _matchesService.ImportFromHtml(html, cancellationToken));
        }
    }
}
