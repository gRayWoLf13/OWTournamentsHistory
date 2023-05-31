namespace OWTournamentsHistory.DataAccess.Model
{
    public class PlayerPair : MongoCollectionEntry
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public int MapsWon { get; set; }
        public int MapsPlayed { get; set; }
    }
}
