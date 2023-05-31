using AutoMapper;
using ApiModel = OWTournamentsHistory.Contract.Model;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.MappingProfiles
{
    public class TeamMappingProfile : Profile
    {
        public TeamMappingProfile()
        {
            CreateMap<ApiModel.Team, DA.Team>()
                .ForMember(t => t.Id, opt => opt.Ignore())
                .ForMember(t => t.ExternalId, opt => opt.MapFrom(src => src.Id));
            CreateMap<ApiModel.TeamPlayerInfo, DA.TeamPlayerInfo>();

            CreateMap<DA.Team, ApiModel.Team>()
                .ForMember(p => p.Id, opt => opt.MapFrom(src => src.ExternalId));
            CreateMap<DA.TeamPlayerInfo, ApiModel.TeamPlayerInfo>();
        }
    }
}
