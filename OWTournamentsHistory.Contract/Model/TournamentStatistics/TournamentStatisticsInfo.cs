using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model.TournamentStatistics;

public class TournamentStatisticsInfo
{
    public int TournamentNumber { get; set; }
    public int TeamsCount { get; set; }
    public int PlayersCount { get; set; }
    public int MatchesCount { get; set; }
    public int MapsPlayed { get; set; }
    public int NewPlayers { get; set; }
    public int NewRolePlayers { get; set; }
    public required ICollection<TournamentTeamInfo> TopTeams { get; set; }
    public required TeamStatistics WinnerTeam { get; set; }
    public required ICollection<Point2DWithLabel<decimal>> MatchesClosenessRelativeToAverage { get; set; }
    public required ICollection<Point2D<decimal>> PlayersToDivisions { get; set; }
    public required ICollection<Point2D<decimal>> GlobalPlayersToDivisions { get; set; }

}

