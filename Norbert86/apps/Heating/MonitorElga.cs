[NetDaemonApp]
public class MonitorElga
{
    private readonly Services _services;

    public MonitorElga(Entities entities, Services services, IScheduler scheduler)
    {
        _services = services;
        
        // Send notification when Elga reports an error
        AlertOnState(entities.BinarySensor.OpenthermGatewayOtgwOtgwFault.StateChanges(), e =>e.IsOn(), 
            TimeSpan.FromSeconds(1), scheduler, 
            "Warmtepomp fout");
        
        AlertOnState(entities.BinarySensor.OpenthermGatewayOtgwOtgwDiagonosticIndicator.StateChanges(), e =>e.IsOn(), 
            TimeSpan.FromSeconds(1), scheduler, 
            "Warmtepomp diagnostic indicator");

        AlertOnState(entities.BinarySensor.OpenthermGatewayOtgwOtgwBoilerConnected.StateChanges(), e => e.IsOff(),
            TimeSpan.FromMinutes(2), scheduler, 
            "OTGW niet verbonden met Warmtepomp");
        
        AlertOnState(entities.BinarySensor.OpenthermGatewayOtgwOtgwBoilerConnected.StateChanges(), e => !e.IsOn() && !e.IsOff(),
            TimeSpan.FromMinutes(2), scheduler, 
            "OTGW niet beschikbaar");
    }

    public void AlertOnState<TEntity, TEntityState>(
        IObservable<StateChange<TEntity, TEntityState>> observable,
        Func<TEntityState?, bool> predicate,
        TimeSpan timeSpan,
        IScheduler scheduler,
        string message)
        where TEntity : Entity
        where TEntityState : EntityState
    {
        observable.WhenStateIsFor(predicate, timeSpan, scheduler).Subscribe(e =>
        {
            _services.Esphome.EspKitchenPanelNotificationShow("Fout!", message);
            _services.Notify.MobileAppPhoneFrank("Storing:" + message, data: new
            {
                tag = "MonitorAlert",
            });
            
            observable.Where(e => !predicate(e.New)).Take(1).Subscribe(e =>
            {
                _services.Esphome.EspKitchenPanelNotificationClear();
                _services.Notify.MobileAppPhoneFrank("Storing verholpen:" + message, data: new
                {
                    tag = "MonitorAlert",
                });
            });
        });
    }
    
}