using System.Collections.Concurrent;

namespace PerfDemo.Services;

public class Service5
{
    private readonly MasterPart[] _masterParts;

    public Service5(MasterPart[] masterParts)
    {
        _masterParts = masterParts;
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        partNumber = partNumber.Trim();
        if (partNumber.Length < 3) return null;

        var filteredMasterParts = _masterParts.Where(x =>
            !string.IsNullOrWhiteSpace(x.PartNumber) &&
            x.PartNumber.Length > 2 &&
            x.PartNumberNoHyphens.Length > 2 &&
            (string.Equals(x.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.PartNumberNoHyphens, partNumber, StringComparison.OrdinalIgnoreCase) ||
            x.PartNumber.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase) ||
            x.PartNumberNoHyphens.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase) ||
            partNumber.EndsWith(x.PartNumber, StringComparison.OrdinalIgnoreCase))).ToArray();


        var masterPart = filteredMasterParts.OrderBy(x => x.PartNumber.Length).FirstOrDefault(x => string.Equals(x.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumberNoHyphens.Length).FirstOrDefault(x => string.Equals(x.PartNumberNoHyphens, partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumber.Length).FirstOrDefault(x => x.PartNumber.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumberNoHyphens.Length).FirstOrDefault(x => x.PartNumberNoHyphens.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderByDescending(x => x.PartNumber.Length).FirstOrDefault(x => partNumber.EndsWith(x.PartNumber, StringComparison.OrdinalIgnoreCase));

        return masterPart;
    }
}
