using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using receive_system.core.Interfaces.Repositories;
using receive_system.module.Interfaces;
using receive_system.root.Context;
using receive_system.root.DTOs;
using receive_system.root.ExternalServices;
using receive_system.root.Interfaces;
using receive_system.root.RabbitMq;
using receive_system.root.Repositories;

namespace receive_system.root
{
    public static class ServiceInfrastructureExtensions
    {
        public static IServiceCollection ConfigurePersistenceApp(this IServiceCollection services, IConfiguration configuration)
        {
            // MongoDb
            services.Configure<MongoDbSettingsDto>(configuration.GetSection("MongoDb"));
            services.AddSingleton<IMongoClient>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<MongoDbSettingsDto>>().Value;
                return new MongoClient(config.ConnectionString);
            });
            services.AddScoped<MongoContext>();

            // RabbitMq
            services.Configure<RabbitMqSettingsDto>(configuration.GetSection("RabbitMqConfig"));
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddHostedService<RabbitMqConsumer>();

            // Repositories
            services.AddScoped<ILogRepository, LogRepository>();

            // Servcies
            services.Configure<YtDlpSettingsDto>(configuration.GetSection("YtDlp"));
            services.AddSingleton<IYoutubeService, YoutubeService>();

            return services;
        }
    }
}
