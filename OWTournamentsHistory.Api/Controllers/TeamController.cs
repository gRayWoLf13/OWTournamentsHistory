using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Api.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using System.Text;
using DA = OWTournamentsHistory.DataAccess.Model;
using DAType = OWTournamentsHistory.DataAccess.Model.Type;

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
        private readonly IMapper _mapper;
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<Team> _logger;

        public TeamController(IMapper mapper, ITeamRepository teamRepository, IPlayerRepository playerRepository, ILogger<Team> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Team>>> GetMany([FromQuery] int? skip = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
        {
            if (skip < 0 || limit < 0)
            {
                return BadRequest();
            }
            try
            {
                var results = await _teamRepository.GetSortedAsync(p => p.ExternalId, skip: skip, limit: limit, cancellationToken: cancellationToken);

                return results.Select(_mapper.Map<Team>).ToArray();
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
        public async Task<ActionResult<Team>> Get(int id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                return NotFound();
            }

            try
            {
                var result = await _teamRepository.GetAsync(id, cancellationToken);

                return _mapper.Map<Team>(result);
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
        public async Task<ActionResult> Add([FromBody] Team team, CancellationToken cancellationToken)
        {
            try
            {
                var generatedId = await _teamRepository.AddAsync(_mapper.Map<DA.Team>(team), cancellationToken);
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
                await _teamRepository.RemoveAsync(id, cancellationToken);
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
            var allPlayers = await _playerRepository.GetAsync(cancellationToken);

            var playerNamesById = allPlayers
                .ToDictionary(player => player.ExternalId, player => player.Name);
            var playerIdByBattleTags = allPlayers
                .SelectMany(player => player.BattleTags, (pl, tag) => (Name: NameExtensions.GetName(tag).ToLowerInvariant(), pl.ExternalId))
                .DistinctBy(pair => pair.Name)
                .ToArray();
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
                    .TakeWhile(x => x.SelectSingleNode("td[1]").InnerText.Length > 0)
                    .Select(node => new
                    {
                        //Team fields
                        ExternalId = long.Parse(node.SelectSingleNode("td[2]").InnerText),
                        TournamentNumber = int.Parse(node.SelectSingleNode("td[1]").InnerText),
                        CaptainName = node.SelectSingleNode("td[5]").InnerText.Replace("Team ", "").Replace(" ", "").Replace(Environment.NewLine, ""),
                        Place = int.Parse(node.SelectSingleNode("td[13]").InnerText),
                        MatchesPlayed = int.Parse(node.SelectSingleNode("td[14]").InnerText),
                        MapsWon = int.Parse(node.SelectSingleNode("td[15]").InnerText),
                        MapsPlayed = int.Parse(node.SelectSingleNode("td[16]").InnerText),
                        AverageMatchesCloseScore = NullableExtensions.ParseTo<decimal>(node.SelectSingleNode("td[19]").InnerText),

                        //Player fields
                        PlayerId = playerIdByBattleTags.Single(pair => pair.Name == node.SelectSingleNode("td[6]").InnerText.Replace(" ", "").Replace(Environment.NewLine, "").ToLowerInvariant()).ExternalId,
                        PlayerBattleTag = node.SelectSingleNode("td[6]").InnerText.Replace(" ", "").Replace(Environment.NewLine, ""),
                        TeamPlayerRole = ParseRole(node.SelectSingleNode("td[7]").InnerText),
                        Weight = NullableExtensions.ParseTo<decimal>(node.SelectSingleNode("td[9]").InnerText),
                        Division = NullableExtensions.ParseTo<int>(node.SelectSingleNode("td[11]").InnerText),
                        DisplayWeight = node.SelectSingleNode("td[10]").InnerText,
                        PlayerSubRole = ParseSubRole(node.SelectSingleNode("td[12]").InnerText),
                        IsNewPlayer = node.SelectSingleNode("td[22]").InnerText == "1",
                        IsNewRole = node.SelectSingleNode("td[23]").InnerText == "1",
                        WeightShift = decimal.Parse(node.SelectSingleNode("td[24]").InnerText)
                    })
                   .GroupBy(t => new { t.ExternalId, t.TournamentNumber, t.CaptainName, t.Place, t.MatchesPlayed, t.MapsWon, t.MapsPlayed, t.AverageMatchesCloseScore })
                   .Select(t => new DA.Team
                   {
                       ExternalId = t.Key.ExternalId,
                       TournamentNumber = t.Key.TournamentNumber,
                       CaptainName = t.Key.CaptainName,
                       Place = t.Key.Place,
                       MatchesPlayed = t.Key.MatchesPlayed,
                       MapsWon = t.Key.MapsWon,
                       MapsPlayed = t.Key.MapsPlayed,
                       AverageMatchesCloseScore = t.Key.AverageMatchesCloseScore,
                       Players = t.Select(p => new DA.TeamPlayerInfo
                       {
                           Name = playerNamesById[p.PlayerId],
                           BattleTag = p.PlayerBattleTag,
                           Role = p.TeamPlayerRole,
                           Weight = p.Weight,
                           Division = p.Division,
                           DisplayWeight = p.DisplayWeight,
                           SubRole = p.PlayerSubRole,
                           IsNewPlayer = p.IsNewPlayer,
                           IsNewRole = p.IsNewRole,
                           WeightShift = p.WeightShift,
                       }).ToArray()
                   })
                   .ToArray();

                await _teamRepository.AddRangeAsync(parsedData, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpPut]
        [Route("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ValidateTeams(CancellationToken cancellationToken)
        {
            var existingPlayers = (await _playerRepository.GetAsync(cancellationToken)).Select(player => player.Name).ToArray();
            var existingBtags = (await _playerRepository.GetAsync(cancellationToken)).SelectMany(player => player.BattleTags).Select(tag => NameExtensions.GetName(tag)).ToArray();
            var teamPlayers = (await _teamRepository.GetAsync(cancellationToken)).SelectMany(t => t.Players).Select(player => player.Name).Distinct().ToArray();

            var missingPlayers = teamPlayers.Except(existingPlayers).ToArray();
            var playersMissingInTeam = existingPlayers.Except(teamPlayers).ToArray();

            var missingPlayerbyBtag = teamPlayers.Except(existingBtags).ToArray();

            return Ok();
        }

        private DAType.TeamPlayerRole ParseRole(string role) =>
            role.ToLower() switch
            {
                "flex" => DAType.TeamPlayerRole.Flex,
                "tank" => DAType.TeamPlayerRole.Tank,
                "dps" => DAType.TeamPlayerRole.Dps,
                "support" => DAType.TeamPlayerRole.Support,
                _ => throw new Exception($"Unexpected TeamPlayerRole: {role}")
            };

        private DAType.TeamPlayerSubRole? ParseSubRole(string role) =>
          string.IsNullOrEmpty(role) ? null : role.ToLower() switch
          {
              "main tank" => DAType.TeamPlayerSubRole.MainTank,
              "off tank" => DAType.TeamPlayerSubRole.OffTank,
              "hitscan" => DAType.TeamPlayerSubRole.HitscanDps,
              "projectile" => DAType.TeamPlayerSubRole.ProjectileDps,
              "main heal" => DAType.TeamPlayerSubRole.MainHeal,
              "light heal" => DAType.TeamPlayerSubRole.LightHeal,
              _ => throw new Exception($"Unexpected TeamPlayerSubRole: {role}")
          };

        private static ObjectResult WrapException(Exception ex)
            => new(ex.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
    }
}
