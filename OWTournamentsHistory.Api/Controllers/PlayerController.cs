using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Common.Utils;
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
    public class PlayerController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(IMapper mapper, IPlayerRepository playerRepository, ILogger<PlayerController> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Player>>> GetMany([FromQuery] int? skip = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
        {
            if (skip < 0 || limit < 0)
            {
                return BadRequest();
            }
            try
            {
                var results = await _playerRepository.GetSortedAsync(p => p.ExternalId, skip: skip, limit: limit, cancellationToken: cancellationToken);

                return results.Select(_mapper.Map<Player>).ToArray();
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
        public async Task<ActionResult<Player>> Get(int id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                return NotFound();
            }

            try
            {
                var result = await _playerRepository.GetAsync(id, cancellationToken);

                return _mapper.Map<Player>(result);
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
        public async Task<ActionResult> Add([FromBody] Player player, CancellationToken cancellationToken)
        {
            try
            {
                var generatedId = await _playerRepository.AddAsync(_mapper.Map<DA.Player>(player), cancellationToken);
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
                await _playerRepository.RemoveAsync(id, cancellationToken);
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
                    .Select(node => new
                    {
                        twitch = string.IsNullOrEmpty(node.SelectSingleNode("td[5]").InnerText) ? node.SelectSingleNode("td[2]").InnerText.Replace(" ", "").Replace(Environment.NewLine, "") : node.SelectSingleNode("td[5]").InnerText,
                        externalId1 = node.SelectSingleNode("td[1]").InnerText,
                        externalId2 = node.SelectSingleNode("td[4]").InnerText,
                        btag = node.SelectSingleNode("td[2]").InnerText.Replace(" ", "").Replace(Environment.NewLine, "")
                    })
                    .GroupBy(data => data.twitch)
                    .Select(data => new DA.Player
                    {
                        Name = NameExtensions.GetName(data.Select(item => item.btag).First()),
                        TwitchId = data.Key,
                        ExternalId = long.Parse(data.First().externalId1),
                        BattleTags = data.Select(item => item.btag).Distinct().ToArray()
                    })
                    .ToArray();

                await _playerRepository.AddRangeAsync(parsedData, cancellationToken);
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
