[NetDaemonApp]
public class EvChargeMonitor
{
    public EvChargeMonitor(Entities entities, Services services, IScheduler scheduler, EsphomeServices esphomeServices)
    {
        scheduler.ScheduleCron("0 21 * * *", CheckCable);

        entities.DeviceTracker.Ix3MSport
            .StateChanges()
            .Where(s=>s.New?.State == "home" && entities.Sensor.Ix3MSportRemainingRangeElectric.State < 200)
            .Subscribe(_ => SendMessageThuiskomst());

        void CheckCable()
        {
            if (entities.BinarySensor.Ix3MSportConnectionStatus.IsOff() &&
                entities.Sensor.Ix3MSportRemainingRangeElectric.State < 200)
            {
                SendMessage();
                entities.BinarySensor.Ix3MSportConnectionStatus.StateChanges()
                    .Where(e => e.New.IsOn()).Take(1)
                    .Subscribe(_ => ClearMessage());
            }
        }

        void SendMessage()
        {
            var message = $"Accu {entities.Sensor.Ix3MSportRemainingBatteryPercent.State ?? 0:N0}% - " +
                          $" {entities.Sensor.Ix3MSportRemainingRangeElectric.State}km";
            esphomeServices.EspKitchenPanelNotificationShow("BWM iX3 is niet aan het laden", message);
            esphomeServices.EspKitchenPanelPlayRtttl("MarioPipe:d=16,o=5,b=100:e6,d#6,d6,c#6,c6,p");
            
            services.Notify.MobileAppPhoneFrank(
                title: $"BWM iX3 is niet aan het laden",
                message: $"🔋 {entities.Sensor.Ix3MSportRemainingBatteryPercent.State ?? 0:N0}% " +
                         $"🏁 {entities.Sensor.Ix3MSportRemainingRangeElectric.State}km",
                data: new
                {
                    tag = "ChargeCableNotification",
                    icon_url =
                        "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0Dh1kXMrLlzSFHjWFfzgrlE5WgXrbXbbJWA&usqp=CAU"
                });
        }
        
        void SendMessageThuiskomst()
        {
            services.Notify.MobileAppPhoneFrank(
                title: $"BWM iX3 moet straks laden",
                message: $"🔋 {entities.Sensor.Ix3MSportRemainingBatteryPercent.State ?? 0:N0}% " +
                         $"🏁 {entities.Sensor.Ix3MSportRemainingRangeElectric.State}km",
                data: new
                {
                    tag = "ChargeCableNotification",
                    icon_url =
                        "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0Dh1kXMrLlzSFHjWFfzgrlE5WgXrbXbbJWA&usqp=CAU"
                });
        }

        void ClearMessage() =>
            services.Notify.MobileAppPhoneFrank(
                message: "clear_notification",
                data: new { tag = "ChargeCableNotification" });
    } 
}