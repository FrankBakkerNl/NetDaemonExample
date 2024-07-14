public record ClimateZone(ClimateEntity Climate, BinarySensorEntity? Window = null);

public class ClimateZones
{
    public ClimateZones(Entities entities)
    {
        var climate = entities.Climate;
        var binarySensor = entities.BinarySensor;
        
        Zones = new ClimateZone[]
        {
            new (climate.WoonkamerThermostaat),
            new (climate.RadiatorSlaapkamerThermostat, binarySensor.RaamSlaapkamer),
            new (climate.TrvBadkamer, binarySensor.RaamBadkamer),
            new (climate.RadiatorSlaapkamerThermostat, binarySensor.RaamSuze),
            new (climate.RadiatorMariusThermostat, binarySensor.RaamMarius),
            new (climate.RadiatorZolderThermostat, binarySensor.RaamZolder),
            new (climate.RadiatorStudeerkamerThermostat),
        };
    }
    public IEnumerable<ClimateZone> Zones { get;}
   
}