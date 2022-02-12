public record ClimateZone(ClimateEntity Climate, BinarySensorEntity? Window = null);

public class ClimateZones
{
    public ClimateZones(IHaContext ha)
    {
        var entities = new Entities(ha);
        var climate = entities.Climate;
        var binarySensor = entities.BinarySensor;
        
        Zones = new ClimateZone[]
        {
            new (climate.RadiatorSlaapkamerThermostat, binarySensor.RaamSlaapkamer),
            new (climate.RadiatorBadkamerThermostat, binarySensor.RaamBadkamer),
            new (climate.RadiatorSuzeThermostat, binarySensor.RaamSuze),
            new (climate.RadiatorMariusThermostat, binarySensor.RaamMarius),
            new (climate.RadiatorZolderThermostat, binarySensor.RaamZolder),
            new (climate.RadiatorStudeerkamerThermostat),
        };
    }
    public IEnumerable<ClimateZone> Zones { get;}
   
}