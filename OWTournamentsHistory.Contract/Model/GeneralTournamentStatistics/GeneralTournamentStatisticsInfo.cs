using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.GeneralTournamentStatistics
{
    public class GeneralTournamentStatisticsInfo
    {
        public required int TournamentsCount { get; set; }
        public required int TeamsCount { get; set; }
        public required int PlayersCount { get; set; }
        public required int MatchesCount { get; set; }
        public required int OWALsCount { get; set; }
        public required int ChampionsCount { get; set; }

        #region ChartsData

        public required ICollection<Point2D<decimal>> PlayersCountToTournament { get; set; }
        public required ICollection<Point2D<decimal>> AverageTeamWeightToTournament { get; set; }
        public required ICollection<Point2D<decimal>> AverageMatchClosenessToTournament { get; set; }

        public required ICollection<Point2D<decimal>> AverageTankDivisionToTournament { get; set; }
        public required ICollection<Point2D<decimal>> AverageDpsDivisionToTournament { get; set; }
        public required ICollection<Point2D<decimal>> AverageSupportDivisionToTournament { get; set; }

        public required ICollection<Point2D<decimal>> TankPlayersToDivision { get; set; }
        public required ICollection<Point2D<decimal>> DpsPlayersToDivision { get; set; }
        public required ICollection<Point2D<decimal>> SupportPlayersToDivision { get; set; }


        #endregion
        public required ICollection<Point2D<string>> TopMapsWon { get; set; }
        public required ICollection<Point2DWithLabel<string>> TopChampions { get; set; }
        public required ICollection<Point2D<string>> TopWinRate { get; set; }
        public required ICollection<Point2DWithLabel<string>> Top0Wins { get; set; }
        public required ICollection<Point2D<string>> WorstDuos { get; set; }
        public required ICollection<Point2D<string>> BestDuos { get; set; }
    }
}
