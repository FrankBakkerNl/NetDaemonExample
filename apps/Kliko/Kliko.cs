[NetDaemonApp]
public class KlikoControle
{
    private readonly Dictionary<string, (BinarySensorEntity, NumericSensorEntity)> _sensorMap;
    private readonly Entities _entities;
    private readonly Services _services;

    public KlikoControle(IHaContext ha, IScheduler scheduler)
    {
        _entities = new(ha);
        _services = new(ha);
        
        _sensorMap = new Dictionary<string, (BinarySensorEntity, NumericSensorEntity)>()
        {
            ["gft"]    = (_entities.BinarySensor.TrackerGftPresence, _entities.Sensor.TrackerGftRssiValue),
            ["papier"] = (_entities.BinarySensor.TrackerPapierPresence, _entities.Sensor.TrackerPapierRssiValue), 
            ["pbd"]    = (_entities.BinarySensor.TrackerPmdPresence, _entities.Sensor.TrackerPmdRssiValue),
        };

        scheduler.ScheduleCron("0 6 * * * ", () =>
        {
            Observable.Interval(TimeSpan.FromMinutes(10)).TakeUntil(_ => scheduler.Now.TimeOfDay >= TimeSpan.FromHours(9)).Subscribe(_ => CheckKliko());
        });
    }

    private void CheckKliko()
    {
        var currentKliko = _entities.Sensor.AfvalinfoToday.State ?? "";
        if (!_sensorMap.TryGetValue(currentKliko, out var sensors)) return;

        var (presence, rssi) = sensors;

        if (!presence.IsOn()) return;

        SetMessage(currentKliko);
        
        rssi.StateAllChanges().Where(s=>s.Entity.State > 80)
            .Throttle(TimeSpan.FromMinutes(2)).Take(1).Subscribe(_ => ClearMessage());
    }

    private void SetMessage(string currentKliko)
    {
        _services.Notify.MobileAppPhoneFrank(
            title: "♻️\t"+ currentKliko,
            message: $"De {currentKliko} kliko moet naar buiten" ,
            data: new { tag= "kliko notification" }
        );
    }

    private void ClearMessage()
    {
        _services.Notify.MobileAppPhoneFrank(
            message: "clear_notification",
            data: new { tag = "kliko notification" }
        );
    }
}