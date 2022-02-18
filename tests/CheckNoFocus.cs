using System.Linq;
using System.Reflection;
using FluentAssertions;
using HomeAssistantGenerated;
using NetDaemon.AppModel;
using Xunit;

namespace daemonapp_test;

public class CheckNoFocus
{
    [Fact]
    public void TestNoFocusAttribute()
    {
        var appsWithFocus = typeof(Entities).Assembly.GetTypes().Where(t => t.GetCustomAttribute<FocusAttribute>() != null);
        appsWithFocus.Should().BeEmpty();
    }
}