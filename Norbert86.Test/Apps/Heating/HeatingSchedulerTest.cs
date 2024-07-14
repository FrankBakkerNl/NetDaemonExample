using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.HassModel.Entities;
using Norbert86.Test.TestHelpers;
using Xunit.Abstractions;

namespace Norbert86.Test.Apps.Heating;

public class HeatingSchedulerTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HeatingSchedulerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Simulate()
    {
        var ctx = new TestContext();
        
        var scheduler = ctx.GetRequiredService<TestScheduler>();

        ctx.HaMock.Setup(m => m.CallService("climate", It.IsAny<string>(), It.IsAny<ServiceTarget?>(), It.IsAny<object?>()))
            .Callback<string, string, ServiceTarget, object>((a, b, target, d) => { _testOutputHelper.WriteLine($"{scheduler.Now.ToLocalTime().DayOfWeek.ToString()[0..3]} {scheduler.Now.ToLocalTime().TimeOfDay} {a} {b} {target?.EntityIds?.FirstOrDefault()} {d}"); });

        scheduler.AdvanceTo(new DateTime(2021, 1, 11, 0, 0, 0).ToUniversalTime().Ticks);

        var app = ctx.GetApp<ClimatesOff>();

        while (scheduler.Now < new DateTime(2021, 1, 11, 0, 0, 0).AddDays(7))
        {
            scheduler.AdvanceBy(TimeSpan.TicksPerMinute);
        }
    }
}