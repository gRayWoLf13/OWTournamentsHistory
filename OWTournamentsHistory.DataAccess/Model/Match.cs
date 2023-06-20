namespace OWTournamentsHistory.DataAccess.Model
{
    public class Match : MongoCollectionEntry
    {
        public int TournamentNumber { get; set; }
        public int MatchNumber { get; set; }
        public required string Team1CaptainName { get; set; }
        public required string Team2CaptainName { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2 { get; set; }
        public decimal? Closeness { get; set; }
    }
}
