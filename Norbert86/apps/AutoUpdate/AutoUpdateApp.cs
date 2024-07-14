[NetDaemonApp]
public class AutoUpdateApp
{
    private readonly Services _services;
    private readonly ILogger<AutoUpdateApp> _logger;
    private readonly UpdateEntity[] _monitorUpdates;

    public AutoUpdateApp(UpdateEntities updates, Services services, IScheduler scheduler, ILogger<AutoUpdateApp> logger)
    {
        _services = services;
        _logger = logger;
        _monitorUpdates = [updates.AcIwSk1, updates.U6LiteStk, updates.U6LiteWk, updates.Usl8lp];
        scheduler.ScheduleCron("0 3 * * *", AutoUpdate);
    }

    private async void AutoUpdate()
    {
        var needUpdate = _monitorUpdates.Where(u => u.IsOn()).ToArray();
        if (!needUpdate.Any()) return;

        var names = string.Join(",", needUpdate.Select(u => u.Attributes?.FriendlyName ?? u.EntityId));
        _logger.LogInformation($"updating {names}");
        
        foreach (var updateEntity in needUpdate)
        {
            _logger.LogInformation($"Start updating {updateEntity.Attributes?.FriendlyName ?? updateEntity.EntityId}");
            _services.Notify.MobileAppPhoneFrank($"installing update for {updateEntity.Attributes?.FriendlyName ?? updateEntity.EntityId}");

            updateEntity.Install();
            
            await updateEntity.StateChanges().Where(s => s.New.IsOff() && s.New?.Attributes?.InProgress == false).Take(1);
            _logger.LogInformation($"Ready updating {updateEntity.Attributes?.FriendlyName ?? updateEntity.EntityId}");
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}