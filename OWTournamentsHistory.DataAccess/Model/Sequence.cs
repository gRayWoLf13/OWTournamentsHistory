using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OWTournamentsHistory.DataAccess.Model
{
    internal class Sequence
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [BsonId]
        public ObjectId _Id { get; set; }

        public string Name { get; set; }

        public long Value { get; set; }

        public void Insert(IMongoDatabase database)
        {
            var collection = database.GetCollection<Sequence>("Sequence");
            collection.InsertOne(this);
        }

        internal static long GetNextSequenceValue(string sequenceName, IMongoDatabase database)
        {
            var collection = database.GetCollection<Sequence>("Sequence");
            var filter = Builders<Sequence>.Filter.Eq(a => a.Name, sequenceName);
            var update = Builders<Sequence>.Update.Inc(a => a.Value, 1);
            var sequence = collection.FindOneAndUpdate(filter, update, new () { IsUpsert = true, ReturnDocument = ReturnDocument.After});

            return sequence.Value;
        }
    }
}
