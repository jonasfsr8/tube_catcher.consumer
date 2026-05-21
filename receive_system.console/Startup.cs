using Microsoft.Extensions.Hosting;
using receive_system.module;
using receive_system.root;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.ConfigureApplicationApp();
        services.ConfigurePersistenceApp(configuration);

    })
    .Build();

await host.RunAsync();