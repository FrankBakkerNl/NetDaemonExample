using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

public static class CustomLoggingProvider
{
    public static IHostBuilder UseCustomLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, provider, logConfig) =>
        {
            logConfig.ReadFrom.Configuration(context.Configuration);
            if (provider.GetRequiredService<IHostEnvironment>().IsDevelopment()) return;
            
            logConfig.WriteTo.Sink(new NotifyExceptionsSink(provider), LogEventLevel.Warning);
            logConfig.WriteTo.Sink(new HomeAssistantLogSink(provider), LogEventLevel.Information);
        });
    }
}