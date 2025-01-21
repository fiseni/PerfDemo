namespace PerfDemo;

[MemoryDiagnoser]
[ShortRunJob]
public class Benchmark6
{
    private SourceDataX _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceDataX.LoadForBenchmark();
    }

    [Benchmark]
    public void Processor6()
    {
        var processor = new Processor6(_sourceData);
        Benchmark.RunFor(processor, _sourceData, true);
    }
}
