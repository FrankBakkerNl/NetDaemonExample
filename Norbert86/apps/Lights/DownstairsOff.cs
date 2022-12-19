[NetDaemonApp]
public class DownstairsOff
{
    public DownstairsOff(Entities entities)
    {
        entities.InputButton.BenedenUit.StateAllChanges().Subscribe(_ =>
        {
            entities.Light.AlleSpotsWoonkamer.TurnOff(transition:5);
            entities.Light.LampenEettafel.TurnOff(transition:5);
            entities.MediaPlayer.Keuken.TurnOff();
        });
    }
}