namespace OWTournamentsHistory.DataAccess.Model
{
    public class Match : MongoCollectionEntry
    {
        public int TournamentNumber { get; set; }
        public int MatchNumber { get; set; }
        public string Team1CaptainName { get; set; }
        public string Team2CaptainName { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2 { get; set; }
        public decimal? Closeness { get; set; }
    }
}
