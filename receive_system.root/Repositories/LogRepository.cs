using MongoDB.Driver;
using receive_system.core.Entities;
using receive_system.core.Interfaces.Repositories;
using receive_system.root.Context;

namespace receive_system.root.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly MongoContext _context;

        public LogRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task InsertLogAsync<T>(T log, string collectionName, string databaseName)
        {
            var collection = _context.GetCollection<T>(collectionName, databaseName);
            await collection.InsertOneAsync(log);
        }

        public async Task UpdateLogAsync(Envelope log, string collectionName, string databaseName)
        {
            var collection = _context.GetCollection<Envelope>(collectionName, databaseName);

            var filter = Builders<Envelope>.Filter.Eq(x => x.CorrelationId, log.CorrelationId);

            var update = Builders<Envelope>.Update
                .Set(x => x.RetryCount, log.RetryCount)
                .Set(x => x.Payload, log.Payload)
                .Set(x => x.Type, log.Type)
                .Set(x => x.Source, log.Source)
                .Set(x => x.DateCreated, log.DateCreated);

            await collection.UpdateOneAsync(filter, update);
        }
    }
}