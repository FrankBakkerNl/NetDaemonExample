using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test.Apps.Lights;

public class KeyPadTest : TestBase
{
    private const string startHoldButtonEvent = """
        {
            "event_type": "zha_event",
            "data": {
                "device_ieee": "84:71:27:ff:fe:40:78:7b",
                "unique_id": "84:71:27:ff:fe:40:78:7b:4:0x0008",
                "device_id": "56aa6e3e2dbc6fbd6093c4ce85744e77",
                "endpoint_id": 3,
                "cluster_id": 8,
                "command": "move_with_on_off",
                "args": [
                    0,
                    50
                ]
            },
            "origin": "LOCAL",
            "time_fired": "2021-11-12T15:35:49.493480+00:00",
            "context": {
                "id": "49f79a459d1ac8b425848224da4c9149",
                "parent_id": null,
                "user_id": null
            }
        }
        """;

    private const string stopHoldButtonEvent = """
        {
            "event_type": "zha_event",
            "data": {
                "device_ieee": "84:71:27:ff:fe:40:78:7b",
                "unique_id": "84:71:27:ff:fe:40:78:7b:4:0x0008",
                "device_id": "56aa6e3e2dbc6fbd6093c4ce85744e77",
                "endpoint_id": 3,
                "cluster_id": 8,
                "command": "stop",
                "args": []
            },
            "origin": "LOCAL",
            "time_fired": "2021-11-12T15:35:53.180927+00:00",
            "context": {
                "id": "0578ccf53cca21799b2e119cc520c580",
                "parent_id": null,
                "user_id": null
            }
        }
        """;
        
    [Fact]
    public void TestKeypad3()
    {
        var scheduler = Context.GetRequiredService<TestScheduler>();

        Context.GetApp<KeypadKitchen>();

        // Start holding Button
        var startHoldEent = JsonSerializer.Deserialize<Event>(startHoldButtonEvent)!; 
        HaMock.TriggerEvent(startHoldEent);
            
        // Hold for 1 second
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
        HaMock.Verify(m=>m.CallService("light", "turn_on", 
            It.Is((ServiceTarget s) =>s.EntityIds!.Contains("light.alle_spots_woonkamer")), It.IsAny<object?>()),
            Times.Exactly(4));

        // Stop hold Button
        var stopHoldEvent = JsonSerializer.Deserialize<Event>(stopHoldButtonEvent)!; 
        HaMock.TriggerEvent(stopHoldEvent);

        // Assert
        scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
        HaMock.VerifyNoOtherCalls();
    }
}