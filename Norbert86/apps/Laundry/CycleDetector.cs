using System.Reactive;
using MathNet.Numerics.Statistics;

namespace HomeAssistantGenerated.apps.Laundry;

public enum CycleState
{
    Off,
    Running,
    Ready
}

public static class CycleDetector
{
    public static IObservable<CycleState> WasherStates(IObservable<double> powerSensorData, IScheduler scheduler)
    {
        return powerSensorData
            .Timestamp(scheduler)
            .KeepAlive(scheduler)
            .Scan((default(Timestamped<double>), stateHandler: StateHandler.Start), (s, i) => (i, s.stateHandler.ProcessValue(i)))
            .Select(i => i.stateHandler.State)
            .DistinctUntilChanged();
    }
    
    // Sometimes, when no power is used the device sends a single 0 value and no other values after that
    // To simplify handling this we repeat the most recent value until we get a new one    
    static IObservable<Timestamped<T>> KeepAlive<T>(this IObservable<Timestamped<T>> powerSensorData, IScheduler scheduler)
    {
        var heartbeat =  Observable.Interval(TimeSpan.FromSeconds(2), scheduler).Timestamp(scheduler);

        return powerSensorData
            .CombineLatest(heartbeat)
            .PairWithPrevious()
            // We want the normal event if there was a newer input value, otherwise this was triggered by the heartbeat and we repeat the most recent input 
            .Where(e => e.Current.First.Timestamp > e.Previous.First.Timestamp || e.Current.Second.Timestamp > e.Current.First.Timestamp.AddSeconds(3))
            .Select(e => Timestamped.Create(e.Current.First.Value, e.Current.Second.Timestamp));
    }

    public abstract class StateHandler(CycleState cycleState)
    {
        public static StateHandler Start => new StateOff();

        public CycleState State { get; } = cycleState;

        public abstract StateHandler ProcessValue(Timestamped<double> timeSeries);
    }

    class StateRunning() : StateHandler(CycleState.Running)
    {
        private readonly MovingStatistics _stats = new(10);
        private readonly MovingStatistics _stats2 = new(60);

        public override StateHandler ProcessValue(Timestamped<double> timeSeries)
        {
            _stats.Push(timeSeries.Value);
            _stats2.Push(_stats.Mean);

            return _stats2.Count == _stats2.WindowSize && _stats2.Maximum < 40 ? new StateReady() : this;
        }
    }

    class StateReady() : StateHandler(CycleState.Ready)
    {
        public override StateHandler ProcessValue(Timestamped<double> timeSeries) => timeSeries.Value switch
        {
            > 100 => new StateRunning(),
            < 2 => new StateOff(),
            _ => this
        };
    }

    class StateOff() : StateHandler(CycleState.Off)
    {
        public override StateHandler ProcessValue(Timestamped<double> timeSeries) =>
            timeSeries.Value > 100 ? new StateRunning() : this;
    }
}