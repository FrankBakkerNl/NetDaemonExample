[NetDaemonApp]
public class AudiApp
{
    public AudiApp(Entities entities, Services services, IScheduler scheduler)
    {
        scheduler.ScheduleCron("0 21 * * *", CheckCable);

        void CheckCable()
        {
            if (entities.Sensor.AudiETronPlugState.State == "disconnected" &&
                entities.Sensor.AudiETronStateOfCharge.State < 90)
            {
                SendMessage();
                entities.Sensor.AudiETronPlugState.StateChanges()
                    .Where(e => e.New?.State == "connected").Take(1)
                    .Subscribe(_ => ClearMessage());
            }
        }

        void SendMessage() =>
            services.Notify.MobileAppPhoneFrank(
                title: $"Charger cable is {entities.Sensor.AudiETronPlugState.State}",
                message: $"🔋 {entities.Sensor.AudiETronStateOfCharge.State ?? 0:N0}% " +
                         $"🏁 {entities.Sensor.AudiETronPrimaryEngineRange.State}km",
                data: new
                {
                    tag = "ChargeCableNotification", 
                    icon_url = "https://t4.ftcdn.net/jpg/01/13/95/01/360_F_113950177_Epw6Kf3FA1IFkAXhnicpv0VYZrBd3z48.jpg"
                });

        void ClearMessage() =>
            services.Notify.MobileAppPhoneFrank(
                message: "clear_notification",
                data: new { tag = "ChargeCableNotification" });
    } 
}