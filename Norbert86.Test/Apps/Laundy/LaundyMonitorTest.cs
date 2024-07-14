using HomeAssistantGenerated;
using HomeAssistantGenerated.apps.Laundry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Laundy;

public class LaundyMonitorTest : TestBase
{
    [Fact]
    public void WasherCycle()
    {
        var testScheduler = Context.GetRequiredService<TestScheduler>();
        testScheduler.AdvanceTo(new DateTime(2023, 02, 2).Ticks);
        HaMock.TriggerStateChange(Entities.Sensor.WasherPower2, "0");
        
        Context.GetApp<WasherMonitor>();
        testScheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);
        HaMock.TriggerStateChange(Entities.Sensor.WasherPower2, "2000");

        testScheduler.AdvanceBy(TimeSpan.FromMinutes(20).Ticks);
        
        HaMock.TriggerStateChange(Entities.Sensor.WasherPower2, "0");
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(3).Ticks);

        HaMock.Verify(h => h.CallService("notify", "mobile_app_phone_frank", null,
            It.Is<NotifyMobileAppPhoneFrankParameters>(p => p.Title != null &&  p.Title.Contains("Washer finished"))));
        HaMock.Reset();

        HaMock.TriggerStateChange(Entities.Sensor.WasherPower2, "2000");
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);
        
        // HaMock.TriggerStateChange(Entities.Sensor.WasherState, "Running");
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