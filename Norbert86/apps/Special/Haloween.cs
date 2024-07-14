using System.Threading;

[NetDaemonApp]
public sealed class Halloween : IDisposable
{
    private readonly LightEntities _lights;
    private readonly SwitchEntities _switchEntities;
    private readonly ILogger<Halloween> _logger;
    private bool animateLedsOn;               
    private Random _random = new();    

    private readonly CancellationTokenSource _disposedAppTokenSource = new();
    
    public Halloween(LightEntities lights, SwitchEntities switchEntities, BinarySensorEntities binarySensors, ILogger<Halloween> logger)
    {
        _lights = lights;
        _switchEntities = switchEntities;
        _logger = logger;

        Task.Run(AnimateVoordeur);

        binarySensors.Motionsensor01.StateChanges().Where(e => e.New.IsOn()).SubscribeAsync(_ => AnimateLeds());
        binarySensors.Motionsensor01.StateChanges().Where(e => e.New.IsOff()).Subscribe(_ => animateLedsOn = false);
    }

    private async Task AnimateVoordeur()
    {
        while (!_disposedAppTokenSource.Token.IsCancellationRequested)
        {
            _lights.LampVoordeurLevelLightColorOnOff.TurnOn(transition: 1, brightness: 200, colorTemp: 500);
            await Task.Delay(2000);
            _lights.LampVoordeurLevelLightColorOnOff.TurnOn(transition: 2, brightness: 50, colorTemp: 500);
            await Task.Delay(3000);
        }
    }

    private async Task AnimateLeds()
    {
        animateLedsOn = true;
        while (!_disposedAppTokenSource.Token.IsCancellationRequested && animateLedsOn)
        {
            var entity = _switchEntities.GarageLedRelay;
            var random = new Random();
            var times = random.Next(4, 8);
            for (int i = 0; i < times && animateLedsOn; i++)
            {
                entity.TurnOn();
                await Task.Delay(500);

                entity.TurnOff();
                await Task.Delay(1000);
            }

            await Task.Delay(_random.Next(5000, 10000));
        }
    }

    public void Dispose() => _disposedAppTokenSource.Cancel();
}