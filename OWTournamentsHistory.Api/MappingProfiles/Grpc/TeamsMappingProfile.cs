using AutoMapper;
using OWTournamentsHistory.Api.MappingProfiles.Helpers;
using OWTournamentsHistory.Api.Proto;
using ApiModel = OWTournamentsHistory.Contract.Model;

namespace OWTournamentsHistory.Api.MappingProfiles.Grpc
{
    public class TeamsMappingProfile : Profile
    {
        public TeamsMappingProfile()
        {
            CreateMap<Team.Types.PlayerInfo, ApiModel.TeamPlayerInfo>()
                .ConvertUsing(pi => ConvertPlayerInfo(pi));

            CreateMap<ApiModel.Team, Team>()
                .ForMember(dest => dest.Players, opt => opt.Ignore())
                .AfterMap(ConvertTeamPlayers);

            CreateMap<Team, ApiModel.Team>();
        }

        private void ConvertTeamPlayers(ApiModel.Team source, Team result)
        {
            result.Players.AddRange(source.Players.Select(ConvertPlayerInfo));
        }

        private Team.Types.PlayerInfo ConvertPlayerInfo(ApiModel.TeamPlayerInfo info) =>
            new()
            {
                Name = info.Name,
                BattleTag = info.BattleTag,
                DisplayWeight = info.DisplayWeight,
                Division = info.Division,
                IsNewPlayer = info.IsNewPlayer,
                IsNewRole = info.IsNewRole,
                PlayerRole = info.Role.ToGrpcModel(),
                SubRole = info.SubRole.ToGrpcModel(),
                Weight = info.Weight,
                WeightShift = info.WeightShift,
            };

        private ApiModel.TeamPlayerInfo ConvertPlayerInfo(Team.Types.PlayerInfo info) =>
           new()
           {
               Name = info.Name,
               BattleTag = info.BattleTag,
               DisplayWeight = info.DisplayWeight,
               Division = info.Division,
               IsNewPlayer = info.IsNewPlayer,
               IsNewRole = info.IsNewRole,
               Role = info.PlayerRole.FromGrpcModel(),
               SubRole = info.SubRole.FromGrpcModel(),
               Weight = info.Weight,
               WeightShift = info.WeightShift,
           };
    }
}
