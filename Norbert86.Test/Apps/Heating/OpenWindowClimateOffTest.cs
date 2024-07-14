using FluentAssertions.Common;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Heating;

public class OpenWindowClimateOffTest : TestBase
{
    [Fact]
    public void OpenWindowWhileHeating()
    {
        Context.GetApp<OpenWindowClimateOff>();
        var testScheduler = Context.GetRequiredService<TestScheduler>();

        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "heat", new ClimateAttributes { Temperature = 20 });
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "on");
        
        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "off"},
            Times.Once());

        // simulate the state change due to set_hvac_mode 
        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "off");
        
        // should not be called yet
        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "heat"},
            Times.Never());
        
        
        // now close the window and it should turn back on again
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "off");

        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "heat"},
            Times.Once());
    }

    [Fact]
    public void OpenWindowWhileHeating_DoNotTurnOnAfterlong_delay()
    {
        Context.GetApp<OpenWindowClimateOff>();
        var testScheduler = Context.GetRequiredService<TestScheduler>();

        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "heat");
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "on");

        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode",
            new ClimateSetHvacModeParameters { HvacMode = "off" },
            Times.Once());

        // simulate the state change due to set_hvac_mode 
        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "off");

        // should not be called yet
        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode",
            new ClimateSetHvacModeParameters { HvacMode = "heat" },
            Times.Never());
        
        testScheduler.AdvanceBy(TimeSpan.FromHours(2.3).Ticks);

        // now close the window and it should NOT turn back on again
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "off");

        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode",
            new ClimateSetHvacModeParameters { HvacMode = "heat" },
            Times.Never());
    }




    [Fact]
    public void StartHeatingWhileWindowOpen()
    {
        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.RaamBadkamer, "on");

        Context.GetApp<OpenWindowClimateOff>();
        
        HaMock.TriggerStateChange(Entities.Climate.TrvBadkamer, "heat");
        
        HaMock.VerifyServiceCalled(
            Entities.Climate.TrvBadkamer, "climate", "set_hvac_mode", new ClimateSetHvacModeParameters { HvacMode = "off"},
            Times.Once());
    }
}