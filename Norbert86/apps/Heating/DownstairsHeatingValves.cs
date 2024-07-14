[NetDaemonApp]
public class DownstairsHeatingValves
{
    private readonly Entities _entities;
    private readonly SwitchEntity _thermostaatHeating;
    private readonly SwitchEntity _forceClose;

    public DownstairsHeatingValves(Entities entities, IScheduler scheduler)
    {
        _entities = entities;
        _forceClose = entities.Switch.HeatingValvesDownstairsForceClose;
        _thermostaatHeating = entities.Switch.WoonkamerThermostaatHeating;

        new []{_forceClose, _thermostaatHeating}.StateAllChanges().Subscribe(_=>SetState());
        SetState();
    }
    
    void SetState()
    {
        // valve should open if Thermostat need heat, unless force close 
        var shouldOpen = _forceClose.IsOff() && _thermostaatHeating.IsOn();
        
        _entities.Switch.HeatingValvesDownstairs.SwitchTo(shouldOpen.ToOnOf());
    }
}