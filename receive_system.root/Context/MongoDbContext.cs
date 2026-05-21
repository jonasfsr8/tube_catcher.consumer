using Microsoft.Extensions.Options;
using MongoDB.Driver;
using receive_system.root.DTOs;

namespace receive_system.root.Context
{
    public class MongoContext
    {
        private readonly IMongoClient _client;

        public MongoContext(IMongoClient client, IOptions<MongoDbSettingsDto> config)
        {
            _client = client;
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName, string databaseName)
        {
            var database = _client.GetDatabase(databaseName);
            return database.GetCollection<T>(collectionName);
        }
    }
}
