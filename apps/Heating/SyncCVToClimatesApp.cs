namespace Heating;

[NetDaemonApp]
public class SyncCVToClimates : IInitializable
{
    public SwitchEntity? HeaterSwitch { get; init; }

    public IEnumerable<ClimateEntity>? Climates { get; init; }

    public void Initialize()
    {
        Climates?.StateAllChanges().Subscribe(_ => SetCvState());
        HeaterSwitch?.StateAllChanges().Subscribe(_ => SetCvState());
    }

    private void SetCvState()
    {
        var anyHeating = Climates?.Any(NeedsHeat) ?? false;
        HeaterSwitch?.SwitchTo(anyHeating ? "on" : "off");
    }
        
    private bool NeedsHeat(ClimateEntity climate) => 
        climate.EntityState?.Attributes?.HvacAction == "heating" && 
        climate.EntityState?.Attributes?.Temperature > climate.EntityState?.Attributes?.CurrentTemperature;
}