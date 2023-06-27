using AutoMapper;
using OWTournamentsHistory.Api.MappingProfiles.Helpers;
using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Contract.Model.PlayerStatistics;
using ApiModel = OWTournamentsHistory.Contract.Model;

namespace OWTournamentsHistory.Api.MappingProfiles.Grpc
{
    public class StatisticsMappingProfile : Profile
    {
        public StatisticsMappingProfile()
        {
            CreateMap<ApiModel.TournamentStatistics.TeamStatistics, TournamentStatisticsResponse.Types.TeamStats>()
                .ForMember(dest => dest.Players, opt => opt.Ignore())
                .AfterMap(ConvertTeamStatisticsPlayers);

            CreateMap<PlayerStatisticsInfo, PlayerStatisticsResponse>()
                .ForMember(dest => dest.CombinationsData, opt => opt.MapFrom(ConvertPlayerStatisticsCombinationsData))
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertPlayerStatisticsChartsData))

                .ForMember(dest => dest.RoleInfos, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsRoleInfos)
                .ForMember(dest => dest.BattleTags, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsBattleTags)
                .ForMember(dest => dest.Teams, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsTeams);

            CreateMap<ApiModel.TournamentStatistics.TournamentStatisticsInfo, TournamentStatisticsResponse>()
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertTournamentStatisticsChartsData))
                .ForMember(dest => dest.TopTeams, opt => opt.Ignore())
                .AfterMap(ConvertTournamentStatisticsTeams);

            CreateMap<ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo, GeneralTournamentStatisticsInfoResponse>()
                .ForMember(dest => dest.OtherStats, opt => opt.MapFrom(ConvertGeneralTournamentInfoOtherStats))
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertGeneralTournamentInfoChartsData));
        }
        #region Player statistics

        private void ConvertPlayerStatisticsTeams(PlayerStatisticsInfo info, PlayerStatisticsResponse response)
        {
            response.Teams.AddRange(info.Teams.Select(item =>
            {
                var team = new PlayerStatisticsResponse.Types.TeamInfo
                {
                    TournamentNumber = item.TournamentNumber,
                    AverageMatchesCloseScore = item.AverageMatchesCloseScore,
                    CaptainName = item.CaptainName,
                    MapsPlayed = item.MapsPlayed,
                    MapsWon = item.MapsWon,
                    MatchesPlayed = item.MatchesPlayed,
                    Place = item.Place,
                    WinRate = item.WinRate,
                };

                team.TeamMatches.AddRange(item.TeamMatches.Select(m => new PlayerStatisticsResponse.Types.TeamInfo.Types.MatchInfo
                {
                    CaptainTeam1 = m.CaptainTeam1,
                    CaptainTeam2 = m.CaptainTeam2,
                    Closeness = m.Closeness,
                    MatchName = m.MatchName,
                    Score = m.Score,
                    ScoreTeam1 = m.ScoreTeam1,
                    ScoreTeam2 = m.ScoreTeam2,
                }));

                team.TeamMembers.AddRange(item.TeamMembers.Select(ConvertTeamPlayerInfo));
                return team;
            }));
        }

        private void ConvertPlayerStatisticsBattleTags(PlayerStatisticsInfo info, PlayerStatisticsResponse response)
        {
            response.BattleTags.AddRange(info.BattleTags);
        }

        private void ConvertPlayerStatisticsRoleInfos(PlayerStatisticsInfo info, PlayerStatisticsResponse response)
        {
            response.RoleInfos.AddRange(info.RoleInfos.Select(item => new PlayerStatisticsResponse.Types.RoleInfo
            {
                Role = item.Key.ToGrpcModel(),
                AverageWinRate = item.Value.AverageWinrate,
                LastDisplayWeight = item.Value.LastDisplayWeight,
                LastDivision = item.Value.LastDivision,
                TournamentsPlayed = item.Value.TournamentsPlayed
            }));
        }

        private PlayerStatisticsResponse.Types.CombinationsData ConvertPlayerStatisticsCombinationsData(PlayerStatisticsInfo info, PlayerStatisticsResponse _)
        {
            var result = new PlayerStatisticsResponse.Types.CombinationsData();
            //result.MostKnockoutsFrom
            //result.MostKnockoutsTo
            result.MostLossesAgaints.AddRange(info.MostLossesAgainst.Select(CommonMappings.ToGrpcModel));
            result.MostWinsAgainst.AddRange(info.MostWinsAgainst.Select(CommonMappings.ToGrpcModel));
            return result;
        }

        private PlayerStatisticsResponse.Types.ChartsData ConvertPlayerStatisticsChartsData(PlayerStatisticsInfo info, PlayerStatisticsResponse _)
        {
            var result = new PlayerStatisticsResponse.Types.ChartsData();
            result.TankPriceData.AddRange(info.TankPriceData.Select(CommonMappings.ToGrpcModel));
            result.DpsPriceData.AddRange(info.DpsPriceData.Select(CommonMappings.ToGrpcModel));
            result.SupportPriceData.AddRange(info.SupportPriceData.Select(CommonMappings.ToGrpcModel));
            result.PlaceData.AddRange(info.PlaceData.Select(CommonMappings.ToGrpcModel));
            result.StandardDeviation.AddRange(info.StandardDeviation.Select(CommonMappings.ToGrpcModel));
            return result;
        }

        #endregion

        #region Tournamnent statistics

        private void ConvertTeamStatisticsPlayers(ApiModel.TournamentStatistics.TeamStatistics info,
            TournamentStatisticsResponse.Types.TeamStats result)
        {
            result.Players.AddRange(info.Players.Select(ConvertTeamPlayerInfo));
        }

        private TournamentStatisticsResponse.Types.ChartsData ConvertTournamentStatisticsChartsData(ApiModel.TournamentStatistics.TournamentStatisticsInfo info,
            TournamentStatisticsResponse _)
        {
            var result = new TournamentStatisticsResponse.Types.ChartsData();
            result.PlayersToDivisions.AddRange(info.PlayersToDivisions.Select(CommonMappings.ToGrpcModel));
            result.GlobalPlayersToDivisions.AddRange(info.GlobalPlayersToDivisions.Select(CommonMappings.ToGrpcModel));
            result.MatchesClosenessRelativeToAverage.AddRange(info.MatchesClosenessRelativeToAverage.Select(CommonMappings.ToGrpcModel));
            return result;
        }

        private void ConvertTournamentStatisticsTeams(ApiModel.TournamentStatistics.TournamentStatisticsInfo info,
            TournamentStatisticsResponse result)
        {
            result.TopTeams.AddRange(info.TopTeams.Select(t => new TournamentStatisticsResponse.Types.TeamInfo
            {
                CaptainName = t.CaptainName,
                Place = t.Place
            }));
        }

        #endregion

        #region General tournament statistics

        private GeneralTournamentStatisticsInfoResponse.Types.OtherStats ConvertGeneralTournamentInfoOtherStats(ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo info,
            GeneralTournamentStatisticsInfoResponse _)
        {
            var result = new GeneralTournamentStatisticsInfoResponse.Types.OtherStats();
            result.TopMapsWon.AddRange(info.TopMapsWon.Select(CommonMappings.ToGrpcModel));
            result.TopChampions.AddRange(info.TopChampions.Select(CommonMappings.ToGrpcModel));
            result.TopWinRate.AddRange(info.TopWinRate.Select(CommonMappings.ToGrpcModel));
            result.Top0Wins.AddRange(info.Top0Wins.Select(CommonMappings.ToGrpcModel));
            result.WorstDuos.AddRange(info.WorstDuos.Select(CommonMappings.ToGrpcModel));
            result.BestDuos.AddRange(info.BestDuos.Select(CommonMappings.ToGrpcModel));
            return result;
        }

        private GeneralTournamentStatisticsInfoResponse.Types.ChartsData ConvertGeneralTournamentInfoChartsData(ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo info,
           GeneralTournamentStatisticsInfoResponse _)
        {
            var result = new GeneralTournamentStatisticsInfoResponse.Types.ChartsData();
            result.PlayersCountToTournament.AddRange(info.PlayersCountToTournament.Select(CommonMappings.ToGrpcModel));
            result.AverageTeamWeightToTournament.AddRange(info.AverageTeamWeightToTournament.Select(CommonMappings.ToGrpcModel));
            result.AverageMatchClosenessToTournament.AddRange(info.AverageMatchClosenessToTournament.Select(CommonMappings.ToGrpcModel));

            result.AverageTankDivisionToTournament.AddRange(info.AverageTankDivisionToTournament.Select(CommonMappings.ToGrpcModel));
            result.AverageDpsDivisionToTournament.AddRange(info.AverageDpsDivisionToTournament.Select(CommonMappings.ToGrpcModel));
            result.AverageSupportDivisionToTournament.AddRange(info.AverageSupportDivisionToTournament.Select(CommonMappings.ToGrpcModel));

            result.TankPlayersToDivision.AddRange(info.TankPlayersToDivision.Select(CommonMappings.ToGrpcModel));
            result.DpsPlayersToDivision.AddRange(info.DpsPlayersToDivision.Select(CommonMappings.ToGrpcModel));
            result.SupportPlayersToDivision.AddRange(info.SupportPlayersToDivision.Select(CommonMappings.ToGrpcModel));

            return result;
        }

        #endregion

        private static TeamPlayerInfo ConvertTeamPlayerInfo(PlayerInTheTeamInfo item) => new TeamPlayerInfo
        {
            BattleTag = item.BattleTag,
            DisplayWeight = item.DisplayWeight,
            Division = item.Division,
            Name = item.Name,
            Role = item.Role.ToGrpcModel(),
            SubRole = item.SubRole.ToGrpcModel(),
        };
    }
}
