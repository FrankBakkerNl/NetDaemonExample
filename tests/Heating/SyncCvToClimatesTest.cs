using System.Linq;
using HomeAssistantGenerated;
using Moq;
using NetDaemon.HassModel.Entities;
using Xunit;

namespace daemonapp_test.Heating;

public class SyncCvToClimatesTest
{
    [Fact]
    public void CvOnWhenHeatNeeded()
    {
        var haMock = new HaContextMock();
        
        haMock.UpdateState("switch.cv_upstairs_relay", new EntityState { State = "off" });
        
        haMock.UpdateState("climate.radiator_slaapkamer_thermostat", new EntityState{State = "heat"}
            .WithAttributes(new ClimateAttributes
            {
                CurrentTemperature = 20,
                HvacAction = "heating",
                Temperature = 20
            }));
        
        var app = new SyncCVToClimates(new ClimateZones(haMock.Object), new Entities(haMock.Object));
        
        haMock.UpdateState("climate.radiator_slaapkamer_thermostat", new EntityState(){State = "heat"}
            .WithAttributes(new ClimateAttributes
            {
                CurrentTemperature = 19,
                HvacAction = "heating",
                Temperature = 20
            }));
        
        haMock.Verify(m => m.CallService("switch", "turn_on", 
            It.Is<ServiceTarget?>(s => s!.EntityIds!.SingleOrDefault() == "switch.cv_upstairs_relay"),
            null));
    }
}
