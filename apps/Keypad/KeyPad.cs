using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using daemonapp.apps.Cube;
using NetDaemon.HassModel.Common;

namespace Keypad
{
    public class KeyPad
    {
        private IObservable<ZhaEventData> _keyPadEvents;

        public KeyPad(IHaContext ha, string id)
        {
            _keyPadEvents = ha.Events.Filter<ZhaEventData>("zha_event")
                .Where(e => e.Data?.DeviceIeee == "84:71:27:ff:fe:40:78:7b")
                .Select(e => e.Data!);
        }

        public Endpoint GetEndoint(int endpointId) => new (_keyPadEvents.Where(e => e.EndpointId == endpointId));

        public class Endpoint
        {
            public IObservable<ZhaEventData> AllEvents { get; }

            public Endpoint(IObservable<ZhaEventData> endpointEvents)
            {
                AllEvents = endpointEvents;
            }

            public IObservable<ZhaEventData> On => AllEvents.Where(e => e.Command == "on");
            public IObservable<ZhaEventData> Off => AllEvents.Where(e => e.Command == "off");
            public IObservable<ZhaEventData> StartLongPressOn =>
                AllEvents.Where(e => e.Command == "move_with_on_off" 
                                     && ((JsonElement?)  ((JsonElement) e.args!).EnumerateArray().ElementAtOrDefault(0))?.GetInt32() != 1);
            
            public IObservable<ZhaEventData> StartLongPressOff =>
                AllEvents.Where(e => e.Command == "move_with_on_off" 
                                     && ((JsonElement?)  ((JsonElement) e.args!).EnumerateArray().ElementAtOrDefault(0))?.GetInt32() == 1);            
            
            public IObservable<ZhaEventData> StopLong => AllEvents.Where(e => e.Command == "stop");            
            
        }
    }
}