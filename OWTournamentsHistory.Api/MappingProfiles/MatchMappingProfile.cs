using AutoMapper;
using ApiModel = OWTournamentsHistory.Contract.Model;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.MappingProfiles
{
    public class MatchMappingProfile : Profile
    {
        public MatchMappingProfile()
        {
            CreateMap<ApiModel.Match, DA.Match>()
                .ForMember(m => m.Id, opt => opt.Ignore())
                .ForMember(m => m.ExternalId, opt => opt.MapFrom(src => src.Id));

            CreateMap<DA.Match, ApiModel.Match>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => src.ExternalId));
        }
    }
}
