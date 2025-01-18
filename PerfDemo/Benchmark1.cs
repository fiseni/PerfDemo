using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerfDemo.Processors;

namespace PerfDemo;

[MemoryDiagnoser]
[SimpleJob(runStrategy: RunStrategy.ColdStart, launchCount: 0, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
public class Benchmark1
{
    private SourceData _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceData.LoadForBenchmark();
    }

    [Benchmark]
    public void Processor1()
    {
        var processor = new Processor1(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }
}
