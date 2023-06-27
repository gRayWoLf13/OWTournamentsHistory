using AutoMapper;
using Grpc.Core;
using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Api.Services;
using System.Diagnostics;

namespace OWTournamentsHistory.Api.GrpcServices
{
    public class TeamsHandlerService : TeamsHandler.TeamsHandlerBase
    {
        private readonly TeamsService _teamsService;
        private readonly IMapper _mapper;
        private readonly ILogger<StatisticsHandlerService> _logger;

        public TeamsHandlerService(TeamsService teamsService, IMapper mapper, ILogger<StatisticsHandlerService> logger)
        {
            _teamsService = teamsService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<TeamsGetManyResponse> GetMany(TeamsGetManyRequest request, ServerCallContext context)
        {
            var data = await _teamsService.GetMany(request.Skip, request.Limit, context.CancellationToken);
            var result = new TeamsGetManyResponse();
            result.Teams.AddRange(data.Select(_mapper.Map<Team>));
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }

        public override async Task<TeamsGetResponse> Get(TeamsGetRequest request, ServerCallContext context)
        {
            var data = await _teamsService.Get(request.Id, context.CancellationToken);
            var result = new TeamsGetResponse
            {
                Team = _mapper.Map<Team>(data)
            };
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
        public override async Task<TeamsAddResponse> Add(TeamsAddRequest request, ServerCallContext context)
        {
            var team = _mapper.Map<Contract.Model.Team>(request.Team);
            var teamId = await _teamsService.Add(team, context.CancellationToken);
            var result = new TeamsAddResponse
            { 
                GeneratedId = teamId 
            };
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
        public override async Task<TeamsDeleteResponse> Delete(TeamsDeleteRequest request, ServerCallContext context)
        {
            await _teamsService.Delete(request.Id, context.CancellationToken);
            return new TeamsDeleteResponse();
        }
        public override async Task<TeamsImportFromHtmlResponse> ImportFromHtml(TeamsImportFromHtmlRequest request, ServerCallContext context)
        {
            await _teamsService.ImportFromHtml(request.Html, context.CancellationToken);
            return new TeamsImportFromHtmlResponse();
        }
        public override async Task<TeamsValidateResponse> Validate(TeamsValidateRequest request, ServerCallContext context)
        {
            await _teamsService.ValidateTeams(context.CancellationToken);
            return new TeamsValidateResponse();
        }
    }
}
