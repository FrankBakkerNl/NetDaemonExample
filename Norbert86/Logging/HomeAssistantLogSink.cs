using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using Serilog.Core;
using Serilog.Events;

class HomeAssistantLogSink(IServiceProvider provider) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var connection = provider.GetService<IHomeAssistantConnection>();
        var level = MapLogLevel(logEvent);
        var logger =logEvent.Properties["SourceContext"]?.ToString().Replace("\"", "");
        if (!logger?.StartsWith("NetDaemon.") ?? false) logger = "NetDaemon." + logger;
        
        var message = logEvent.RenderMessage();

        // prevent recursive logging
        if (message.Contains("Exception in NetDaemon")) return;
        connection?.CallServiceAsync("system_log", "write", new { message = "Exception in NetDaemon: " + message, level, logger});
    }
    
    private static string MapLogLevel(LogEvent logEvent) =>
        logEvent.Level switch
        {
            LogEventLevel.Fatal => "critical",
            LogEventLevel.Error => "error",
            LogEventLevel.Warning => "warning",
            LogEventLevel.Information => "info",
            LogEventLevel.Debug => "debug",
            _ => "info",
        };
}