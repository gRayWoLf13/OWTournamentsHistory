namespace OWTournamentsHistory.DataAccess.Model
{
    public abstract class MongoCollectionEntry
    {
        protected MongoCollectionEntry()
        {
            Id = Guid.Empty;
            LastModified = DateTimeOffset.UtcNow;
        }
        public Guid Id { get; set; }
        public long ExternalId { get; set; }
        public DateTimeOffset LastModified { get; set; }
    }
}
