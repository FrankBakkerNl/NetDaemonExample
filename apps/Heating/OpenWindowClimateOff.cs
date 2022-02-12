[NetDaemonApp]
public class OpenWindowClimateOff
{
    private readonly Services _services;
    public OpenWindowClimateOff(IHaContext ha)
    {
        _services = ha.Services();
        
        foreach (var zone in new ClimateZones(ha).Zones.Where(z => z.Window != null))
        {
            zone.Window!.StateChanges().Subscribe(_ => SyncZone(zone));
            zone.Climate.StateAllChanges().Subscribe(_ => SyncZone(zone));
            SyncZone(zone);
        }
    }

    private void SyncZone(ClimateZone zone)
    {
        if (!zone.Window.IsOn()) return;

        if (zone.Climate.State != "off")
        {
            zone.Climate.SetHvacMode(hvacMode:"off");
            _services.Logbook.Log(
                entityId:  zone.Climate.EntityId,
                message : "Uitgeschakeld omdat het raam open is",
                name : "radiator",
                domain : "climate");
        }
    }
}