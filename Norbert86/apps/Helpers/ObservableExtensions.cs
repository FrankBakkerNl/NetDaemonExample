using System.Globalization;
using System.Security.Cryptography;
using MathNet.Numerics.Statistics;

public static class ObservableExtensions
{
    public static IObservable<NumericStateChange<TEntity, TEntityState>> NotNull<TEntity, TEntityState>(this IObservable<NumericStateChange<TEntity, TEntityState>> source)
        where TEntityState : NumericEntityState<NumericSensorAttributes>
        where TEntity : Entity
    {
        return source.Where(x => x.New?.State != null);
    }

    public static IObservable<(TSource? Previous, TSource? Current)> PairWithPrevious<TSource>(this IObservable<TSource> source)
    {
        return source.Scan((default(TSource), default(TSource)), (acc, current) => (acc.Item2, current));
    }

    public static NumericEntityState<NumericSensorAttributes> DefaultIfNull(this NumericEntityState<NumericSensorAttributes>? state, double defaultValue)
    {
        return state ?? new NumericEntityState<NumericSensorAttributes>(new EntityState { State = defaultValue.ToString(CultureInfo.InvariantCulture) });
    }

    public static IObservable<StateChange<TEntity, TState>> StateChangesWithCurrent<TEntity, TState, TAttributes>(this TEntity entity)
        where TEntity : Entity<TEntity, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        return entity.StateAllChanges().Prepend(new StateChange<TEntity, TState>(entity, null, entity.EntityState));
    }
    



    public static IObservable<StateChange<NumericSensorEntity, NumericEntityState<NumericSensorAttributes>>> StateChangesWithCurrent(this NumericSensorEntity entity)
    {
        return StateChangesWithCurrent<NumericSensorEntity, NumericEntityState<NumericSensorAttributes>, NumericSensorAttributes>(entity);
    }

    public static IObservable<double> MovingAverage(this IObservable<double> source, int windowSize)
    {
        return source.MovingAverage(x => x, windowSize);
    }

    public static IObservable<double> MovingAverage(this IObservable<NumericStateChange<NumericSensorEntity, NumericEntityState<NumericSensorAttributes>>> source, int windowSize)
    {
        return source.Where(x => x.New?.State != null).MovingAverage(x => x.New!.State.GetValueOrDefault(), windowSize);
    }

    /// <summary>
    /// Calculates a running moving average of a sequence of values.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="windowSize"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IObservable<double> MovingAverage<TSource>(this IObservable<TSource> source, Func<TSource, double> selector, int windowSize)
    {
        var statistics = new MovingStatistics(windowSize);
        return source.Select(x =>
        {
            var value = selector(x);
            statistics.Push(value);
            return statistics.Mean;
        });
    }

    public static IObservable<MovingStatistics> MovingStatistics<TSource>(this IObservable<TSource> source, Func<TSource, double> selector, int windowSize)
    {
        var statistics = new MovingStatistics(windowSize);
        return source.Select(x =>
        {
            var value = selector(x);
            statistics.Push(value);
            return statistics;
        });
    }
    
    /// <summary>
    /// Will let a value through if <paramref name="trigger"/> returns true and will then block values until <paramref name="reset"/> returns true.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="trigger"></param>
    /// <param name="reset"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IObservable<TSource> TriggerAndWait<TSource>(this IObservable<TSource> source, Func<TSource, bool> trigger, Func<TSource, bool> reset)
    {
        ArgumentNullException.ThrowIfNull(source);

        bool triggered = false;

        return source.Where(x =>
        {
            if (trigger(x))
            {
                if (!triggered)
                {
                    triggered = true;
                    return true;
                }
            }
            else if (reset(x))
            {
                triggered = false;
            }

            return false;
        });
    }
}
