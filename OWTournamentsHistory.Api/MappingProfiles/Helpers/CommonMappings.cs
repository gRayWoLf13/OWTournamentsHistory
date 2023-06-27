using OWTournamentsHistory.Api.Proto;
using OWTournamentsHistory.Contract.Model.Type;
using ApiModel = OWTournamentsHistory.Contract.Model;
using GrpcModel = OWTournamentsHistory.Api.Proto;

namespace OWTournamentsHistory.Api.MappingProfiles.Helpers
{
    internal static class CommonMappings
    {
        public static GrpcModel.PlayerRole ToGrpcModel(this ApiModel.Type.PlayerRole value) => value switch
        {
            ApiModel.Type.PlayerRole.Tank => GrpcModel.PlayerRole.Tank,
            ApiModel.Type.PlayerRole.Dps => GrpcModel.PlayerRole.Dps,
            ApiModel.Type.PlayerRole.Support => GrpcModel.PlayerRole.Support,
            ApiModel.Type.PlayerRole.Flex => GrpcModel.PlayerRole.Flex,
            _ => throw new ArgumentException($"Unexpected {typeof(ApiModel.Type.PlayerRole)} value: {value}")
        };

        public static GrpcModel.PlayerSubRole ToGrpcModel(this ApiModel.Type.PlayerSubRole? value) => value switch
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


        public static ApiModel.Type.PlayerRole FromGrpcModel(this GrpcModel.PlayerRole value) => value switch
        {
            GrpcModel.PlayerRole.Tank => ApiModel.Type.PlayerRole.Tank,
            GrpcModel.PlayerRole.Dps => ApiModel.Type.PlayerRole.Dps,
            GrpcModel.PlayerRole.Support => ApiModel.Type.PlayerRole.Support,
            GrpcModel.PlayerRole.Flex => ApiModel.Type.PlayerRole.Flex,
            _ => throw new ArgumentException($"Unexpected {typeof(GrpcModel.PlayerRole)} value: {value}")
        };

        public static ApiModel.Type.PlayerSubRole? FromGrpcModel(this GrpcModel.PlayerSubRole value) => value switch
        {
            GrpcModel.PlayerSubRole.MainTank => ApiModel.Type.PlayerSubRole.MainTank,
            GrpcModel.PlayerSubRole.OffTank => ApiModel.Type.PlayerSubRole.OffTank,
            GrpcModel.PlayerSubRole.ProjectileDps => ApiModel.Type.PlayerSubRole.ProjectileDps,
            GrpcModel.PlayerSubRole.HitscanDps => ApiModel.Type.PlayerSubRole.HitscanDps,
            GrpcModel.PlayerSubRole.MainHeal => ApiModel.Type.PlayerSubRole.MainHeal,
            GrpcModel.PlayerSubRole.LightHeal => ApiModel.Type.PlayerSubRole.LightHeal,
            GrpcModel.PlayerSubRole.UnknownPlayerSubRole => null,
            _ => throw new ArgumentException($"Unexpected {typeof(GrpcModel.PlayerSubRole)} value: {value}")
        };

        public static Point2D ToGrpcModel<T>(this Point2D<T> source)
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

        public static Point2DWithLabel ToGrpcModel<T>(this Point2DWithLabel<T> source)
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

        public static PlayerStatisticsResponse.Types.CombinationsData.Types.NameCount ToGrpcModel(this ApiModel.PlayerStatistics.NameCount value) =>
            new PlayerStatisticsResponse.Types.CombinationsData.Types.NameCount
            {
                Name = value.Name,
                Count = value.Count
            };
    }
}
