using System.Globalization;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using FluentAssertions.Extensions;
using HomeAssistantGenerated.apps.Laundry;
using Microsoft.Reactive.Testing;

namespace Norbert86.Test.CycleDetection;



public class TestCycleDetection
{
    [Fact]
    public void TestWasherDefault()
    {
        ValidateWasherStates("WasherCycle.csv",
        [
            (new DateTime(2024, 5, 17, 15, 30, 48), 3, CycleState.Running),
            (new DateTime(2024, 5, 17, 15, 50, 28), 1, CycleState.Ready)
        ]);
    }
    
    [Fact]
    public void Test60Graden()
    {
        ValidateWasherStates("Washer60.csv",
            [
                (new DateTime(2024, 5, 20, 09, 03, 0), 4, CycleState.Running),
                (new DateTime(2024, 5, 20, 10, 58, 0), 2, CycleState.Ready),
                (new DateTime(2024, 5, 20, 11, 37, 0), 2, CycleState.Off),
            ]);
    }
    
    private void ValidateWasherStates(string filename, (DateTime timestamp, int toleranceMinutes, CycleState state)[] expectedStates)
    {
        var lines = File.ReadLines(Path.Combine(@"CycleDetection\samplefiles", filename));
        var data = lines.Skip(1).Select(ParseLine).ToList();

        var subject = new Subject<double>();
        var testScheduler = new TestScheduler();

        var results = new List<Timestamped<CycleState>>();
        testScheduler.AdvanceTo(data.First().Timestamp.Ticks);
        
        CycleDetector.WasherStates(subject, testScheduler).Timestamp(testScheduler).Subscribe(e => results.Add(e));

        // now push all values from the file to the subject
        foreach (var pair in data)
        {
            testScheduler.AdvanceTo(pair.Timestamp.Ticks);
            subject.OnNext(pair.Value);
        }

        results.Should().HaveCount(expectedStates.Length + 1);

        int index = 1;
        foreach (var (timestamp, toleranceMinutes, state) in expectedStates)
        {
            results[index].Value.Should().Be(state);
            results[index].Timestamp.Should().BeOnOrAfter(timestamp.AsUtc());
            results[index].Timestamp.Should().BeOnOrBefore(timestamp.AsUtc().Add(TimeSpan.FromMinutes(toleranceMinutes)));
            index++;
        }
    }
    

    private Timestamped<double> ParseLine(string line)
    {
        var parts = line.Split(",");
        return Timestamped.Create(parts[1] is "" ? 0 : double.Parse(parts[1], CultureInfo.InvariantCulture), DateTimeOffset.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal));
    }
}