using System.Reactive.Subjects;
using System.Text.Json;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace Norbert86.Test.TestHelpers;

public class HaContextMockImpl : IHaContext
{
    public Dictionary<string, EntityState> _entityStates { get; } = new ();
    public Subject<StateChange> StateAllChangeSubject { get; } = new();
    public Subject<Event> EventsSubject { get; } = new();

    public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

    public EntityState? GetState(string entityId) => _entityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => _entityStates.Keys.Select(s => new Entity(this, s)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        if (target?.EntityIds is null) return;
        
        if (service == "turn_on")
        {
            foreach (var entityId in target.EntityIds)
            {
                TriggerStateChange(entityId, "on");
            }
        }
        if (service == "turn_off")
        {
            foreach (var entityId in target.EntityIds)
            {
                TriggerStateChange(entityId, "off");
            }
        }
        
    }
    
    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        throw new NotSupportedException();
    }

    public Area? GetAreaFromEntityId(string entityId) => null;
    public EntityRegistration? GetEntityRegistration(string entityId) => new(){Id = entityId};

    public virtual void SendEvent(string eventType, object? data = null)
    { }

    public IObservable<Event> Events => EventsSubject;
   
    
    public void TriggerStateChange(string entityId, string newStatevalue, object? attributes = null)
    {
        var newState = new EntityState { State = newStatevalue };
        if (attributes != null)
        {
            newState = newState with {AttributesJson = attributes.AsJsonElement()};
        }
        
        TriggerStateChange(entityId, newState);
    }    
    
    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = _entityStates.TryGetValue(entityId, out var current) ? current : null;
        _entityStates[entityId] = newState;
        StateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }
}

public class HaContextMock : Mock<HaContextMockImpl>
{

    public HaContextMock()
    {
        this.CallBase = true;
    }
    public void TriggerStateChange(Entity entity, string newStatevalue, object? attributes = null)
    {
        var newState = new EntityState { State = newStatevalue };
        if (attributes != null)
        {
            newState = newState with {AttributesJson = attributes.AsJsonElement()};
        }
        
        TriggerStateChange(entity.EntityId, newState);
    }    
    
    public void TriggerStateChange(string entityId, EntityState newState)
    {
        Object.TriggerStateChange(entityId, newState);
    }

    public void VerifyServiceCalled(Entity entity, string domain, string service, object? data = null) =>
        VerifyServiceCalled(entity, domain, service, data, Times.Once());

    
    public void VerifyServiceCalled(Entity entity, string domain, string service, object? data, Times times)
    {
        Verify(m => m.CallService(domain, service, 
            It.Is<ServiceTarget?>(s => s!.EntityIds!.SingleOrDefault() == entity.EntityId),
            data), times);        
    }

    public void TriggerEvent(Event @event)
    {
        Object.EventsSubject.OnNext(@event);
    }
 }

public static class TestExtensions
{
    public static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}


