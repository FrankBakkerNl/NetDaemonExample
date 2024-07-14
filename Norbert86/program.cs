using System.Reflection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;


await Host.CreateDefaultBuilder(args)
    .UseNetDaemonAppSettings()
    .UseCustomLogging()
    .UseNetDaemonRuntime()
    .ConfigureServices((_, services) =>
        services
            .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
            .AddNetDaemonStateManager()
            .AddNetDaemonScheduler()
            .AddHomeAssistantGenerated()
            .AddNorbert86Services()
    )
    .Build()
    .RunAsync()
    .ConfigureAwait(false);