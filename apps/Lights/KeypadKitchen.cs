[NetDaemonApp]
public class KeypadKitchen
{
    private readonly IScheduler _scheduler;

    public KeypadKitchen(IHaContext ha, IScheduler scheduler, Entities entities)
    {
        _scheduler = scheduler;
        var keypad = new KeyPad(ha, "84:71:27:ff:fe:40:78:7b");

        // Buttons1 => keuken
        var endpoint1 = keypad.GetEndoint(1);
        endpoint1.On.Subscribe(_ => entities.Switch.EspBwKitchen2Relay.TurnOn());
        endpoint1.Off.Subscribe(_ => entities.Switch.EspBwKitchen2Relay.TurnOff());

        // Buttons 2 => Eettafel
        var endpoint2 = keypad.GetEndoint(2);
        endpoint2.On.Subscribe(_ => OnWithStep(entities.Light.LampenEettafel));
        endpoint2.Off.Subscribe(_ => entities.Light.LampenEettafel.TurnOff());
        LongPressAdjustBrightness(endpoint2, entities.Light.LampenEettafel);

        // Buttons 3 => spots
        var endpoint3 = keypad.GetEndoint(3);
        endpoint3.On.Subscribe(_ => DuoLightCycleOnState(entities.Light.SpotsWoonkamerRechts,
            entities.Light.SpotsWoonkamerLinks));

        endpoint3.Off.Subscribe(_ => new[] {entities.Light.SpotsWoonkamerLinks, entities.Light.SpotsWoonkamerRechts}.TurnOff());

        LongPressAdjustBrightness(endpoint3, entities.Light.SpotsWoonkamerLinks);
        LongPressAdjustBrightness(endpoint3, entities.Light.SpotsWoonkamerRechts);

        // Kerstboom
        keypad.GetEndoint(4).On.Subscribe(_ => entities.Switch.EspBwSpotlivingleftRelay.TurnOn());
        keypad.GetEndoint(4).Off.Subscribe(_ => entities.Switch.EspBwSpotlivingleftRelay.TurnOff());
    }

    private void OnWithStep(LightEntity light) => light.TurnOn(brightness: light.IsOff() ? 128 : 256);

    private void DuoLightCycleOnState(LightEntity rightLight, LightEntity leftLight)
    {
        var rightIsOn = rightLight.IsOn();
        var leftIsOn = leftLight.IsOn();

        // Cycle states on each press 
        var (rightOn, leftOn) = (rightIsOn, leftIsOn) switch
        {
            (false, false) => (true,  true),
            (true,  true ) => (true,  false),
            (true,  false) => (false, true),
            (false, true ) => (true,  true),
        };

        TurnOnOff(rightLight, rightOn);
        TurnOnOff(leftLight, leftOn);
    }

    private static void TurnOnOff(LightEntity @switch, bool newState)
    {
        if (@switch.IsOn() == newState) return;

        if (newState)
            @switch.TurnOn(brightness: 255, transition: 2);
        else
            @switch.TurnOff();
    }

    private void LongPressAdjustBrightness(KeyPad.Endpoint endpoint, LightEntity light)
    {
        endpoint.StartLongPressOn.Subscribe(_ => SlideBrighness(light, 10, endpoint.StopLong));
        endpoint.StartLongPressOff.Subscribe(_ => SlideBrighness(light, -10, endpoint.StopLong));
    }

    private void SlideBrighness(LightEntity light, long delta, IObservable<object> stopSlide)
    {
        var current = (long?)light.EntityState?.Attributes?.Brightness ?? 0;

        var subscribtion = Observable.Interval(TimeSpan.FromMilliseconds(250), _scheduler)
            .TakeWhile(i => AdjustBrightness(light, current + (i + 1) * delta)).Subscribe();

        stopSlide.Take(1).Subscribe(_ => subscribtion.Dispose());
    }

    private bool AdjustBrightness(LightEntity light, long newValue)
    {
        bool result = true;

        if (newValue > 255)
        {
            newValue = 255;
            result = false;
        }

        if (newValue < 10)
        {
            newValue = 10;
            result = false;
        }

        light.TurnOn(brightness: newValue);
        return result;
    }
}