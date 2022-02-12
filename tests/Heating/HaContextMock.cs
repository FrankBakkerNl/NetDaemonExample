using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace daemonapp_test.Heating;


public class HaCOntextMockImpl : IHaContext
{
    public Dictionary<string, EntityState> _entityStates = new ();
    public Subject<StateChange> StateAllChangeSubject { get; } = new();
    public Subject<Event> EventsSubject { get; } = new();

    public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

    public EntityState? GetState(string entityId) => _entityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => _entityStates.Keys.Select(s => new Entity(this, s)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    { }

    public Area? GetAreaFromEntityId(string entityId) => null;

    public void SendEvent(string eventType, object? data = null)
    {
    }

    public IObservable<Event> Events => EventsSubject;
}

public class HaContextMock : Mock<HaCOntextMockImpl>
{
    public void UpdateState(string entityId, EntityState newState)
    {
        var oldState = Object._entityStates.TryGetValue(entityId, out var current) ? current : null;
        Object._entityStates[entityId] = newState;
        Object.StateAllChangeSubject.OnNext(new StateChange(new Entity(this.Object, entityId), oldState, newState));
    }
 }

public static class TestExtensions
{
    public static EntityState WithAttributes(this EntityState entityState, object attributes)
    {
        var copy = entityState with {};
        entityState.GetType().GetProperty("AttributesJson", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(copy, AsJsonElement(attributes));
        return copy;
    }
    
    public static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}


