[NetDaemonApp]
public class MagicCube
{
    private MediaPlayerEntity _mediaPlayerKeuken;
    private SwitchEntity _keukenlamp;

    public MagicCube(IHaContext ha)
    {
        _mediaPlayerKeuken = ha.MyEntities().MediaPlayer.Keuken;
        _keukenlamp = ha.MyEntities().Switch.EspBwKitchen2Relay;

        ha.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => e.Data?.DeviceIeee == "00:15:8d:00:05:d9:d0:37")
            .Subscribe(e => CubeEvent(e.Data!));
    }

    private void CubeEvent(ZhaEventData eventData)
    {
        switch (eventData.Command)
        {
            case "shake":
                _mediaPlayerKeuken.MediaPlay();
                _mediaPlayerKeuken.VolumeMute(isVolumeMuted: false);
                break;

            case "drop":
                _mediaPlayerKeuken.MediaPause();
                break;

            case "rotate_left":
                _mediaPlayerKeuken.VolumeDown();
                break;

            case "rotate_right":
                _mediaPlayerKeuken.VolumeUp();
                break;
                
            case "flip":
                _keukenlamp.Toggle();
                break;
        }
    }
}