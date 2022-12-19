[NetDaemonApp]
public class LaundyMonitor
{
    private readonly Entities _entities;
    private readonly NotifyServices _notify;

    public LaundyMonitor(Entities entities, NotifyServices notifyServices, ILogger<LaundyMonitor> logger)
    {
        _entities = entities;
        _notify = notifyServices;
            
        _entities.Sensor.WasherState.StateChanges().Where(e => e.Old?.State == "Running").SubscribeSafe(_ => WasherReady(), logger);
        _entities.Sensor.WasherState.StateChanges().Where(e => e.New?.State == "Running").SubscribeSafe(_ => WasherReset(), logger);

        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Running" && e.New?.State == "Ready").SubscribeSafe(_ => DryerReady(), logger);
        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Ready").SubscribeSafe(_ => DryerReset(), logger);
    }

    private void WasherReady() => _notify.MobileAppPhoneFrank(
            message: $"⌛ {TimeSpan.FromSeconds(_entities.Sensor.WasherProgramTime.State ?? 0.0):hh\\:mm}" +
                     $"⚡ {_entities.Sensor.WasherProgramEnergy.State:N0} Wh " +
                     $"💶 € {_entities.Sensor.WasherProgramEnergy.State * 0.22 / 1000:N2}",
            title: "🧺 Washer finished",
            data: new { tag = "WasherNotification" }
        );

    private void WasherReset() => _notify.MobileAppPhoneFrank(
            message: "clear_notification",
            data: new { tag = "WasherNotification" });
    

    private void DryerReady() => _notify.MobileAppPhoneFrank(
            message: $"⌛ {TimeSpan.FromSeconds(_entities.Sensor.DryerProgramTime.State ?? 0.0):hh\\:mm}" +
                     $"⚡ {_entities.Sensor.DryerProgramEnergy.State:N0}" +
                     $"💶 € {(_entities.Sensor.DryerProgramEnergy.State ?? 0) * 0.22 / 1000:N2}",
            title: "🧺 Dryer finished",
            data: new { tag = "DryerNotification" });

    private void DryerReset() => _notify.MobileAppPhoneFrank(
            message : "clear_notification",
            data : new { tag = "DryerNotification" });
}