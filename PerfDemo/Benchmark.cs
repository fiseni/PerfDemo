using System.Text;

namespace PerfDemo;

public class Benchmark
{
    public static void RunFor(IProcessor processor, SourceData sourceData, bool printResults = false)
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

    public static void RunFor(Processor6 processor, SourceDataX sourceData, bool printResults = false)
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

    public static void RunAndDumpFor(IProcessor processor, SourceData sourceData, bool printResults = false)
    {
        List<string> result = new List<string>();
        var matchCount = 0;

        var parts = sourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
                result.Add(match.PartNumber);
            }
        }

        if (printResults)
        {
            Console.WriteLine($"### {processor.Identifier}. Found {matchCount:n0} matches!");
        }
        File.WriteAllLines("result1.txt", result);
    }

    public static void RunAndDumpFor(Processor6 processor, SourceDataX sourceData, bool printResults = false)
    {
        List<string> result = new List<string>();
        var matchCount = 0;

        var parts = sourceData.Parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var match = processor.FindMatchedPart(parts[i].PartNumber);
            if (match is not null)
            {
                matchCount++;
                result.Add(Encoding.UTF8.GetString(match.Value.PartNumber.Span));
            }
        }

        if (printResults)
        {
            Console.WriteLine($"### {processor.Identifier}. Found {matchCount:n0} matches!");
        }
        File.WriteAllLines("result2.txt", result);
    }
}
