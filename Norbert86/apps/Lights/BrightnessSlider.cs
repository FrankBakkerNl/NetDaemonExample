public class BrightnessSlider
{
    private readonly IScheduler _scheduler;

    public BrightnessSlider(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    
    public IDisposable StartSliding(LightEntity light, TimeSpan interval, long delta)
    {
        var current = light.EntityState?.Attributes?.Brightness ?? 0;

        return Observable.Interval(interval, _scheduler)
            .TakeWhile(i => AdjustBrightness(light, current + (i + 1) * delta)).Subscribe();
    }

    private static bool AdjustBrightness(LightEntity light, long newValue)
    {
        var clamped = Math.Clamp(newValue, 0, 255);

        light.TurnOn(brightness: clamped, transition:0);
        return clamped == newValue;
    }
}