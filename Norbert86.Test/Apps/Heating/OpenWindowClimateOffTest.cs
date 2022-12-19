using HomeAssistantGenerated;
using Moq;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Heating;

public class OpenWindowClimateOffTest : TestBase
{
    [Fact]
    public void OpenWindowWhileHeating()
    {
        Context.GetApp<OpenWindowClimateOff>();

        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat");
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "on");
        
        HaMock.VerifyServiceCalled(
            Entities.Climate.RadiatorBadkamerThermostat, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "off"},
            Times.Once());
    }
    
    [Fact]
    public void StartHeatingWhileWindowOpen()
    {
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "on");

        Context.GetApp<OpenWindowClimateOff>();
        
        HaMock.TriggerStateChange(Entities.Climate.RadiatorBadkamerThermostat, "heat");
        
        HaMock.VerifyServiceCalled(
            Entities.Climate.RadiatorBadkamerThermostat, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "off"},
            Times.Once());
    }
}