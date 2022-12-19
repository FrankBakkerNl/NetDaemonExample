[NetDaemonApp]
public class PingND : IDisposable
{
    private readonly Services _services;
    private readonly InputBooleanEntity _inputBooleanPingpong;

    public PingND(Services services, Entities entities, IScheduler scheduler)
    {
        _services = services;
        _inputBooleanPingpong = entities.InputBoolean.Netdaemonpingpong;
            
        _inputBooleanPingpong.TurnOn();
        _services.Logbook.Log("Ping", "NetDaemon started", _inputBooleanPingpong.EntityId);

        _services.Notify.MobileAppPhoneFrank(
            title: $"NetDaemon restarted: {DateTime.Now}",
            message: $"{DateTime.Now}");

        _inputBooleanPingpong.WhenTurnsOff(_ =>
        {
            _services.Logbook.Log("Ping", "NetDaemon was pinged", _inputBooleanPingpong.EntityId);
            scheduler.Schedule(TimeSpan.FromSeconds(0.5), () => _inputBooleanPingpong.TurnOn());
        });
    }

    public void Dispose()
    {
        _inputBooleanPingpong.TurnOff();
        _services.Logbook.Log("Ping", "NetDaemon stopped", _inputBooleanPingpong.EntityId);

        
        _services.Notify.MobileAppPhoneFrank(
            title: $"NetDaemon stopped: {DateTime.Now}",
            message: $"{DateTime.Now}");
    }
}