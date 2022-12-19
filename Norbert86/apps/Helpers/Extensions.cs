using Microsoft.Extensions.DependencyInjection;

namespace Norbert.Apps.Helpers;

public static class Extensions
{
    public static IServiceCollection AddNorbert86Services(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddTransient<ClimateZones>()
            .AddTransient<BrightnessSlider>()
            .AddTransient(s => s.GetRequiredService<SunEntities>().Sun)
        ;
    
    public static void SwitchTo(this LightEntity? light, string? newState)
    {
        if (light?.State != newState)
        {
            if (newState == "on")
            {
                light?.TurnOn();
            }
            else
            {
                light?.TurnOff();
            }
        }
    }
    
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

    public static void RunDaily(this INetDaemonScheduler scheduler, TimeOnly timeOfDay, Action action)
    {
        var now = scheduler.Now.LocalDateTime.AddSeconds(1); // add some time to be sure we do not schedule for a time in the past 
        DateTime startDateTimeToday = now.Date.Add(timeOfDay.ToTimeSpan());
        
        var startTime = startDateTimeToday < now ? startDateTimeToday.AddDays(1) : startDateTimeToday;
        var startOffset = (DateTimeOffset)startTime;
        
        scheduler.RunEvery(TimeSpan.FromDays(1), startOffset, action);
    }

    public static IDisposable WhenTurnsOn<TEntity, TAttributes>(this Entity<TEntity, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<TEntity, EntityState<TAttributes>>> action)
        where TAttributes : class
        where TEntity : Entity<TEntity, EntityState<TAttributes>, TAttributes>
        => entity.StateChanges().Where(c => c.New?.IsOn() ?? false).Subscribe(action);
    
    public static IDisposable WhenTurnsOff<TEntity, TAttributes>(this Entity<TEntity, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<TEntity, EntityState<TAttributes>>> action)
        where TAttributes : class
        where TEntity : Entity<TEntity, EntityState<TAttributes>, TAttributes>
        => entity.StateChanges().Where(c => c.New?.IsOff() ?? false).Subscribe(action);    
    
    public static Entity ToTypedEntity(this Entity entity) =>
        entity.EntityId[0..(entity.EntityId.IndexOf("."))] switch
        {
            "automation" => new AutomationEntity(entity),
            "binary_sensor" => new BinarySensorEntity((entity)),
            "climate" => new ClimateEntity((entity)),
            _ => entity
        };
    
    public static IDisposable Track<T, TState>(this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> pattern, 
        Action whenBecomesTrue, 
        Action whenBecomesFalse) 
        where T : Entity 
        where TState : EntityState =>
        source.Subscribe(e =>
        {
            var wasTrue = pattern(e?.Old);
            var isTrue = pattern(e?.New);
            if (!wasTrue && isTrue) whenBecomesTrue();
            if (wasTrue && !isTrue) whenBecomesFalse();
        });
        
        public static IObservable<T> ThrottleTime<T>(this IObservable<T> source, TimeSpan ts)
        {
            return ThrottleTime(source, ts, Scheduler.Default);
        }
        
        public static IObservable<T> ThrottleTime<T>(this IObservable<T> source, TimeSpan ts, IScheduler scheduler)
        {
            return source
                .Timestamp(scheduler)
                .Scan((EmitValue: false, OpenTime: DateTimeOffset.MinValue, Item: default(T)!), 
                    (state, item) => item.Timestamp > state.OpenTime
                    ? (true, item.Timestamp + ts, item.Value)
                    : (false, state.OpenTime, item.Value)
                )
                .Where(t => t.EmitValue)
                .Select(t => t.Item);
        }
        
        /// <summary>
        /// listens to all state changes, and prepends the current state 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        // public static IObservable<StateChange> StateAllChangesWithCurrent(this Entity entity)
        // {
        //     return entity.StateAllChanges().Prepend(new StateChange(entity, null, entity.EntityState));
        // }

        
        public static IObservable<StateChange<TEntity, TState>> StateAllChangesWithCurrent<TEntity, TState, TAttributes>(this TEntity entity) 
            where TEntity : Entity<TEntity, TState, TAttributes>
            where TState : EntityState<TAttributes>
            where TAttributes : class
        {
            return entity.StateAllChanges().Prepend(new StateChange<TEntity, TState>(entity, null, entity.EntityState));
        }
}