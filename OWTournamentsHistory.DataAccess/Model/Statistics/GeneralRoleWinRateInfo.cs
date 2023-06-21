using OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.DataAccess.Model.Statistics
{
    public class GeneralRoleWinRateInfo
    {
        public TeamPlayerRole Role { get; set; }
        public decimal AverageWinRate { get; set; }
    }
}
