using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using HomeAssistantGenerated;
using NetDaemon.Common;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.Helpers;

namespace Audi
{
    [NetDaemonApp]
    public class AudiApp
    {
        private Entities _entities;
        private Services _services;

        public AudiApp(IHaContext ha, INetDaemonScheduler scheduler)
        {
            _entities = new (ha);
            _services = new (ha);
            scheduler.RunDaily(new TimeOnly(21, 0), CheckCable);
        }

        private async void CheckCable()
        {
            var shouldPlugIn = 
                _entities.Sensor.AudiETronPlugState.State == "disconnected" &&
                _entities.Sensor.AudiETronChargingState.AsNumeric().State < 90;

            if (!shouldPlugIn) return;

            _services.Notify.MobileAppOneplusA6003(
                title: "🚘 Audi cable is unplugged",
                message: $"🔋 {_entities.Sensor.AudiETronStateOfCharge.AsNumeric().State ?? 0:N0}% " +
                         $"🏁 {_entities. Sensor.AudiETronPrimaryEngineRange.State}km",
                data: new {tag = "ChargeCableNotification" });

            await _entities.Sensor.AudiETronPlugState.StateChanges().Where(e => e.New?.State == "connected").Take(1).ToTask();

            _services.Notify.MobileAppOneplusA6003(
                message: "clear_notification",
                data: new { tag = "ChargeCableNotification" });
        }
    }
}
