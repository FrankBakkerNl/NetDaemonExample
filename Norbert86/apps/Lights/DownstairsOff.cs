[NetDaemonApp]
public class DownstairsOff
{
    public DownstairsOff(Entities entities)
    {
        entities.InputButton.BenedenUit.StateAllChanges().Subscribe(_ =>
        {
            entities.Light.AllesBeneden.TurnOff(transition:5);
            entities.MediaPlayer.Keuken.MediaStop();            
        });
    }
}