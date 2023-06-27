using AutoMapper;
using OWTournamentsHistory.Api.Proto;
using ApiModel = OWTournamentsHistory.Contract.Model;

namespace OWTournamentsHistory.Api.MappingProfiles.Grpc
{
    public class PlayersMappingProfile : Profile
    {
        public PlayersMappingProfile()
        {
            CreateMap<ApiModel.Player, Player>()
                .ForMember(dest => dest.BattleTags, opt => opt.Ignore())
                .AfterMap(ConvertBattleTags);

            CreateMap<Player, ApiModel.Player>();
        }

        private void ConvertBattleTags(ApiModel.Player source, Player result)
        {
            result.BattleTags.AddRange(source.BattleTags);
        }
    }
}
