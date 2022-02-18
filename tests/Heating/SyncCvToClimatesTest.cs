using System.Linq;
using HomeAssistantGenerated;
using Moq;
using NetDaemon.HassModel.Entities;
using Xunit;

namespace daemonapp_test.Heating;

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

        var app = Context.GetApp<SyncCVToClimates>();
        
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat",
            new ClimateAttributes
            {
                CurrentTemperature = 19,
                HvacAction = "heating",
                Temperature = 20
            });
        
        HaMock.VerifyServiceCalled(Entities.Switch.CvUpstairsRelay, "switch", "turn_on");
    }
}