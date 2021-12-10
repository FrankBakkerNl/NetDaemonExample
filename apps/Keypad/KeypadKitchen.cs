using System.Reactive.Concurrency;
using Keypad;

[NetDaemonApp]
public class KeypadKitchen
{
    private readonly IScheduler _scheduler;

    private readonly Entities _myEntities;

    public KeypadKitchen(IHaContext ha, IScheduler? scheduler = null)
    {
        _myEntities = ha.MyEntities();
        _scheduler = scheduler ?? TaskPoolScheduler.Default;

        var keypad = new KeyPad(ha, "84:71:27:ff:fe:40:78:7b");

        // Buttons1 => keuken
        var endpoint1 = keypad.GetEndoint(1);
        endpoint1.On.Subscribe(_ => _myEntities.Switch.EspBwKitchen2Relay.TurnOn());
        endpoint1.Off.Subscribe(_ => _myEntities.Switch.EspBwKitchen2Relay.TurnOff());

        // Buttons 2 => Eettafel
        var endpoint2 = keypad.GetEndoint(2);
        endpoint2.On.Subscribe(_ => OnWithStep(_myEntities.Light.LampenEettafel));
        endpoint2.Off.Subscribe(_ => _myEntities.Light.LampenEettafel.TurnOff());
        LongPressAdjustBrightness(endpoint2, _myEntities.Light.LampenEettafel);

        // Buttons 3 => spots
        var endpoint3 = keypad.GetEndoint(3);
        endpoint3.On.Subscribe(_ => DuoLightCycleOnState(_myEntities.Light.SpotsWoonkamerRechts,
            _myEntities.Light.SpotsWoonkamerLinks));

        endpoint3.Off.Subscribe(_ => _myEntities.Light.SpotsWoonkamerLinks.TurnOff());
        endpoint3.Off.Subscribe(_ => _myEntities.Light.SpotsWoonkamerRechts.TurnOff());

        LongPressAdjustBrightness(endpoint3, _myEntities.Light.SpotsWoonkamerLinks);
        LongPressAdjustBrightness(endpoint3, _myEntities.Light.SpotsWoonkamerRechts);
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
        endpoint.StartLongPressOn.Subscribe(_ => SlideBrighness(light, 20, endpoint.StopLong));
        endpoint.StartLongPressOff.Subscribe(_ => SlideBrighness(light, -20, endpoint.StopLong));
    }

    private void SlideBrighness(LightEntity light, long delta, IObservable<object> stopSlide)
    {
        var current = (long?)light.EntityState?.Attributes?.Brightness ?? 0;

        var subscribtion = Observable.Interval(TimeSpan.FromMilliseconds(200), _scheduler)
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

        if (newValue < 1)
        {
            newValue = 1;
            result = false;
        }

        light.TurnOn(brightness: newValue);
        return result;
    }
}