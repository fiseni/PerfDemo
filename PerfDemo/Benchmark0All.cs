using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerfDemo.Processors;

namespace PerfDemo;

[MemoryDiagnoser]
[SimpleJob(runStrategy: RunStrategy.ColdStart, launchCount: 0, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
public class Benchmark0All
{
    private SourceData _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceData.LoadForBenchmark();
    }

    [Benchmark(Baseline = true)]
    public void Processor1()
    {
        var processor = new Processor1(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }

    [Benchmark]
    public void Processor2()
    {
        var processor = new Processor2(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }

    [Benchmark]
    public void Processor3()
    {
        var processor = new Processor3(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }

    [Benchmark]
    public void Processor4()
    {
        var processor = new Processor4(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }
}
