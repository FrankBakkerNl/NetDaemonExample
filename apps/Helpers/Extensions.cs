using Microsoft.Extensions.DependencyInjection;

namespace Norbert.Apps.Helpers;

public static class Extensions
{

    public static IServiceCollection AddGeneratedCode(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddTransient<Entities>()
            .AddTransient<Services>();
        
    public static IServiceCollection AddConfigs(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddTransient<ClimateZones>();
    
    public static Entities MyEntities(this IHaContext ha) => new (ha);
    public static Services Services(this IHaContext ha) => new (ha);
        

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

    public static IDisposable WhenTurnsOn<TEntity, TAttributes>(this Entity<TEntity, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<TEntity, EntityState<TAttributes>>> action)
        where TAttributes : class
        where TEntity : Entity<TEntity, EntityState<TAttributes>, TAttributes>
        => entity.StateChanges().Where(c => c.New?.IsOn() ?? false).Subscribe(action);
    
    public static Entity ToTypedEntity(this Entity entity) =>
        entity.EntityId[..entity.EntityId.IndexOf(".")] switch
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
        where TState : NetDaemon.HassModel.Entities.EntityState =>
        source.Subscribe(e =>
        {
            var wasTrue = pattern(e?.Old);
            var isTrue = pattern(e?.New);
            if (!wasTrue && isTrue) whenBecomesTrue();
            if (wasTrue && !isTrue) whenBecomesFalse();
        });

    public static Entity GetTypedEntity(Entity entity) =>
        entity.EntityId[entity.EntityId.IndexOf(".") ..] switch
        {
            "automation" => new AutomationEntity(entity),
            "climate" => new ClimateEntity(entity),
            "light" => new LightEntity(entity),
            // ...
            _ => entity
        };
}