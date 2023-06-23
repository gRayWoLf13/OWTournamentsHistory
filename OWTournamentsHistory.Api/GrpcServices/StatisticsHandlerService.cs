using AutoMapper;
using Grpc.Core;
using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Api.Services;
using System.Diagnostics;

namespace OWTournamentsHistory.Api.GrpcServices
{
    public class StatisticsHandlerService : TournamentStatisticsHandler.TournamentStatisticsHandlerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<StatisticsHandlerService> _logger;
        private readonly StatisticsService _statisticsService;

        public StatisticsHandlerService(
            StatisticsService statisticsService,
            IMapper mapper,
            ILogger<StatisticsHandlerService> logger)
        {
            _statisticsService = statisticsService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<PlayerStatisticsResponse> GetPlayerStatistics(PlayerStatisticsRequest request, ServerCallContext context)
        {
            var result = _mapper.Map<PlayerStatisticsResponse>(await _statisticsService.GetPlayerStatistics(request.Name));
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }

        public override async Task<TournamentStatisticsResponse> GetTournamentStatistics(TournamentStatisticsRequest request, ServerCallContext context)
        {
            var result = _mapper.Map<TournamentStatisticsResponse>(await _statisticsService.GetTournamentStatistics(request.TournamentNumber));
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }

        public override async Task<GeneralTournamentStatisticsInfoResponse> GetGeneralTournamentStatistics(GeneralTournamentStatisticsInfoRequest request, ServerCallContext context)
        {
            var result = _mapper.Map<GeneralTournamentStatisticsInfoResponse>(await _statisticsService.GetGeneralTournamentStatisticsInfo());
            Debug.WriteLine($"Response size: {result.CalculateSize()}");
            return result;
        }
    }
}

