using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerfDemo.Processors;

namespace PerfDemo;

[MemoryDiagnoser]
[SimpleJob(runStrategy: RunStrategy.ColdStart, launchCount: 0, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
public class Benchmark
{
    private Part[] _parts = default!;
    private MasterPart[] _masterParts = default!;

    [GlobalSetup]
    public void Setup()
    {
        // BenchmarkDotNet creates a new folder on each run named with an arbitrary GUID.
        // This is a workaround to get the correct path to the data files. Don't ask :)
        _parts = File.ReadAllLines(Path.Combine("..", "..", "..", "..", "Data", "parts.txt"))
            .Select(x => new Part(x))
            .ToArray();
        _masterParts = File.ReadAllLines(Path.Combine("..", "..", "..", "..", "Data", "masterParts.txt"))
            .Select(x => new MasterPart(x))
            .ToArray();

        Console.WriteLine("### Setup completed!");
    }

    [Benchmark(Baseline = true)]
    public void Processor1()
    {
        var matchCount = 0;
        var service = new Processor1(_masterParts);

        for (var i = 0; i < _parts.Length; i++)
        {
            var match = service.FindMatchedPart(_parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }
        Console.WriteLine($"### {nameof(Processor1)}. Found {matchCount:n0} matches!");
    }

    [Benchmark]
    public void Processor2()
    {
        var matchCount = 0;
        var service = new Processor2(_masterParts);

        for (var i = 0; i < _parts.Length; i++)
        {
            var match = service.FindMatchedPart(_parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }
        Console.WriteLine($"### {nameof(Processor2)}.  Found {matchCount:n0} matches!");
    }

    [Benchmark]
    public void Processor3()
    {
        var matchCount = 0;
        var service = new Processor3(_masterParts, _parts);

        for (var i = 0; i < _parts.Length; i++)
        {
            var match = service.FindMatchedPart(_parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }
        Console.WriteLine($"### {nameof(Processor3)}.  Found {matchCount:n0} matches!");
    }

    [Benchmark]
    public void Processor4()
    {
        var matchCount = 0;
        var service = new Processor4(_masterParts, _parts);

        for (var i = 0; i < _parts.Length; i++)
        {
            var match = service.FindMatchedPart(_parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }
        Console.WriteLine($"### {nameof(Processor4)}.  Found {matchCount:n0} matches!");
    }
}
