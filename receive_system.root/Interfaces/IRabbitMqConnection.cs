using RabbitMQ.Client;

namespace receive_system.root.Interfaces
{
    public interface IRabbitMqConnection
    {
        Task<IChannel> CreateChannel();
    }
}
