using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.HassModel.Entities;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Heating;

public class MonitorElgaTest : TestBase
{
    [Fact]
    void TestOTGWConnected()
    {
        HaMock.TriggerStateChange(Entities.BinarySensor.OpenthermGatewayOtgwOtgwBoilerConnected, "on");
        var testScheduler = Context.GetRequiredService<TestScheduler>();
        Context.GetApp<MonitorElga>();
        
        HaMock.TriggerStateChange(Entities.BinarySensor.OpenthermGatewayOtgwOtgwBoilerConnected, "off");
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);

        HaMock.Verify(m => m.CallService("notify", "mobile_app_phone_frank", It.IsAny<ServiceTarget>(),
            It.Is<NotifyMobileAppPhoneFrankParameters>(o => o.Message!.Contains("Storing:"))), Times.Once);

        HaMock.TriggerStateChange(Entities.BinarySensor.OpenthermGatewayOtgwOtgwBoilerConnected, "on");
        HaMock.Verify(m => m.CallService("notify", "mobile_app_phone_frank", It.IsAny<ServiceTarget>(), It.Is<NotifyMobileAppPhoneFrankParameters>(o => o.Message!.Contains("Storing verholpen:"))),
            Times.Once);
    }
}