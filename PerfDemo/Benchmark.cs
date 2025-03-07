﻿namespace PerfDemo;

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
}
