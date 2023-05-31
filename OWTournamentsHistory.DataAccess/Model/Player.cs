namespace OWTournamentsHistory.DataAccess.Model
{
    public class Player : MongoCollectionEntry
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
        public ICollection<string> BattleTags { get; set; }
        public string TwitchId { get; set; }
    }
}
