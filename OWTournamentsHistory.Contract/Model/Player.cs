namespace OWTournamentsHistory.Contract.Model
{
    public class Player
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required ICollection<string> BattleTags { get; set; }
        public required string TwitchId { get; set; }
    }
}
