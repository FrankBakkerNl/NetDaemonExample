[NetDaemonApp]
public class OpenWindowClimateOff
{
    private readonly Services _services;
    private readonly IScheduler _scheduler;
    private readonly Entities _entities;

    public OpenWindowClimateOff(Services services, ClimateZones climateZones, IScheduler scheduler, Entities entities)
    {
        _services = services;
        _scheduler = scheduler;
        _entities = entities;
        foreach (var zone in climateZones.Zones.Where(z => z.Window != null))
        {
            zone.Window!.WhenTurnsOn(_ => WindowOpened(zone));
            zone.Climate.StateAllChanges().Subscribe(_ => CheckZone(zone));
            CheckZone(zone);
        }
    }

    void WindowOpened(ClimateZone zone)
    {
        if (zone.Climate.State != "off")
        {
            // If the window is closed again withing 2 hours, restore the state of the climate to what it was
            var previousState = zone.Climate.EntityState;
            zone.Window?.StateChanges()
                .Where(s => s.New.IsOff())
                .Take(1)
                .Timeout(TimeSpan.FromHours(2), _scheduler)
                .Subscribe(_ => RestorePreviousClimateState(zone, previousState), onError: _ => { });

            // Now actually turn off the climate
            zone.Climate.SetHvacMode(hvacMode: "off");
            LogBookClimate(zone.Climate, "Uitgeschakeld omdat het raam werd geopend");

            if (zone.Climate.EntityId == _entities.Climate.WoonkamerThermostaat.EntityId)
            {
                _services.Esphome.EspKitchenPanelNotificationShow("Raam open", "Verwarming is uitgeschakeld");
            }
        }
    }
    
    private void RestorePreviousClimateState(ClimateZone zone, EntityState<ClimateAttributes>? previousState)
    {
        if (previousState?.State == "heat" && (previousState.Attributes?.Temperature.HasValue ?? false))
        {
            zone.Climate.SetHvacMode(hvacMode: "heat");
            zone.Climate.SetTemperature(temperature: previousState?.Attributes?.Temperature, hvacMode: "heat");
            LogBookClimate(zone.Climate, "Weer ingeschakeld omdat het raam werd gesloten");
        }
    }
    
    private void CheckZone(ClimateZone zone)
    {
        if (zone.Window.IsOn() && zone.Climate.State != "off")
        {
            // Now actually turn off the climate
            zone.Climate.SetHvacMode(hvacMode: "off");
            LogBookClimate(zone.Climate, "Uitgeschakeld omdat het raam open is");
        }
    }

    private void LogBookClimate(ClimateEntity climateEntity, string message)
    {
        _services.Logbook.Log(
            entityId: climateEntity.EntityId,
            message: message,
            name: "radiator",
            domain: "climate");
    }
}