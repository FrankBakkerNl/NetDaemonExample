using System.Reflection;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using Norbert86.Test.TestHelpers;

namespace Norbert86.Test;

public class AppValidation
{
    [Fact]
    public void TestNoFocusAttribute()
    {
        var appsWithFocus = typeof(Entities).Assembly.GetTypes().Where(t => t.GetCustomAttribute<FocusAttribute>() != null);
        appsWithFocus.Should().BeEmpty();
    }

    [Fact]
    public void LoadAllApps()
    {
        var apps = typeof(OpenWindowClimateOff).Assembly.GetTypes().Where(t => t.GetCustomAttribute<NetDaemonAppAttribute>() != null);
        var tcx = new TestContext();
        
        foreach (var type in apps)
        {
            ActivatorUtilities.GetServiceOrCreateInstance(tcx, type);
        }
    }
}