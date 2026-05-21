using receive_system.core.Entities;

namespace receive_system.core.Interfaces.Messages
{
    public interface IMessageHandler
    {
        Task HandleAsync(Envelope message);
    }
}
