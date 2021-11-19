using HomeAssistantGenerated;
using NetDaemon.Helpers;


[NetDaemonApp]
public class ClimatesOff
{
    public ClimatesOff(IHaContext ha, INetDaemonScheduler scheduler)
    {
        scheduler.RunDaily(new TimeOnly(17, 0), AllOff);
        scheduler.RunDaily(new TimeOnly(0, 0), AllOff);

        void AllOff()
        {
            var climateEntities = ha.MyEntities().Climate;
            climateEntities.RadiatorSlaapkamerThermostat.SetHvacMode("off");
            climateEntities.RadiatorBadkamerThermostat.SetHvacMode("off");
            climateEntities.RadiatorSuzeThermostat.SetHvacMode("off");
            climateEntities.RadiatorMariusThermostat.SetHvacMode("off");
            climateEntities.RadiatorStudeerkamerThermostat.SetHvacMode("off");
            climateEntities.RadiatorZolderThermostat.SetHvacMode("off");
        }
    }
}
