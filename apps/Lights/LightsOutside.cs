[NetDaemonApp]
public class LightsOutside
{
    public LightsOutside(Entities entities)
    {
        var lampVoordeur = entities.Light.LampVoordeurLevelLightColorOnOff;

        entities.Sun.Sun.StateChanges()
            .Subscribe(e => lampVoordeur.SwitchTo(e.New?.State == "below_horizon" ? "on" : "off"));
    }
}