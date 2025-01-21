using System.Text;

namespace PerfDemo;

[MemoryDiagnoser]
[ShortRunJob]
public class Benchmark6
{
    private SourceData6 _sourceData = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sourceData = SourceData6.LoadForBenchmark();
    }

    [Benchmark]
    public void Processor6()
    {
        var processor = new Processor6(_sourceData);
        RunFor(processor, _sourceData, true);
    }

    public static void RunFor(Processor6 processor, SourceData6 sourceData, bool printResults = false)
    {
        var matchCount = 0;

        var parts = sourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
            }
        }

        if (printResults)
        {
            Console.WriteLine($"### {processor.Identifier}. Found {matchCount:n0} matches!");
        }
    }

    public static void RunAndDumpFor(Processor6 processor, SourceData6 sourceData, string filePath)
    {
        List<string> result = new List<string>();

        var parts = sourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
            if (match is not null)
            {
                result.Add(Encoding.UTF8.GetString(match.Value.PartNumber.Span));
            }
        }
        File.WriteAllLines(filePath, result);
    }
}
