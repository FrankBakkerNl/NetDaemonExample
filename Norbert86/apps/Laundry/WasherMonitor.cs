namespace HomeAssistantGenerated.apps.Laundry;

[NetDaemonApp]
public class WasherMonitor
{
    public WasherMonitor(Entities entities, NotifyServices notifyServices, ILogger<LaundyMonitor> logger,
        EsphomeServices esphomeServices, IScheduler scheduler)
    {
        var powerSeries = entities.Sensor.WasherPower2.StateChanges().Select(e => e.New?.State ?? 0);

        var states = CycleDetector.WasherStates(powerSeries, scheduler);

        states.Subscribe(s => entities.InputSelect.WasherState.SelectOption(s.ToString()));
        states.PairWithPrevious().Where(e => e.Previous == CycleState.Running).Subscribe(_ => WasherReady());
        states.Where(e => e == CycleState.Running).Subscribe(_ => WasherReset());

        void WasherReady()
        {
            esphomeServices.EspKitchenPanelNotificationShow("Wasmashine", "De wasmachine is klaar");
            esphomeServices.EspKitchenPanelPlayRtttl("smb:d=4,o=5,b=100:16e6,16e6,32p,8e6,16c6,8e6,8g6,8p");

            notifyServices.MobileAppPhoneFrank(
                message: $"⌛ {TimeSpan.FromSeconds(entities.Sensor.WasherProgramTime.State ?? 0.0):hh\\:mm}" +
                         $"⚡ {entities.Sensor.WasherProgramEnergy.State:N0} Wh " +
                         $"💶 € {entities.Sensor.WasherProgramEnergy.State * entities.InputNumber.EnergyTarif.State / 1000:N2}",
                title: "🧺 Washer finished",
                data: new { tag = "WasherNotification" }
            );
        }

        void WasherReset()
        {
            esphomeServices.EspKitchenPanelNotificationClear();

            notifyServices.MobileAppPhoneFrank(
                message: "clear_notification",
                data: new { tag = "WasherNotification" });
        }
    }
}