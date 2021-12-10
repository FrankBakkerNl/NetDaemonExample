namespace HomeAssistantGenerated.apps.Ping;

[NetDaemonApp]
public class PingND
{
    public PingND(IHaContext ha, INetDaemonScheduler scheduler)
    {
            
        var inputBooleanPingpong = ha.MyEntities().InputBoolean.Netdaemonpingpong;
            
        inputBooleanPingpong.TurnOff();
        ha.Services().Logbook.Log("Ping", "NetDaemon started", inputBooleanPingpong.EntityId);

        inputBooleanPingpong.WhenTurnsOn(_ =>
        {
            ha.Services().Logbook.Log("Ping", "NetDaemon was pinged", inputBooleanPingpong.EntityId);
            scheduler.RunIn(TimeSpan.FromSeconds(1), () => inputBooleanPingpong.TurnOff());
        });
    }
}