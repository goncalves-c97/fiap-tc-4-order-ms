using MongoDB.Bson;
using MongoDB.Driver;

namespace Infra.Data.MongoDb
{
    public static class DatabaseInitializer
    {
        // For MongoDB, ensure certain collections and indexes exist (e.g., counters for sequences)
        public static bool EnsureDatabaseExists(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName);

            // create counters collection if not exists
            var filter = new BsonDocument();
            var collectionNames = db.ListCollectionNames().ToList();
            if (!collectionNames.Contains("counters"))
            {
                db.CreateCollection("counters");
                var counters = db.GetCollection<BsonDocument>("counters");
                counters.InsertOne(new BsonDocument { { "_id", "init" }, { "seq", 0 } });
            }

            return true;
        }
    }
}
