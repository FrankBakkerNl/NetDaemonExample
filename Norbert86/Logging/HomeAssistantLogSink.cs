using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using Serilog.Core;
using Serilog.Events;

class HomeAssistantLogSink : ILogEventSink
{
    private readonly IServiceProvider _provider;

    public HomeAssistantLogSink(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public void Emit(LogEvent logEvent)
    {
        var connection = _provider.GetService<IHomeAssistantConnection>();
        var level = MapLogLevel(logEvent);
        var logger  = "NetDaemon." + logEvent.Properties["SourceContext"]?.ToString().Replace("\"", "");
        connection?.CallServiceAsync("system_log", "write", new { message = logEvent.RenderMessage(), level, logger});
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