using AutoMapper;
using HtmlAgilityPack;
using OWTournamentsHistory.Api.Services.Contract.Exceptions;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model;
using OWTournamentsHistory.DataAccess.Contract;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.Services
{
    public class MatchesService
    {
        private readonly IMapper _mapper;
        private readonly IMatchRepository _matchRepository;

        public MatchesService(IMapper mapper, IMatchRepository matchRepository)
        {
            _mapper = mapper;
            _matchRepository = matchRepository;
        }

        public async Task<IReadOnlyCollection<Match>> GetMany(int? skip = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            if (skip < 0 || limit < 0)
            {
                throw new InvalidParametersException();
            }
            var results = await _matchRepository.GetSortedAsync(p => p.ExternalId, skip: skip, limit: limit, cancellationToken: cancellationToken);

            return results.Select(_mapper.Map<Match>).ToArray();
        }

        public async Task<Match> Get(long id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                throw new NotFoundException($"Match (id:{id}) was not found");
            }
            var result = await _matchRepository.GetAsync(id, cancellationToken);
            return _mapper.Map<Match>(result);
        }

        public async Task<long> Add(Match match, CancellationToken cancellationToken)
        {
            var generatedId = await _matchRepository.AddAsync(_mapper.Map<DA.Match>(match), cancellationToken);
            return generatedId;
        }

        public async Task Delete(long id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                throw new NotFoundException($"Match (id:{id}) was not found");
            }
            await _matchRepository.RemoveAsync(id, cancellationToken);
        }

        public async Task ImportFromHtml(string html, CancellationToken cancellationToken)
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

            await _matchRepository.AddRangeAsync(parsedData, cancellationToken);
        }
    }
}
