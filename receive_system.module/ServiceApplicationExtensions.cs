using Microsoft.Extensions.DependencyInjection;
using receive_system.core.Interfaces.Messages;
using receive_system.module.Handlers;

namespace receive_system.module
{
    public static class ServiceApplicationExtensions
    {
        public static void ConfigureApplicationApp(this IServiceCollection services)
        {
            services.AddScoped<IMessageHandler, MessageHandler>();
        }
    }
}
