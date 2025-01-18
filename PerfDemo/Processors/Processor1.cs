namespace PerfDemo.Processors;

public class Processor1
{
    private readonly MasterPart[] _masterParts;

    public Processor1(MasterPart[] masterParts)
    {
        _masterParts = masterParts;
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        partNumber = partNumber.Trim();
        if (partNumber.Length < 3) return null;

        partNumber = partNumber.ToUpper();

        var masterPart = _masterParts.FirstOrDefault(x => x.PartNumber.EndsWith(partNumber));
        masterPart ??= _masterParts.FirstOrDefault(x => x.PartNumberNoHyphens.EndsWith(partNumber));
        masterPart ??= _masterParts.FirstOrDefault(x => partNumber.EndsWith(x.PartNumber));

        return masterPart;
    }
}
