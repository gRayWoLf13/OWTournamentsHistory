using AutoMapper;
using ApiModel = OWTournamentsHistory.Contract.Model;
using DA = OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.MappingProfiles
{
    public class PlayerMappingProfile : Profile
    {
        public PlayerMappingProfile()
        {
            CreateMap<ApiModel.Player, DA.Player>()
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.ExternalId, opt => opt.MapFrom(src => src.Id));

            CreateMap<DA.Player, ApiModel.Player>()
                .ForMember(p => p.Id, opt => opt.MapFrom(src => src.ExternalId));
        }
    }
}
