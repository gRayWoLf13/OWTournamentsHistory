using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Api.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using System.Text;
using DA = OWTournamentsHistory.DataAccess.Model;

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
        private readonly IMapper _mapper;
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<MatchController> _logger;

        public MatchController(IMapper mapper, IMatchRepository matchRepository, ILogger<MatchController> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Match>>> GetMany([FromQuery] int? skip = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
        {
            if (skip < 0 || limit < 0)
            {
                return BadRequest();
            }
            try
            {
                var results = await _matchRepository.GetSortedAsync(p => p.ExternalId, skip: skip, limit: limit, cancellationToken: cancellationToken);

                return results.Select(_mapper.Map<Match>).ToArray();
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Match>> Get(int id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                return NotFound();
            }

            try
            {
                var result = await _matchRepository.GetAsync(id, cancellationToken);

                return  _mapper.Map<Match>(result);
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpPut]
        [Route("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Add([FromBody] Match player, CancellationToken cancellationToken)
        {
            try
            {
                var generatedId = await _matchRepository.AddAsync(_mapper.Map<DA.Match>(player), cancellationToken);
                return CreatedAtAction(nameof(Get), new { id = generatedId }, null);
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                return NotFound();
            }

            try
            {
                await _matchRepository.RemoveAsync(id, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpPut]
        [Route("import")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ImportFromHtml(CancellationToken cancellationToken)
        {
            try
            {
                string html;
                using (var streamReader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    html = await streamReader.ReadToEndAsync(cancellationToken);
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var parsedData = doc.DocumentNode
                    .SelectNodes("/div/table/tbody/tr")
                    .Skip(1)
                    .Select(node => new DA.Match
                        {
                            ExternalId = long.Parse(node.SelectSingleNode("td[14]").InnerText),
                            TournamentNumber = int.Parse(node.SelectSingleNode("td[1]").InnerText),
                            MatchNumber = int.Parse(node.SelectSingleNode("td[4]").InnerText),
                            Team1CaptainName = node.SelectSingleNode("td[6]").InnerText.Replace(" ", "").Replace(Environment.NewLine, ""),
                            Team2CaptainName = node.SelectSingleNode("td[7]").InnerText.Replace(" ", "").Replace(Environment.NewLine, ""),
                            ScoreTeam1 = int.Parse(node.SelectSingleNode("td[9]").InnerText), 
                            ScoreTeam2 = int.Parse(node.SelectSingleNode("td[10]").InnerText),
                            Closeness = node.SelectSingleNode("td[12]").InnerText.ParseTo<decimal>()
                        })
                    .GroupBy(m => m.ExternalId)
                    .Select(m => m.First())
                    .ToArray();

                await _matchRepository.AddRangeAsync(parsedData, cancellationToken);
                return NoContent();
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
