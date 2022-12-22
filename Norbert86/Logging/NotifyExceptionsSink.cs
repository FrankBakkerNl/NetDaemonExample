using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using Serilog.Core;
using Serilog.Events;

class NotifyExceptionsSink : ILogEventSink
{
    private readonly IServiceProvider _provider;

    public NotifyExceptionsSink(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public void Emit(LogEvent logEvent)
    {
        var connection = _provider.GetService<IHomeAssistantConnection>();
        connection?.CallServiceAsync("notify", "mobile_app_phone_frank", new { message = logEvent.RenderMessage(), title = "👿 Exception in NetDaemon 👿" });
    }
}