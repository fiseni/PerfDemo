namespace PerfDemo;

[MemoryDiagnoser]
[ShortRunJob]
public class Benchmark4
{
    private SourceData _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceData.LoadForBenchmark();
    }

    [Benchmark]
    public void Processor4()
    {
        var processor = new Processor4(_sourceData);
        Benchmark.RunFor(processor, _sourceData);
    }
}
