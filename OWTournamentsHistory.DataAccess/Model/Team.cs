namespace OWTournamentsHistory.DataAccess.Model
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Team : MongoCollectionEntry
    {
        public int TournamentNumber { get; set; }
        public int Place { get; set; }
        public string CaptainName { get; set; }
        public int MatchesPlayed { get; set; }
        public int MapsPlayed { get; set; }
        public int MapsWon { get; set; }
        public decimal? AverageMatchesCloseScore { get; set; }
        public ICollection<TeamPlayerInfo> Players { get; set; }
    }
}
