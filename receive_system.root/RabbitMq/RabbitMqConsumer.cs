using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using receive_system.core.Entities;
using receive_system.core.Interfaces.Messages;
using receive_system.core.Interfaces.Repositories;
using receive_system.root.DTOs;
using receive_system.root.Interfaces;
using System.Text;

namespace receive_system.root.RabbitMq
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettingsDto _config;
        private readonly ILogger<RabbitMqConsumer> _logger;

        public RabbitMqConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqSettingsDto> config,
            ILogger<RabbitMqConsumer> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _config = config.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RabbitMQ] starting consumer...");

            await using var consumerChannel = await _connection.CreateChannel();

            await using var publishChannel = await _connection.CreateChannel();

            var mainQueue = _config.QueueName;
            var retryQueue = $"{mainQueue}-retry";
            var deadQueue = $"{mainQueue}-dead";

            // Controle de carga
            await consumerChannel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false);

            // Dead queue
            await consumerChannel.QueueDeclareAsync(
                queue: deadQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Retry queue
            await consumerChannel.QueueDeclareAsync(
                queue: retryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-message-ttl", 5000 },
                    { "x-dead-letter-exchange", "" },
                    { "x-dead-letter-routing-key", mainQueue }
                });

            // Main queue
            await consumerChannel.QueueDeclareAsync(
                queue: mainQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(consumerChannel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                Envelope? message = null;

                try
                {
                    message = JsonConvert.DeserializeObject<Envelope>(body);

                    if (message is null)
                    {
                        _logger.LogWarning("[RabbitMQ] invalid message");

                        await consumerChannel.BasicAckAsync(
                            ea.DeliveryTag,
                            false);

                        return;
                    }

                    if (message.Type == "best")
                        message.Type = "mp3";

                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();

                    var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

                    if (message.RetryCount == 0)
                    {
                        await logRepository.InsertLogAsync(message, "youtube_tasks", "messages");
                    }

                    await handler.HandleAsync(message);

                    _logger.LogInformation("[RabbitMQ] message processed successfully");

                    await consumerChannel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RabbitMQ] error processing message");

                    if (message is not null)
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();

                            var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

                            const int maxRetry = 3;

                            if (message.RetryCount < maxRetry)
                            {
                                message.RetryCount++;

                                await logRepository.UpdateLogAsync(message, "youtube_tasks", "messages");

                                var retryBody = Encoding.UTF8.GetBytes(
                                    JsonConvert.SerializeObject(message));

                                var properties = new BasicProperties
                                {
                                        Persistent = true
                                };

                                await publishChannel.BasicPublishAsync(
                                    exchange: "",
                                    routingKey: retryQueue,
                                    mandatory: true,
                                    basicProperties: properties,
                                    body: retryBody);

                                _logger.LogWarning("[RabbitMQ] message sent to retry queue");
                            }
                            else
                            {
                                var deadBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                                var properties = new BasicProperties
                                {
                                        Persistent = true
                                };

                                await publishChannel.BasicPublishAsync(
                                    exchange: "",
                                    routingKey: deadQueue,
                                    mandatory: true,
                                    basicProperties: properties,
                                    body: deadBody);

                                _logger.LogError("[RabbitMQ] message sent to dead queue");
                            }
                        }
                        catch (Exception retryEx)
                        {
                            _logger.LogCritical(retryEx, "[RabbitMQ] failed to publish retry/dead-letter");
                        }
                    }

                    await consumerChannel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };

            await consumerChannel.BasicConsumeAsync(
                queue: mainQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("[RabbitMQ] consumer started successfully");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("[RabbitMQ] consumer stopped");
            }
        }
    }
}
