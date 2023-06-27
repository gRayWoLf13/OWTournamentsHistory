using AutoMapper;
using HtmlAgilityPack;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.Services
{
    public class PlayersService : ControllerServiceBase<Player, DA.Player>
    {
        public PlayersService(IMapper mapper, IPlayerRepository playerRepository) : base(mapper, playerRepository)
        {
        }

        public override async Task ImportFromHtml(string html, CancellationToken cancellationToken)
        {
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

            await _repository.AddRangeAsync(parsedData, cancellationToken);
        }
    }
}
