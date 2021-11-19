global using System;
using NetDaemon.Common;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.Helpers;

namespace HomeAssistantGenerated.apps.Ping
{
    [NetDaemonApp]
    public class PingND
    {
        private readonly IHaContext _ha;
        private readonly INetDaemonScheduler _scheduler;

        public PingND(IHaContext ha, INetDaemonScheduler scheduler)
        {
            _ha = ha;
            _scheduler = scheduler;
            var inputBooleanPingpong = ha.MyEntities().InputBoolean.Netdaemonpingpong;
            
            inputBooleanPingpong.WhenTurnsOn(OnNext);
            inputBooleanPingpong.TurnOff();
            _ha.Services().Logbook.Log("Ping", "NetDaemon started", inputBooleanPingpong.EntityId);
        }

        private void OnNext(StateChange<InputBooleanEntity, EntityState<InputBooleanAttributes>> obj)
        {
            _scheduler.RunIn(TimeSpan.FromSeconds(1), () => obj.Entity.TurnOff());
            _ha.Services().Logbook.Log("Ping", "NetDaemon was pinged", obj.Entity.EntityId);
        }
    }
}