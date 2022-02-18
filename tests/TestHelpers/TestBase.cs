using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;

namespace daemonapp_test.Heating;

public class TestBase
{
    public TestContext Context = new ();
    public Entities Entities => Context.GetRequiredService<Entities>();
    public HaContextMock HaMock => Context.GetRequiredService<HaContextMock>();
    
}