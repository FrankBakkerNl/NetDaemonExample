using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using Serilog.Core;
using Serilog.Events;

class NotifyExceptionsSink(IServiceProvider provider) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var connection = provider.GetService<IHomeAssistantConnection>();
        var message = logEvent.RenderMessage();
        // prevent recursive logging
        if (message.Contains("Exception in NetDaemon")) return;
        connection?.CallServiceAsync("notify", "mobile_app_phone_frank", new { message = logEvent.RenderMessage(), title = "👿 Exception in NetDaemon 👿" });
    }
}