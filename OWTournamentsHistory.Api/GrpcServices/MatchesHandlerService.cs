using AutoMapper;
using Grpc.Core;
using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Api.Services;
using System.Diagnostics;

namespace OWTournamentsHistory.Api.GrpcServices
{
    public class MatchesHandlerService : MatchesHandler.MatchesHandlerBase
    {
        private readonly MatchesService _matchesService;
        private readonly IMapper _mapper;
        private readonly ILogger<MatchesHandlerService> _logger;

        public MatchesHandlerService(MatchesService matchesService, IMapper mapper, ILogger<MatchesHandlerService> logger)
        {
            _matchesService = matchesService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<MatchesGetManyResponse> GetMany(MatchesGetManyRequest request, ServerCallContext context)
        {
            var data = await _matchesService.GetMany(request.Skip, request.Limit, context.CancellationToken);
            var result = new MatchesGetManyResponse();
            result.Matches.AddRange(data.Select(_mapper.Map<Match>));
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
        public override async Task<MatchesGetResponse> Get(MatchesGetRequest request, ServerCallContext context)
        {
            var data = await _matchesService.Get(request.Id, context.CancellationToken);
            var result = new MatchesGetResponse
            {
                Match = _mapper.Map<Match>(data)
            };
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
        public override async Task<MatchesAddResponse> Add(MatchesAddRequest request, ServerCallContext context)
        {
            var match = _mapper.Map<Contract.Model.Match>(request.Match);
            var matchId = await _matchesService.Add(match, context.CancellationToken);
            var result = new MatchesAddResponse
            {
                GeneratedId = matchId
            };
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
        public override async Task<MatchesDeleteResponse> Delete(MatchesDeleteRequest request, ServerCallContext context)
        {
            await _matchesService.Delete(request.Id, context.CancellationToken);
            return new MatchesDeleteResponse();
        }
        public override async Task<MatchesImportFromHtmlResponse> ImportFromHtml(MatchesImportFromHtmlRequest request, ServerCallContext context)
        {
            await _matchesService.ImportFromHtml(request.Html, context.CancellationToken);
            return new MatchesImportFromHtmlResponse();
        }
    }
}
