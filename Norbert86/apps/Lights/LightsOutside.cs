[NetDaemonApp]
public class LightsOutside
{
    public LightsOutside(LightEntities lights, SunEntity sun)
    {
        sun.StateChanges()
            .Subscribe(e => lights.LampVoordeurLevelLightColorOnOff.SwitchTo(e.New?.State == "below_horizon" ? "on" : "off"));
    }
}