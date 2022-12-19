using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;

namespace Norbert86.Test.TestHelpers;

public class TestBase
{
    public TestContext Context = new ();
    public Entities Entities => Context.GetRequiredService<Entities>();
    public HaContextMock HaMock => Context.GetRequiredService<HaContextMock>();
    
}