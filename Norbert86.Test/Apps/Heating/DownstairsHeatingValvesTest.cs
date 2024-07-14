using NetDaemon.HassModel.Entities;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Heating;

public class DownstairsHeatingValvesTest : TestBase
{
    [Fact]
    public void TestValveControl()
    {

        HaMock.TriggerStateChange(Entities.Switch.HeatingValvesDownstairsForceClose, "off");
        HaMock.TriggerStateChange(Entities.Switch.WoonkamerThermostaatHeating, "on");
        HaMock.TriggerStateChange(Entities.Switch.HeatingValvesDownstairs, "on");
        
        Context.GetApp<DownstairsHeatingValves>();
        HaMock.TriggerStateChange(Entities.Switch.HeatingValvesDownstairsForceClose, "on");

        Entities.Switch.HeatingValvesDownstairs.State!.Should().Be("off");

        HaMock.TriggerStateChange(Entities.Switch.WoonkamerThermostaatHeating, "on");
        Entities.Switch.HeatingValvesDownstairs.State!.Should().Be("off");

        HaMock.TriggerStateChange(Entities.Switch.WoonkamerThermostaatHeating, "off");
        Entities.Switch.HeatingValvesDownstairs.State!.Should().Be("off");
   
        HaMock.TriggerStateChange(Entities.Switch.HeatingValvesDownstairsForceClose, "off");
        Entities.Switch.HeatingValvesDownstairs.State!.Should().Be("off");
        
        HaMock.TriggerStateChange(Entities.Switch.WoonkamerThermostaatHeating, "on");
        Entities.Switch.HeatingValvesDownstairs.State!.Should().Be("on");
        
        
    }
}