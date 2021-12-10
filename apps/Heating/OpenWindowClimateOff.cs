namespace Heating;

[NetDaemonApp]
public class OpenWindowClimateOff : IInitializable
{
    public IEnumerable<ClimateZone> ClimateZones { get; init; } = Enumerable.Empty<ClimateZone>();

    private readonly Services _services;
    public OpenWindowClimateOff(IHaContext ha)
    {
        _services = ha.Services();
    }

    public void Initialize()
    {
        foreach (var zone in ClimateZones)
        {
            zone.Windows.StateChanges().Subscribe(_ => SyncZone(zone));
            zone.Climates.StateAllChanges().Subscribe(_ => SyncZone(zone));
            SyncZone(zone);
        }
    }

    private void SyncZone(ClimateZone zone)
    {
        if (!zone.Windows.Any(w => w.IsOn())) return;

        foreach (var climate in zone.Climates)
        {
            if (climate.State != "off")
            {
                climate.SetHvacMode(hvacMode:"off");
                _services.Logbook.Log(
                    entityId:  climate.EntityId,
                    message : "Uitgeschakeld omdat het raam open is",
                    name : "radiator",
                    domain : "climate");
            }
        }
    }
}

public class ClimateZone
{
    public string? Name { get; init; }
    public IEnumerable<ClimateEntity> Climates { get; init; } = Enumerable.Empty<ClimateEntity>();
    public IEnumerable<BinarySensorEntity> Windows { get; init; } = Enumerable.Empty<BinarySensorEntity>();
}