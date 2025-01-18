namespace PerfDemo;

public class SourceData
{
    // BenchmarkDotNet creates a new folder on each run named with an arbitrary GUID.
    // This is a workaround to get the correct path to the data files. Don't ask :)
    public static string MasterPartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "masterParts.txt");
    public static string PartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "parts.txt");

    public MasterPart[] MasterParts { get; }
    public Part[] Parts { get; }

    public SourceData(MasterPart[] masterParts, Part[] parts)
    {
        MasterParts = masterParts;
        Parts = parts;
    }

    public static SourceData LoadForBenchmark()
        => Load(MasterPartsFilePath, PartsFilePath);

    public static SourceData Load(string masterPartsPath, string partsPath)
    {
        var masterParts = File.ReadAllLines(masterPartsPath)
            .Select(x => new MasterPart(x))
            .ToArray();

        var parts = File.ReadAllLines(partsPath)
            .Select(x => new Part(x))
            .ToArray();


        return new SourceData(masterParts, parts);
    }
}
