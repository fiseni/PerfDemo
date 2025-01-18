using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerfDemo.Processors;

namespace PerfDemo;

[MemoryDiagnoser]
[SimpleJob(runStrategy: RunStrategy.ColdStart, launchCount: 0, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
public class Benchmark
{
    // BenchmarkDotNet creates a new folder on each run named with an arbitrary GUID.
    // This is a workaround to get the correct path to the data files. Don't ask :)
    public static string MasterPartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "masterParts.txt");
    public static string PartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "parts.txt");

    public SourceData SourceData { get; set; } = default!;

    [GlobalSetup]
    public void Setup()
    {
        SourceData = SourceData.Load(MasterPartsFilePath, PartsFilePath);
    }

    [Benchmark(Baseline = true)]
    public void Processor1()
    {
        var matchCount = 0;
        var processor = new Processor1(SourceData);

        var parts = SourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
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
        var processor = new Processor2(SourceData);

        var parts = SourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
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
        var processor = new Processor3(SourceData);

        var parts = SourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
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
        var processor = new Processor4(SourceData);

        var parts = SourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }
        Console.WriteLine($"### {nameof(Processor4)}.  Found {matchCount:n0} matches!");
    }
}
