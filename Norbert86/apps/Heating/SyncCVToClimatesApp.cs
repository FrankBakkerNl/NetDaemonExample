[NetDaemonApp]
public class SyncCVToClimates
{
    private readonly Services _services;
    private readonly SwitchEntity _heaterSwitch;
    private readonly IEnumerable<ClimateEntity> _climates;

    public SyncCVToClimates(ClimateZones climateZones, Entities entities, Services services)
    {
        _services = services;
        _heaterSwitch = entities.Switch.CvUpstairsRelay;
        _climates = climateZones.Zones.Select(z => z.Climate).ToArray(); 
        
        _heaterSwitch.StateChanges().Subscribe(_ => SetCvState());
        _climates.StateAllChanges().Subscribe(_ => SetCvState());
        SetCvState();
    }

    private void SetCvState()
    {
        var heatingClimates = _climates.Where(NeedsHeat).ToList();

        if (!heatingClimates.Any())
        {
            if ( _heaterSwitch.IsOn()) _heaterSwitch.TurnOff();
        }
        else
        {
            if (_heaterSwitch.IsOff())
            {
                _heaterSwitch.TurnOn();
                
                var climateNames = string.Join(Environment.NewLine, heatingClimates.Select(c => c.Attributes?.FriendlyName ?? c.EntityId));
                _services.Logbook.Log(
                    entityId: _heaterSwitch.EntityId,
                    message: $"Ingeschakeld voor {climateNames}",
                    name: "CV",
                    domain: "switch");
            }
        }
    }
        
    private bool NeedsHeat(ClimateEntity climate) => 
        climate.EntityState?.Attributes?.HvacAction == "heating" && 
        climate.EntityState?.Attributes?.Temperature > climate.EntityState?.Attributes?.CurrentTemperature;
}