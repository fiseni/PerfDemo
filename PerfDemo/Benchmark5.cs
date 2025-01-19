namespace PerfDemo;

[MemoryDiagnoser]
[ShortRunJob]
public class Benchmark5
{
    private SourceData _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceData.LoadForBenchmark();
    }

    [Benchmark]
    public void Processor5()
    {
        var processor = new Processor5(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }
}
