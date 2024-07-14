namespace Norbert86.apps.Helpers;

public class KeukenRtttl(EsphomeServices esphomeServices)
{
    public void KnightRider() => esphomeServices.EspKitchenPanelPlayRtttl("Knight Rider:o=5,d=32,b=63,b=63:16e,f,e,8b,16e6,f6,e6,8b");
    public void Notify(string label, string message, string rtttlSound)
    {
        esphomeServices.EspKitchenPanelPlayRtttl(rtttlSound);
        esphomeServices.EspKitchenPanelNotificationShow(label, message);
    }
}