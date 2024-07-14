[NetDaemonApp]
public class LaundyMonitor
{
    private readonly Entities _entities;
    private readonly NotifyServices _notify;
    private readonly EsphomeServices _esphomeServices;

    public LaundyMonitor(Entities entities, NotifyServices notifyServices, ILogger<LaundyMonitor> logger, EsphomeServices esphomeServices)
    {
        _entities = entities;
        _notify = notifyServices;
        _esphomeServices = esphomeServices;

        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Running" && e.New?.State == "Ready").SubscribeSafe(_ => DryerReady(), logger);
        _entities.Sensor.DryerState.StateChanges().Where(e => e.Old?.State == "Ready").SubscribeSafe(_ => DryerReset(), logger);
    }

    private void DryerReady()
    {
        _esphomeServices.EspKitchenPanelNotificationShow("Droger", "De droger is klaar");
        _esphomeServices.EspKitchenPanelPlayRtttl("smb2:d=4,o=5,b=130:8p,8p,8g5,8a5,8f6,16g6,16p,16e6,8c6,16d6,8b5");
        
        _notify.MobileAppPhoneFrank(
            message: $"⌛ {TimeSpan.FromSeconds(_entities.Sensor.DryerProgramTime.State ?? 0.0):hh\\:mm}" +
                     $"⚡ {_entities.Sensor.DryerProgramEnergy.State:N0}" +
                     $"💶 € {(_entities.Sensor.DryerProgramEnergy.State ?? 0) * _entities.InputNumber.EnergyTarif.State / 1000:N2}",
            title: "🧺 Dryer finished",
            data: new { tag = "DryerNotification" });
    }

    private void DryerReset()
    {
        _esphomeServices.EspKitchenPanelNotificationClear();

        _notify.MobileAppPhoneFrank(
            message: "clear_notification",
            data: new { tag = "DryerNotification" });
    }
}