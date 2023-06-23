using AutoMapper;
using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Contract.Model.PlayerStatistics;
using OWTournamentsHistory.Contract.Model.Type;
using ApiModel = OWTournamentsHistory.Contract.Model;
using GrpcModel = OWTournamentsHistory.Api.Proto;

namespace OWTournamentsHistory.Api.MappingProfiles
{
    public class GrpcMappingProfile : Profile
    {
        public GrpcMappingProfile()
        {
            CreateMap<ApiModel.TournamentStatistics.TeamStatistics, GrpcModel.TournamentStatisticsResponse.Types.TeamStats>()
                .ForMember(dest => dest.Players, opt => opt.Ignore())
                .AfterMap(ConvertTeamStatisticsPlayers);

            CreateMap<ApiModel.PlayerStatistics.PlayerStatisticsInfo, GrpcModel.PlayerStatisticsResponse>()
                .ForMember(dest => dest.CombinationsData, opt => opt.MapFrom(ConvertPlayerStatisticsCombinationsData))
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertPlayerStatisticsChartsData))

                .ForMember(dest => dest.RoleInfos, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsRoleInfos)
                .ForMember(dest => dest.BattleTags, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsBattleTags)
                .ForMember(dest => dest.Teams, opt => opt.Ignore())
                .AfterMap(ConvertPlayerStatisticsTeams);

            CreateMap<ApiModel.TournamentStatistics.TournamentStatisticsInfo, GrpcModel.TournamentStatisticsResponse>()
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertTournamentStatisticsChartsData))
                .ForMember(dest => dest.TopTeams, opt => opt.Ignore())
                .AfterMap(ConvertTournamentStatisticsTeams);

            CreateMap<ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo, GrpcModel.GeneralTournamentStatisticsInfoResponse>()
                .ForMember(dest => dest.OtherStats, opt => opt.MapFrom(ConvertGeneralTournamentInfoOtherStats))
                .ForMember(dest => dest.ChartsData, opt => opt.MapFrom(ConvertGeneralTournamentInfoChartsData));
        }
        #region Player statistics

        private void ConvertPlayerStatisticsTeams(ApiModel.PlayerStatistics.PlayerStatisticsInfo info, PlayerStatisticsResponse response)
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

        private void ConvertPlayerStatisticsBattleTags(ApiModel.PlayerStatistics.PlayerStatisticsInfo info, PlayerStatisticsResponse response)
        {
            response.BattleTags.AddRange(info.BattleTags);
        }


        private void ConvertPlayerStatisticsRoleInfos(ApiModel.PlayerStatistics.PlayerStatisticsInfo info, GrpcModel.PlayerStatisticsResponse response)
        {
            response.RoleInfos.AddRange(info.RoleInfos.Select(item => new PlayerStatisticsResponse.Types.RoleInfo
            {
                Role = ConvertPlayerRole(item.Key),
                AverageWinRate = item.Value.AverageWinrate,
                LastDisplayWeight = item.Value.LastDisplayWeight,
                LastDivision = item.Value.LastDivision,
                TournamentsPlayed = item.Value.TournamentsPlayed
            }));
        }

        private GrpcModel.PlayerStatisticsResponse.Types.CombinationsData ConvertPlayerStatisticsCombinationsData(ApiModel.PlayerStatistics.PlayerStatisticsInfo info, GrpcModel.PlayerStatisticsResponse _)
        {
            var result = new GrpcModel.PlayerStatisticsResponse.Types.CombinationsData();
            //result.MostKnockoutsFrom
            //result.MostKnockoutsTo
            result.MostLossesAgaints.AddRange(info.MostLossesAgainst.Select(ConvertNameCount));
            result.MostWinsAgainst.AddRange(info.MostWinsAgainst.Select(ConvertNameCount));
            return result;
        }

        private GrpcModel.PlayerStatisticsResponse.Types.ChartsData ConvertPlayerStatisticsChartsData(ApiModel.PlayerStatistics.PlayerStatisticsInfo info, GrpcModel.PlayerStatisticsResponse _)
        {
            var result = new GrpcModel.PlayerStatisticsResponse.Types.ChartsData();
            result.TankPriceData.AddRange(info.TankPriceData.Select(ConvertPoint));
            result.DpsPriceData.AddRange(info.DpsPriceData.Select(ConvertPoint));
            result.SupportPriceData.AddRange(info.SupportPriceData.Select(ConvertPoint));
            result.PlaceData.AddRange(info.PlaceData.Select(ConvertPoint));
            result.StandardDeviation.AddRange(info.StandardDeviation.Select(ConvertPoint));
            return result;
        }

        #endregion

        #region Tournamnent statistics

        private void ConvertTeamStatisticsPlayers(ApiModel.TournamentStatistics.TeamStatistics info,
            GrpcModel.TournamentStatisticsResponse.Types.TeamStats result)
        {
            result.Players.AddRange(info.Players.Select(ConvertTeamPlayerInfo));
        }

        private GrpcModel.TournamentStatisticsResponse.Types.ChartsData ConvertTournamentStatisticsChartsData(ApiModel.TournamentStatistics.TournamentStatisticsInfo info,
            GrpcModel.TournamentStatisticsResponse _)
        {
            var result = new GrpcModel.TournamentStatisticsResponse.Types.ChartsData();
            result.PlayersToDivisions.AddRange(info.PlayersToDivisions.Select(ConvertPoint));
            result.GlobalPlayersToDivisions.AddRange(info.GlobalPlayersToDivisions.Select(ConvertPoint));
            result.MatchesClosenessRelativeToAverage.AddRange(info.MatchesClosenessRelativeToAverage.Select(ConvertPoint));
            return result;
        }

        private void ConvertTournamentStatisticsTeams(ApiModel.TournamentStatistics.TournamentStatisticsInfo info,
            GrpcModel.TournamentStatisticsResponse result)
        {
            result.TopTeams.AddRange(info.TopTeams.Select(t => new TournamentStatisticsResponse.Types.TeamInfo
            {
                CaptainName = t.CaptainName,
                Place = t.Place
            }));
        }

        #endregion

        #region General tournament statistics

        private GrpcModel.GeneralTournamentStatisticsInfoResponse.Types.OtherStats ConvertGeneralTournamentInfoOtherStats(ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo info,
            GrpcModel.GeneralTournamentStatisticsInfoResponse _)
        {
            var result = new GrpcModel.GeneralTournamentStatisticsInfoResponse.Types.OtherStats();
            result.TopMapsWon.AddRange(info.TopMapsWon.Select(ConvertPoint));
            result.TopChampions.AddRange(info.TopChampions.Select(ConvertPoint));
            result.TopWinRate.AddRange(info.TopWinRate.Select(ConvertPoint));
            result.Top0Wins.AddRange(info.Top0Wins.Select(ConvertPoint));
            result.WorstDuos.AddRange(info.WorstDuos.Select(ConvertPoint));
            result.BestDuos.AddRange(info.BestDuos.Select(ConvertPoint));
            return result;
        }

        private GrpcModel.GeneralTournamentStatisticsInfoResponse.Types.ChartsData ConvertGeneralTournamentInfoChartsData(ApiModel.GeneralTournamentStatistics.GeneralTournamentStatisticsInfo info,
           GrpcModel.GeneralTournamentStatisticsInfoResponse _)
        {
            var result = new GrpcModel.GeneralTournamentStatisticsInfoResponse.Types.ChartsData();
            result.PlayersCountToTournament.AddRange(info.PlayersCountToTournament.Select(ConvertPoint));
            result.AverageTeamWeightToTournament.AddRange(info.AverageTeamWeightToTournament.Select(ConvertPoint));
            result.AverageMatchClosenessToTournament.AddRange(info.AverageMatchClosenessToTournament.Select(ConvertPoint));

            result.AverageTankDivisionToTournament.AddRange(info.AverageTankDivisionToTournament.Select(ConvertPoint));
            result.AverageDpsDivisionToTournament.AddRange(info.AverageDpsDivisionToTournament.Select(ConvertPoint));
            result.AverageSupportDivisionToTournament.AddRange(info.AverageSupportDivisionToTournament.Select(ConvertPoint));

            result.TankPlayersToDivision.AddRange(info.TankPlayersToDivision.Select(ConvertPoint));
            result.DpsPlayersToDivision.AddRange(info.DpsPlayersToDivision.Select(ConvertPoint));
            result.SupportPlayersToDivision.AddRange(info.SupportPlayersToDivision.Select(ConvertPoint));

            return result;
        }

        #endregion

        private static TeamPlayerInfo ConvertTeamPlayerInfo(PlayerInTheTeamInfo item) => new TeamPlayerInfo
        {
            BattleTag = item.BattleTag,
            DisplayWeight = item.DisplayWeight,
            Division = item.Division,
            Name = item.Name,
            Role = ConvertPlayerRole(item.Role),
            SubRole = ConvertPlayerSubRole(item.SubRole),
        };

        private static Point2D ConvertPoint<T>(Point2D<T> source)
        {
            var result = new Point2D { Y = source.Y };

            if (source.X is string xString)
            {
                result.XString = xString;
            }
            else if (source.X is int xInt)
            {
                result.XInt = xInt;
            }
            else if (source.X is decimal xDecimal)
            {
                result.XDecimal = xDecimal;
            }
            else
            {
                throw new ArgumentException($"Unexpected type of the {nameof(Point2D<T>.X)}: {typeof(T)}");
            }

            return result;
        }

        private static Point2DWithLabel ConvertPoint<T>(Point2DWithLabel<T> source)
        {
            var result = new Point2DWithLabel { Y = source.Y, Label = source.Label };

            if (source.X is string xString)
            {
                result.XString = xString;
            }
            else if (source.X is int xInt)
            {
                result.XInt = xInt;
            }
            else if (source.X is decimal xDecimal)
            {
                result.XDecimal = xDecimal;
            }
            else
            {
                throw new ArgumentException($"Unexpected type of the {nameof(Point2D<T>.X)}: {typeof(T)}");
            }
            return result;
        }

        private static GrpcModel.PlayerStatisticsResponse.Types.CombinationsData.Types.NameCount ConvertNameCount(NameCount value) =>
            new GrpcModel.PlayerStatisticsResponse.Types.CombinationsData.Types.NameCount
            {
                Name = value.Name,
                Count = value.Count
            };

        private static GrpcModel.PlayerRole ConvertPlayerRole(ApiModel.Type.PlayerRole value) => value switch
        {
            ApiModel.Type.PlayerRole.Tank => GrpcModel.PlayerRole.Tank,
            ApiModel.Type.PlayerRole.Dps => GrpcModel.PlayerRole.Dps,
            ApiModel.Type.PlayerRole.Support => GrpcModel.PlayerRole.Support,
            ApiModel.Type.PlayerRole.Flex => GrpcModel.PlayerRole.Flex,
            _ => throw new ArgumentException($"Unexpected {typeof(ApiModel.Type.PlayerRole)} value: {value}")
        };

        private static GrpcModel.PlayerSubRole ConvertPlayerSubRole(ApiModel.Type.PlayerSubRole? value) => value switch
        {
            ApiModel.Type.PlayerSubRole.MainTank => GrpcModel.PlayerSubRole.MainTank,
            ApiModel.Type.PlayerSubRole.OffTank => GrpcModel.PlayerSubRole.OffTank,
            ApiModel.Type.PlayerSubRole.ProjectileDps => GrpcModel.PlayerSubRole.ProjectileDps,
            ApiModel.Type.PlayerSubRole.HitscanDps => GrpcModel.PlayerSubRole.HitscanDps,
            ApiModel.Type.PlayerSubRole.MainHeal => GrpcModel.PlayerSubRole.MainHeal,
            ApiModel.Type.PlayerSubRole.LightHeal => GrpcModel.PlayerSubRole.LightHeal,
            null => GrpcModel.PlayerSubRole.UnknownPlayerSubRole,
            _ => throw new ArgumentException($"Unexpected {typeof(ApiModel.Type.PlayerSubRole)} value: {value}")
        };

    }
}
