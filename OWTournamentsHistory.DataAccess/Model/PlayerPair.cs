namespace OWTournamentsHistory.DataAccess.Model
{
    public class PlayerPair : MongoCollectionEntry
    {
        public required string Player1 { get; set; }
        public required string Player2 { get; set; }
        public int MapsWon { get; set; }
        public int MapsPlayed { get; set; }
    }
}
