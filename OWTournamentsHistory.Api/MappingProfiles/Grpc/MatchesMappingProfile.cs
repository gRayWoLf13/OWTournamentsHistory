using AutoMapper;
using OWTournamentsHistory.Api.MappingProfiles.Helpers;
using OWTournamentsHistory.Api.Proto;
using ApiModel = OWTournamentsHistory.Contract.Model;

namespace OWTournamentsHistory.Api.MappingProfiles.Grpc
{
    public class MatchesMappingProfile : Profile
    {
        public MatchesMappingProfile()
        {
            CreateMap<ApiModel.Match, Match>()
                .ForMember(dest => dest.MatchCloseness, opt => opt.MapFrom(src => src.MatchCloseness.ToGrpcModel()));

            CreateMap<Match, ApiModel.Match>()
                .ForMember(dest => dest.MatchCloseness, opt => opt.MapFrom(src => src.MatchCloseness.FromGrpcModel()));
        }
    }
}
