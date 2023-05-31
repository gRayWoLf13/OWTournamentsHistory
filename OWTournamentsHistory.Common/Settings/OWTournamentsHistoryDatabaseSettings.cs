namespace OWTournamentsHistory.Common.Settings;

public class OWTournamentsHistoryDatabaseSettings
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string PlayersCollectionName { get; set; }
    public required string TeamsCollectionName { get; set; }
    public required string MatchesCollectionName { get; set; }
    public required string PlayerDuosCollectionName { get; set; }
    public required string PlayerOpponentsCollectionName { get; set; }
}
