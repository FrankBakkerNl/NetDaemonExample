using HomeAssistantGenerated;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Heating;

public class SyncCvToClimatesTest : TestBase
{
    [Fact]
    public void CvOnWhenHeatNeeded()
    {
        HaMock.TriggerStateChange(Entities.Switch.CvUpstairsRelay, "off");
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat", 
            new ClimateAttributes
            {
                CurrentTemperature = 20,
                HvacAction = "heating",
                Temperature = 20
            });

        Context.GetApp<SyncCVToClimates>();
        
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat",
            new ClimateAttributes
            {
                CurrentTemperature = 19,
                HvacAction = "heating",
                Temperature = 20
            });
        
        HaMock.VerifyServiceCalled(Entities.Switch.CvUpstairsRelay, "switch", "turn_on");
        // manually sync the state of the switch
        HaMock.TriggerStateChange(Entities.Switch.CvUpstairsRelay, "on");
        
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat",
            new ClimateAttributes
            {
                CurrentTemperature = 21,
                HvacAction = "heating",
                Temperature = 20
            });
        
        HaMock.VerifyServiceCalled(Entities.Switch.CvUpstairsRelay, "switch", "turn_off");
        
    }
}