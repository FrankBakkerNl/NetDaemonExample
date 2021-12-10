namespace Laundry;

[NetDaemonApp]
public class LaundyMonitor
{
    private readonly Entities _entities;
    private NotifyServices Notify { get; }

    public LaundyMonitor(IHaContext ha)
    {
        _entities = ha.MyEntities();
        Notify = ha.Services().Notify;
            
        _entities.Sensor.WasherState.StateChanges().Where(e => e.Old?.State == "Running").Subscribe(_ => WasherReady());
        _entities.Sensor.WasherState.StateChanges().Where(e => e.New?.State == "Running").Subscribe(_ => WasherReset());

        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Running" && e.New?.State == "Ready").Subscribe(_ => DryerReady());
        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Ready").Subscribe(_ => DryerReset());
    }

    private void WasherReady()
    {
        Notify.MobileAppPhoneFrank(
            message: $"⌛ {TimeSpan.FromSeconds((long?)_entities.Sensor.WasherProgramTime.AsNumeric().State ?? 0)}" +
                     $"⚡ {_entities.Sensor.WasherProgramEnergy.AsNumeric().State:N0} Wh " +
                     $"💶 € {(_entities.Sensor.WasherProgramEnergy.AsNumeric().State) * 0.22 / 1000:N2}",
            title: "🧺 Washer finished",
            data: new { tag = "WasherNotification" }
        );
    }

    private void WasherReset()
    {
        Notify.MobileAppPhoneFrank(message: "clear_notification",
            data: new { tag = "WasherNotification" });
    }

    private void DryerReady()
    {
        Notify.MobileAppPhoneFrank(
            message: $"⌛ {TimeSpan.FromSeconds((long?)_entities.Sensor.DryerProgramTime.AsNumeric().State ?? 0)}" +
                     $"⚡ {_entities.Sensor.DryerProgramEnergy.AsNumeric().State:N0}" +
                     $"💶 € {(_entities.Sensor.DryerProgramEnergy.AsNumeric().State ?? 0) * 0.22 / 1000:N2}",
            title: "🧺 Dryer finished",
            data: new { tag = "DryerNotification"});
    }

    private void DryerReset()
    {
        Notify.MobileAppPhoneFrank(
            message : "clear_notification",
            data : new { tag = "DryerNotification" });
    }
}