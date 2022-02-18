[NetDaemonApp]
public class OpenWindowClimateOff
{
    public OpenWindowClimateOff(Services services, ClimateZones climateZones)
    {
        foreach (var zone in climateZones.Zones.Where(z => z.Window != null))
        {
            zone.Window!.StateChanges().Subscribe(_ => SyncZone(zone));
            zone.Climate.StateAllChanges().Subscribe(_ => SyncZone(zone));
            SyncZone(zone);
        }

        void SyncZone(ClimateZone zone)
        {
            if (zone.Window.IsOn() && zone.Climate.IsOn())
            {
                zone.Climate.SetHvacMode(hvacMode: "off");

                services.Logbook.Log(
                    entityId: zone.Climate.EntityId,
                    message: "Uitgeschakeld omdat het raam open is",
                    name: "radiator",
                    domain: "climate");
            }
        }
    }
}