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
            logConfig.WriteTo.Sink(new NotifyExceptionsSink(provider), LogEventLevel.Error);
            logConfig.WriteTo.Sink(new HomeAssistantLogSink(provider), LogEventLevel.Information);
        });
    }
}