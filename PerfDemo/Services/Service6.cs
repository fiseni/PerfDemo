using System.Collections.Concurrent;

namespace PerfDemo.Services;

public class Service6
{
    private readonly MasterPart[] _masterParts;

    public Service6(MasterPart[] masterParts)
    {
        _masterParts = masterParts;
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        partNumber = partNumber.Trim();
        if (partNumber.Length < 3) return null;

        var filteredMasterParts = new List<MasterPart>();

        foreach (var x in _masterParts)
        {
            if (string.IsNullOrWhiteSpace(x.PartNumber)) continue;
            if (x.PartNumber.Length < 3) continue;
            if (x.PartNumberNoHyphens.Length < 3) continue;
            
            if (string.Equals(x.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase))
            {
                return x; 
            }

            if (string.Equals(x.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase) ||
            x.PartNumber.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase) ||
            x.PartNumberNoHyphens.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase) ||
            partNumber.EndsWith(x.PartNumber, StringComparison.OrdinalIgnoreCase))
            {
                filteredMasterParts.Add(x);
                continue;
            }
        }

        var masterPart = filteredMasterParts.OrderBy(x => x.PartNumber.Length).FirstOrDefault(x => string.Equals(x.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumberNoHyphens.Length).FirstOrDefault(x => string.Equals(x.PartNumberNoHyphens, partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumber.Length).FirstOrDefault(x => x.PartNumber.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderBy(x => x.PartNumberNoHyphens.Length).FirstOrDefault(x => x.PartNumberNoHyphens.EndsWith(partNumber, StringComparison.OrdinalIgnoreCase));
        masterPart ??= filteredMasterParts.OrderByDescending(x => x.PartNumber.Length).FirstOrDefault(x => partNumber.EndsWith(x.PartNumber, StringComparison.OrdinalIgnoreCase));

        return masterPart;
    }
}
