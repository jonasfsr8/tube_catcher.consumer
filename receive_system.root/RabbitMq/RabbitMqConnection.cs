using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using receive_system.root.DTOs;
using receive_system.root.Interfaces;

namespace receive_system.root.RabbitMq
{
    public class RabbitMqConnection : IRabbitMqConnection, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMqConnection> _logger;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private IConnection? _connection;

        public RabbitMqConnection(
            IOptions<RabbitMqSettingsDto> config,
            ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;

            var settings = config.Value;

            _factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                Port = settings.Port,

                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection is not null && _connection.IsOpen)
                return _connection;

            await _lock.WaitAsync();

            try
            {
                if (_connection is not null && _connection.IsOpen)
                    return _connection;

                _logger.LogInformation("[RabbitMQ] connecting...");

                _connection = await _factory.CreateConnectionAsync();

                _logger.LogInformation("[RabbitMQ] connected");

                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] error connecting");
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IChannel> CreateChannel()
        {
            var connection = await GetConnectionAsync();

            return await connection.CreateChannelAsync();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_connection is not null)
                {
                    if (_connection.IsOpen)
                        await _connection.CloseAsync();

                    _connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] error disposing connection");
            }
        }
    }
}
