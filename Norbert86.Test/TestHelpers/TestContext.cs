using System.Reactive.Concurrency;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel;
using Norbert.Apps.Helpers;

namespace Norbert86.Test.TestHelpers;

public class TestContext : IServiceProvider
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private readonly IServiceProvider _serviceProvider;

    public TestContext()
    {
        _serviceCollection.AddHomeAssistantGenerated();
        _serviceCollection.AddNorbert86Services();
        
        _serviceCollection.AddSingleton(_ => new HaContextMock());
        _serviceCollection.AddTransient<IHaContext>(s => s.GetRequiredService<HaContextMock>().Object);
        _serviceCollection.AddSingleton<TestScheduler>();
        _serviceCollection.AddTransient<IScheduler>(s => s.GetRequiredService<TestScheduler>());
        _serviceCollection.AddTransient(typeof(ILogger<>), typeof(NullLogger<>));
        _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public T GetApp<T>() => ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider);
    
    public Entities Entities => this.GetRequiredService<Entities>();
    public HaContextMock HaMock => this.GetRequiredService<HaContextMock>();

}