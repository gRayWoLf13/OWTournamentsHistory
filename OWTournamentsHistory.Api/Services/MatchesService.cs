using AutoMapper;
using HtmlAgilityPack;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.Services
{
    public class MatchesService : ControllerServiceBase<Match, DA.Match>
    {
        public MatchesService(IMapper mapper, IMatchRepository matchRepository) : base(mapper, matchRepository)
        {
        }

        public override async Task ImportFromHtml(string html, CancellationToken cancellationToken)
        {
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

            await _repository.AddRangeAsync(parsedData, cancellationToken);
        }
    }
}
