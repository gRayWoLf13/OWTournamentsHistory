using AutoMapper;
using HtmlAgilityPack;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using DA = OWTournamentsHistory.DataAccess.Model;
using DAType = OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.Api.Services
{
    public class TeamsService : ControllerServiceBase<Team, DA.Team>
    {
        private readonly IMapper _mapper;
        private readonly IPlayerRepository _playerRepository;

        public TeamsService(IMapper mapper, ITeamRepository teamRepository, IPlayerRepository playerRepository) : base(mapper, teamRepository)
        {
            _playerRepository = playerRepository;
        }

        public override async Task ImportFromHtml(string html, CancellationToken cancellationToken)
        {
            var allPlayers = await _playerRepository.GetAsync(cancellationToken);

            var playerNamesById = allPlayers
                .ToDictionary(player => player.ExternalId, player => player.Name);
            var playerIdByBattleTags = allPlayers
                .SelectMany(player => player.BattleTags, (pl, tag) => (Name: NameExtensions.GetName(tag).ToLowerInvariant(), pl.ExternalId))
                .DistinctBy(pair => pair.Name)
                .ToArray();

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
                    AverageMatchesCloseScore = node.SelectSingleNode("td[19]").InnerText.ParseTo<decimal>(),

                    //Player fields
                    PlayerId = playerIdByBattleTags.Single(pair => pair.Name == node.SelectSingleNode("td[6]").InnerText.Replace(" ", "").Replace(Environment.NewLine, "").ToLowerInvariant()).ExternalId,
                    PlayerBattleTag = node.SelectSingleNode("td[6]").InnerText.Replace(" ", "").Replace(Environment.NewLine, ""),
                    TeamPlayerRole = ParseRole(node.SelectSingleNode("td[7]").InnerText),
                    Weight = node.SelectSingleNode("td[9]").InnerText.ParseTo<decimal>(),
                    Division = node.SelectSingleNode("td[11]").InnerText.ParseTo<int>(),
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

            await _repository.AddRangeAsync(parsedData, cancellationToken);
        }

        public async Task ValidateTeams(CancellationToken cancellationToken)
        {
            var existingPlayers = (await _playerRepository.GetAsync(cancellationToken)).Select(player => player.Name).ToArray();
            var existingBtags = (await _playerRepository.GetAsync(cancellationToken)).SelectMany(player => player.BattleTags).Select(NameExtensions.GetName).ToArray();
            var teamPlayers = (await _repository.GetAsync(cancellationToken)).SelectMany(t => t.Players).Select(player => player.Name).Distinct().ToArray();

            var missingPlayers = teamPlayers.Except(existingPlayers).ToArray();
            var playersMissingInTeam = existingPlayers.Except(teamPlayers).ToArray();

            var missingPlayerbyBtag = teamPlayers.Except(existingBtags).ToArray();
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
    }
}
