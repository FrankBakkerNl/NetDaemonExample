[NetDaemonApp]
public class SyncCVToClimates
{
    private readonly SwitchEntity _heaterSwitch;
    private readonly IEnumerable<ClimateEntity> _climates;

    public SyncCVToClimates(ClimateZones climateZones, Entities entities)
    {
        _heaterSwitch = entities.Switch.CvUpstairsRelay;
        _climates = climateZones.Zones.Select(z => z.Climate).ToArray(); 
        
        _heaterSwitch.StateChanges().Subscribe(_ => SetCvState());
        _climates.StateAllChanges().Subscribe(_ => SetCvState());
        SetCvState();
    }

    private void SetCvState()
    {
        var anyHeating = _climates.Any(NeedsHeat);
        _heaterSwitch.SwitchTo(anyHeating ? "on" : "off");
    }
        
    private bool NeedsHeat(ClimateEntity climate) => 
        climate.EntityState?.Attributes?.HvacAction == "heating" && 
        climate.EntityState?.Attributes?.Temperature > climate.EntityState?.Attributes?.CurrentTemperature;
}