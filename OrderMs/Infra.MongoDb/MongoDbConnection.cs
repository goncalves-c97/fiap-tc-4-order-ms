using Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Infra.Data.MongoDb
{
    public class MongoDbConnection : IDbConnection
    {
        private readonly IMongoDatabase _database;

        public MongoDbConnection(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        private IMongoCollection<BsonDocument> GetCollection(string name)
        {
            return _database.GetCollection<BsonDocument>(name);
        }

        private static string ToSnakeCase(string str)
        {
            return string.Concat(str.Select((ch, i) =>
            i > 0 && char.IsUpper(ch) ? "_" + char.ToLower(ch) : char.ToLower(ch).ToString()));
        }

        private async Task<int> GetNextSequenceAsync(string sequenceName)
        {
            var counters = _database.GetCollection<BsonDocument>("counters");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = await counters.FindOneAndUpdateAsync(filter, update, options);
            return result.Contains("seq") ? result["seq"].AsInt32 : 0;
        }

        // Helper to build a Mongo filter from a simple SQL-like where clause and parameters
        private FilterDefinition<BsonDocument> BuildFilter(string whereClause, object whereParams)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                return Builders<BsonDocument>.Filter.Empty;

            var filters = new List<FilterDefinition<BsonDocument>>();

            // Split by AND (case insensitive)
            var parts = Regex.Split(whereClause, "\bAND\b", RegexOptions.IgnoreCase)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p));

            var paramDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (whereParams != null)
            {
                foreach (var prop in whereParams.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    paramDict[prop.Name] = prop.GetValue(whereParams);
                }
            }

            foreach (var part in parts)
            {
                // Expect patterns like: field = @Param
                var m = Regex.Match(part, "^(?<field>\\w+)\\s*(?<op>=|!=|>|<|>=|<=)\\s*@(?<param>\\w+)$");
                if (!m.Success)
                {
                    // if not matched, skip this condition
                    continue;
                }

                var field = m.Groups["field"].Value;
                var op = m.Groups["op"].Value;
                var param = m.Groups["param"].Value;

                if (!paramDict.TryGetValue(param, out var value))
                {
                    // try case-insensitive
                    var key = paramDict.Keys.FirstOrDefault(k => string.Equals(k, param, StringComparison.OrdinalIgnoreCase));
                    if (key != null) value = paramDict[key];
                }

                var elementName = field; // assume caller uses correct DB field names (snake_case)

                if (value == null)
                {
                    if (op == "=")
                        filters.Add(Builders<BsonDocument>.Filter.Eq(elementName, BsonNull.Value));
                    else if (op == "!=")
                        filters.Add(Builders<BsonDocument>.Filter.Ne(elementName, BsonNull.Value));
                    continue;
                }

                switch (op)
                {
                    case "=":
                        filters.Add(Builders<BsonDocument>.Filter.Eq(elementName, BsonValue.Create(value)));
                        break;
                    case "!=":
                        filters.Add(Builders<BsonDocument>.Filter.Ne(elementName, BsonValue.Create(value)));
                        break;
                    case ">":
                        filters.Add(Builders<BsonDocument>.Filter.Gt(elementName, BsonValue.Create(value)));
                        break;
                    case "<":
                        filters.Add(Builders<BsonDocument>.Filter.Lt(elementName, BsonValue.Create(value)));
                        break;
                    case ">=":
                        filters.Add(Builders<BsonDocument>.Filter.Gte(elementName, BsonValue.Create(value)));
                        break;
                    case "<=":
                        filters.Add(Builders<BsonDocument>.Filter.Lte(elementName, BsonValue.Create(value)));
                        break;
                }
            }

            if (filters.Count == 0)
                return Builders<BsonDocument>.Filter.Empty;

            return Builders<BsonDocument>.Filter.And(filters);
        }

        private T ConvertBsonTo<T>(BsonDocument doc) where T : class, new()
        {
            var instance = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var snake = ToSnakeCase(prop.Name);
                if (!doc.Contains(snake))
                    continue;

                var bsonVal = doc[snake];
                if (bsonVal == BsonNull.Value)
                {
                    prop.SetValue(instance, null);
                    continue;
                }

                try
                {
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object? value = targetType switch
                    {
                        Type t when t == typeof(int) => bsonVal.AsInt32,
                        Type t when t == typeof(long) => bsonVal.AsInt64,
                        Type t when t == typeof(string) => bsonVal.ToString(),
                        Type t when t == typeof(DateTime) => bsonVal.ToUniversalTime(),
                        Type t when t.IsEnum => Enum.ToObject(t, bsonVal.AsInt32),
                        _ => BsonTypeMapper.MapToDotNetValue(bsonVal)
                    };

                    prop.SetValue(instance, value);
                }
                catch
                {
                    // ignore conversion errors for unknown types
                }
            }

            return instance;
        }

        public async Task<int> InsertAsync(string table, Dictionary<string, object> values)
        {
            var collection = GetCollection(table);
            var doc = new BsonDocument();
            foreach (var kv in values)
                doc[kv.Key] = BsonValue.Create(kv.Value ?? BsonNull.Value);

            await collection.InsertOneAsync(doc);
            return 1; // return number of inserted documents
        }

        public async Task<int> InsertAndReturnIdAsync(string tableName, Dictionary<string, object> values, string idColumn = "id")
        {
            var seqName = $"{tableName}_{idColumn}";
            int id = await GetNextSequenceAsync(seqName);
            values[idColumn] = id;

            var collection = GetCollection(tableName);
            var doc = new BsonDocument();
            foreach (var kv in values)
                doc[kv.Key] = BsonValue.Create(kv.Value ?? BsonNull.Value);

            await collection.InsertOneAsync(doc);
            return id;
        }

        public async Task<int> UpdateAsync(string table, Dictionary<string, object> values, string whereClause, object whereParams = null)
        {
            var collection = GetCollection(table);
            var filter = BuildFilter(whereClause, whereParams);

            var updateDef = new List<UpdateDefinition<BsonDocument>>();
            foreach (var kv in values)
            {
                updateDef.Add(Builders<BsonDocument>.Update.Set(kv.Key, BsonValue.Create(kv.Value ?? BsonNull.Value)));
            }

            var update = Builders<BsonDocument>.Update.Combine(updateDef);

            var result = await collection.UpdateManyAsync(filter, update);
            return (int)result.ModifiedCount;
        }

        public async Task<int> DeleteAsync(string table, string whereClause, object whereParams = null)
        {
            var collection = GetCollection(table);
            var filter = BuildFilter(whereClause, whereParams);

            var result = await collection.DeleteManyAsync(filter);
            return (int)result.DeletedCount;
        }

        public async Task<T?> SearchFirstOrDefaultByParametersAsync<T>(string table, string whereClause, object whereParams = null)
        where T : class, new()
        {
            var collection = GetCollection(table);
            var filter = BuildFilter(whereClause, whereParams);

            var doc = await collection.Find(filter).FirstOrDefaultAsync();
            if (doc == null) return null;

            return ConvertBsonTo<T>(doc);
        }

        public async Task<IEnumerable<T>> SearchByParametersAsync<T>(string table, string whereClause, object whereParams = null)
        where T : class, new()
        {
            var collection = GetCollection(table);
            var filter = BuildFilter(whereClause, whereParams);

            var docs = await collection.Find(filter).ToListAsync();
            return docs.Select(d => ConvertBsonTo<T>(d));
        }

        public async Task<IEnumerable<T>> ListAllAsync<T>(string table, string[] columns = null)
        where T : class, new()
        {
            var collection = GetCollection(table);
            var docs = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
            return docs.Select(d => ConvertBsonTo<T>(d));
        }

        public Task<int> ExecuteRawSql(string rawSql)
        {
            throw new NotSupportedException("ExecuteRawSql is not supported for MongoDB provider.");
        }

        public Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, T4, T5, TReturn>(string sql, Func<T1, T2, T3, T4, T5, TReturn> map, object? param, string splitOn)
        {
            throw new NotSupportedException("Multi-map QueryAsync is not supported for MongoDB provider.");
        }
    }
}
