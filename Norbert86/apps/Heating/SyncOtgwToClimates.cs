[NetDaemonApp]
public class SyncOtgwToClimates
{
    const string GatewayId = "otgw_elga";

    private readonly Entities _entities;
    private readonly Services _services;
    private readonly IScheduler _scheduler;
    private readonly IEnumerable<ClimateEntity> _climates;
    private readonly IEnumerable<ClimateEntity> _trvs;
    private readonly ClimateEntity _woonkamerClimate;

    public SyncOtgwToClimates(ClimateZones climateZones, Entities entities, Services services, IScheduler scheduler)
    {
        _entities = entities;
        _services = services;
        _scheduler = scheduler;
        _climates = climateZones.Zones.Select(z => z.Climate).ToArray();
        _trvs = _climates.Except(new[] { entities.Climate.WoonkamerThermostaat });
        _woonkamerClimate = entities.Climate.WoonkamerThermostaat;


        _climates.StateAllChanges().Subscribe(_ => SetCvState());

        // Must repeat control set point withing 1 minute, even if already enabled
        scheduler.SchedulePeriodic(TimeSpan.FromSeconds(30), SetCvState);
        SetCvState();

        // stop boost mode after 10 minutes
        entities.InputBoolean.ClimateBoost.StateChanges()
            .WhenStateIsFor(s =>s.IsOn(), TimeSpan.FromMinutes(30), _scheduler)
            .Subscribe(t =>
            {
                t.Entity.TurnOff();
                SetCvState();
            });

        entities.InputNumber.ChMaxModulation.StateChanges().Subscribe(e =>
            services.OpenthermGw.SetMaxModulation(GatewayId, (long)(e.New?.State ?? 100)));
    }

    private void SetCvState()
    {
        var heatingClimates = _climates.Where(NeedsHeat).ToList();

        if (heatingClimates.Any())
        {
            SetOn(heatingClimates);
        }
        else
        {
            SetOff();
        }
    }

    private void SetOn(List<ClimateEntity> heatingClimates)
    {
        // repeat setPoint regardless of current state to avoid it to timeout 
        var setPoint = _entities.InputBoolean.ClimateBoost.IsOn() 
            ? _entities.InputNumber.ChBoostSetpoint.State : _entities.InputNumber.ChDefaultSetpoint.State;
        
        _services.OpenthermGw.SetControlSetpoint(GatewayId, setPoint ?? 35);

        if (_entities.BinarySensor.OpenthermGatewayOtgwOtgwCentralHeatingEnable.IsOff())
        {
            var climateNames = string.Join(Environment.NewLine,
                heatingClimates.Select(c => c.Attributes?.FriendlyName ?? c.EntityId));

            _services.Logbook.Log(
                entityId: _entities.BinarySensor.OpenthermGatewayOtgwOtgwCentralHeatingEnable.EntityId,
                message: $"Ingeschakeld voor {climateNames}",
                name: "CV",
                domain: "binary_sensor");
        }
    }

    // private DateTimeOffset? _lastBoostStarted;   
    // private DateTimeOffset? _lastBoostEnded;
    // private bool _boostModeOn;
    // private void CheckBoostMode()
    // {
    //     if (_trvs.Any(c => RequiredIncrease(c) > 4) && RequiredIncrease(_entities.Climate.WoonkamerThermostaat) < 1
    //         && _scheduler.Now - _lastBoostStarted < TimeSpan.FromMinutes(20))
    //     {
    //         _lastBoostStarted = _scheduler.Now;
    //         _entities.Switch.HeatingValvesDownstairsForceClose.TurnOn();
    //         _scheduler.Schedule(TimeSpan.FromMinutes(2), () => { _entities.InputBoolean.ClimateBoost.TurnOn(); });
    //     }
    // }

    private void SetOff()
    {
        if (_entities.BinarySensor.OpenthermGatewayOtgwOtgwCentralHeatingEnable.IsOn())
        {
            _services.OpenthermGw.SetControlSetpoint(GatewayId, 0);
        }
    }

    double RequiredIncrease(ClimateEntity climate)
    {
        return Math.Min(0,
            climate.EntityState?.Attributes?.HvacAction != "heating" ? 0.0 :
            climate.EntityState?.Attributes?.Temperature - climate.EntityState?.Attributes?.CurrentTemperature ?? 0);
    }
    
    private bool NeedsHeat(ClimateEntity climate) => 
        climate.EntityState?.Attributes?.HvacAction is "heating" or null && 
        climate.EntityState?.Attributes?.Temperature >= climate.EntityState?.Attributes?.CurrentTemperature;
}