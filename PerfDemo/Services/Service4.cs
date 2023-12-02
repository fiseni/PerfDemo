using System.Runtime.CompilerServices;

namespace PerfDemo.Services;

public class Service4
{
    private readonly Dictionary<string, MasterPart?> _masterPartsByPartNumber;

    public Service4(MasterPart[] masterParts, Part[] parts)
    {
        var masterPartsInfo = new MasterPartsInfo(masterParts);
        var partsInfo = new PartsInfo(parts);

        _masterPartsByPartNumber = BuildDictionary(masterPartsInfo, partsInfo);
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        if (partNumber.Length < 3) return null;

        partNumber = partNumber.Trim().ToUpper();

        return _masterPartsByPartNumber.GetValueOrDefault(partNumber);
    }

    private static Dictionary<string, MasterPart?> BuildDictionary(MasterPartsInfo masterPartsInfo, PartsInfo partsInfo)
    {
        var masterPartsByPartNumber = new Dictionary<string, MasterPart?>();

        for (var i = 0; i < partsInfo.PartNumbers.Length; i++)
        {
            var partNumber = partsInfo.PartNumbers[i];
            var match = FindMatchForPartNumber(partNumber, masterPartsInfo.SuffixesByLength);
            match ??= FindMatchForPartNumber(partNumber, masterPartsInfo.SuffixesByNoHyphensLength);

            if (match is not null)
            {
                masterPartsByPartNumber.TryAdd(partNumber, match);
            }
        }

        for (var i = masterPartsInfo.MasterPartNumbers.Length - 1; i >= 0; i--)
        {
            var masterPart = masterPartsInfo.MasterPartNumbers[i];

            if (partsInfo.SuffixesByLength.TryGetValue(masterPart.PartNumber.Length, out var suffixDictionary)
                && suffixDictionary is not null)
            {
                if (suffixDictionary.TryGetValue(masterPart.PartNumber, out var originalParts) && originalParts is not null)
                {
                    for (var j = originalParts.Count - 1; j >= 0; j--)
                    {
                        masterPartsByPartNumber.TryAdd(originalParts[j], masterPart);
                    }
                }
            }
        }

        return masterPartsByPartNumber;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MasterPart? FindMatchForPartNumber(
        ReadOnlySpan<char> partNumber,
        Dictionary<int, Dictionary<string, MasterPart>> suffixByLength)
    {
        if (suffixByLength.TryGetValue(partNumber.Length, out var masterPartBySuffix) && masterPartBySuffix != null)
        {
            masterPartBySuffix.TryGetValue(partNumber.ToString(), out var match);
            return match;
        }

        return null;
    }

    private sealed class MasterPartsInfo
    {
        public MasterPart[] MasterPartNumbers { get; private set; } = default!;
        public MasterPart[] MasterPartNumbersNoHyphens { get; private set; } = default!;

        public Dictionary<int, Dictionary<string, MasterPart>> SuffixesByLength { get; private set; } = default!;
        public Dictionary<int, Dictionary<string, MasterPart>> SuffixesByNoHyphensLength { get; private set; } = default!;

        public MasterPartsInfo(MasterPart[] masterParts)
        {
            MasterPartNumbers = masterParts
                .Where(x => x.PartNumber.Length > 2)
                .OrderBy(x => x.PartNumber.Length)
                .DistinctBy(x => x.PartNumber)
                .ToArray();

            MasterPartNumbersNoHyphens = masterParts
                .Where(x => x.PartNumberNoHyphens.Length > 2)
                .OrderBy(x => x.PartNumberNoHyphens.Length)
                .DistinctBy(x => x.PartNumberNoHyphens)
                .ToArray();

            SuffixesByLength = GenerateDictionary(MasterPartNumbers, false);
            SuffixesByNoHyphensLength = GenerateDictionary(MasterPartNumbersNoHyphens, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<int, Dictionary<string, MasterPart>> GenerateDictionary(MasterPart[] masterPartNumbers, bool useNoHyphen)
        {
            var suffixesByLength = new Dictionary<int, Dictionary<string, MasterPart>>(51);
            var startIndexByLength = GenerateStartIndexesByLengthDictionary(masterPartNumbers, useNoHyphen);

            for (var length = 3; length <= 50; length++)
            {
                var tempDictionary = new Dictionary<string, MasterPart>();

                if (startIndexByLength.TryGetValue(length, out var startIndex) && startIndex is not null)
                {
                    for (var i = startIndex.Value; i < masterPartNumbers.Length; i++)
                    {
                        var suffix = useNoHyphen
                            ? masterPartNumbers[i].PartNumberNoHyphens[^length..]
                            : masterPartNumbers[i].PartNumber[^length..];

                        tempDictionary.TryAdd(suffix, masterPartNumbers[i]);
                    }
                }

                suffixesByLength[length] = tempDictionary;
            }

            return suffixesByLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<int, int?> GenerateStartIndexesByLengthDictionary(MasterPart[] masterPartNumbers, bool useNoHyphen)
        {
            var startIndexesByLength = new Dictionary<int, int?>(51);

            for (var i = 0; i <= 50; i++)
            {
                startIndexesByLength[i] = null;
            }

            for (var i = 0; i < masterPartNumbers.Length; i++)
            {
                if (useNoHyphen)
                {
                    var length = masterPartNumbers[i].PartNumberNoHyphens.Length;
                    if (startIndexesByLength[length] is null)
                        startIndexesByLength[length] = i;
                }
                else
                {
                    var length = masterPartNumbers[i].PartNumber.Length;
                    if (startIndexesByLength[length] is null)
                        startIndexesByLength[length] = i;

                }
            }

            BackwardFill(startIndexesByLength);

            return startIndexesByLength;
        }
    }

    private sealed class PartsInfo
    {
        public string[] PartNumbers { get; private set; } = default!;
        public Dictionary<int, Dictionary<string, List<string>>> SuffixesByLength { get; private set; } = default!;

        public PartsInfo(Part[] parts)
        {
            PartNumbers = parts
                .Select(x => x.PartNumber.Trim().ToUpper())
                .Where(x => x.Length > 2)
                .OrderBy(x => x.Length)
                .Distinct()
                .ToArray();

            SuffixesByLength = GenerateDictionary(PartNumbers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<int, Dictionary<string, List<string>>> GenerateDictionary(string[] partNumbers)
        {
            var suffixesByLength = new Dictionary<int, Dictionary<string, List<string>>>(51);
            var startIndexesByLength = new Dictionary<int, int?>(51);

            for (var i = 0; i <= 50; i++)
            {
                startIndexesByLength[i] = null;
            }

            for (var i = 0; i < partNumbers.Length; i++)
            {
                var length = partNumbers[i].Length;
                if (startIndexesByLength[length] is null)
                    startIndexesByLength[length] = i;
            }

            BackwardFill(startIndexesByLength);

            for (var length = 4; length <= 50; length++)
            {
                var tempDictionary = new Dictionary<string, List<string>>();

                if (startIndexesByLength.TryGetValue(length + 1, out var startIndex) && startIndex is not null)
                {
                    for (var i = startIndex.Value; i < partNumbers.Length; i++)
                    {
                        var suffix = partNumbers[i][^length..];
                        if (tempDictionary.TryGetValue(suffix, out var originalPartNumbers))
                        {
                            originalPartNumbers.Add(partNumbers[i]);
                        }
                        else
                        {
                            tempDictionary.TryAdd(suffix, new List<string>() { partNumbers[i] });
                        }
                    }
                }

                suffixesByLength[length] = tempDictionary;
            }

            return suffixesByLength;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BackwardFill(Dictionary<int, int?> dictionary)
    {
        var temp = dictionary[50];
        for (var i = 50; i >= 0; i--)
        {
            if (dictionary[i] is null)
            {
                dictionary[i] = temp;
            }
            else
            {
                temp = dictionary[i];
            }
        }
    }
}
