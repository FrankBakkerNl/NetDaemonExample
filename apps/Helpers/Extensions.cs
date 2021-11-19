using System;
using System.Reactive.Linq;
using HomeAssistantGenerated;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Helpers
{
    public static class Extensions
    {
        public static Entities MyEntities(this IHaContext ha) => new (ha);
        public static Services Services(this IHaContext ha) => new (ha);
        
      
        public static void SwitchTo(this SwitchEntity? heaterSwitch, string? newState)
        {
            if (heaterSwitch?.State != newState)
            {
                if (newState == "on")
                {
                    heaterSwitch?.TurnOn();
                }
                else
                {
                    heaterSwitch?.TurnOff();
                }
            }
        }

        public static void RunDaily(this INetDaemonScheduler scheduler, TimeSpan timeOfDay, Action action)
        {
            var startTime = scheduler.Now.Date.Add(timeOfDay);
            if (startTime < scheduler.Now) startTime = startTime.AddDays(1);

            scheduler.RunEvery(TimeSpan.FromDays(1), startTime, action);
        }
        
        public static void RunDaily(this INetDaemonScheduler scheduler, TimeOnly timeOfDay, Action action)
        {
            var startTime = DateOnly.FromDateTime(scheduler.Now.Date).ToDateTime(timeOfDay);
            if (startTime < scheduler.Now) startTime = startTime.AddDays(1);

            scheduler.RunEvery(TimeSpan.FromDays(1), startTime, action);
        }

        public static IDisposable WhenTurnsOn<TEntity, TAttributes>(this Entity<TEntity, EntityState<TAttributes>, TAttributes> entity,
            Action<StateChange<TEntity, EntityState<TAttributes>>> action)
            where TAttributes : class
            where TEntity : Entity<TEntity, EntityState<TAttributes>, TAttributes>
            => entity.StateChanges().Where(c => c.New?.IsOn() ?? false).Subscribe(action);
    }
}