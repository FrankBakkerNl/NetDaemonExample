namespace HomeAssistantGenerated;

public partial class Entities
{
     public IEnumerable<Entity> EnumerateAll() => 
         _haContext.GetAllEntities()
             .Select(ToTypedEntity);


    public static Entity ToTypedEntity(Entity entity) =>
        entity.EntityId[..entity.EntityId.IndexOf(".")] switch
        {
            "automation" => new AutomationEntity(entity),
            "binary_sensor" => new BinarySensorEntity((entity)),
            "climate" => new ClimateEntity(entity),
            _ => entity
        };
}