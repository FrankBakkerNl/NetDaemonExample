using HomeAssistantGenerated;
using Moq;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Laundy;

public class LaundyMonitorTest : TestBase
{
    [Fact]
    public void WasherCycle()
    {
        Context.GetApp<LaundyMonitor>();
        HaMock.TriggerStateChange(Entities.Sensor.WasherState, "Running");

        HaMock.TriggerStateChange(Entities.Sensor.WasherState, "Ready");
        HaMock.Verify(h => h.CallService("notify", "mobile_app_phone_frank", null,
            It.Is<NotifyMobileAppPhoneFrankParameters>(p => p.Title != null &&  p.Title.Contains("Washer finished"))));
        HaMock.Reset();

        HaMock.TriggerStateChange(Entities.Sensor.WasherState, "Running");
        HaMock.Verify(h => h.CallService("notify", "mobile_app_phone_frank", null, 
            It.Is<NotifyMobileAppPhoneFrankParameters>(p => p.Message == "clear_notification")));
    }
    
    [Fact]
    public void DryerCycle()
    {
        Context.GetApp<LaundyMonitor>();
        HaMock.TriggerStateChange(Entities.Sensor.DryerState, "Running");

        HaMock.TriggerStateChange(Entities.Sensor.DryerState, "Ready");
        HaMock.Verify(h => h.CallService("notify", "mobile_app_phone_frank", null,
            It.Is<NotifyMobileAppPhoneFrankParameters>(p =>p.Title!=null &&  p.Title.Contains("Dryer finished"))));
        HaMock.Reset();

        HaMock.TriggerStateChange(Entities.Sensor.DryerState, "off");
        HaMock.Verify(h => h.CallService("notify", "mobile_app_phone_frank", null, 
            It.Is<NotifyMobileAppPhoneFrankParameters>(p => p.Message == "clear_notification")));
    }
    
}