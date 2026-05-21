using receive_system.core.Entities;

namespace receive_system.core.Interfaces.Repositories
{
    public interface ILogRepository
    {
        Task InsertLogAsync<T>(T log, string collectionName, string databaseName);
        Task UpdateLogAsync(Envelope log, string collectionName, string databaseName);
    }
}
