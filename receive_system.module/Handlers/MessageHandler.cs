using receive_system.core.Entities;
using receive_system.core.Interfaces.Messages;
using receive_system.module.Interfaces;

namespace receive_system.module.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IYoutubeService _youtubeService;

        public MessageHandler(IYoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        public async Task HandleAsync(Envelope message)
        {
            try
            {
                if (message is null || string.IsNullOrWhiteSpace(message.Payload))
                    throw new Exception("Empty payload.");

                if (!message.Payload.Contains("youtube.com"))
                    throw new Exception("Invalid URL");

                var result = await _youtubeService.DownloadAsync(message.Payload, message.Type);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
