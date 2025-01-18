namespace PerfDemo;

public class SourceData
{
    public MasterPart[] MasterParts { get; }
    public Part[] Parts { get; }

    public SourceData(MasterPart[] masterParts, Part[] parts)
    {
        MasterParts = masterParts;
        Parts = parts;
    }

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
