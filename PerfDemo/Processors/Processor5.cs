using System.Runtime.CompilerServices;

namespace PerfDemo.Processors;

public class Processor5 : IProcessor
{
    public string Identifier { get; } = nameof(Processor5);

    private const int MIN_STRING_LENGTH = 3;
    private const int MAX_STRING_LENGTH = 50;

    private readonly Dictionary<string, MasterPart?> _masterPartsByPartNumber;
    private readonly Dictionary<string, MasterPart?>.AlternateLookup<ReadOnlySpan<char>> _masterPartsByPartNumberAltLookup;

    public Processor5(SourceData sourceData)
    {
        var masterPartsInfo = new MasterPartsInfo(sourceData.MasterParts);
        var partsInfo = new PartsInfo(sourceData.Parts);

        _masterPartsByPartNumber = BuildDictionary(masterPartsInfo, partsInfo);
        _masterPartsByPartNumberAltLookup = _masterPartsByPartNumber.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        var partNumberSpan = partNumber.AsSpan().Trim();
        if (partNumberSpan.Length < MIN_STRING_LENGTH) return null;

        Span<char> buffer = stackalloc char[partNumberSpan.Length];
        partNumberSpan.ToUpperInvariant(buffer);

        _masterPartsByPartNumberAltLookup.TryGetValue(buffer, out var match);
        return match;
    }

    private static Dictionary<string, MasterPart?> BuildDictionary(MasterPartsInfo masterPartsInfo, PartsInfo partsInfo)
    {
        var masterPartsByPartNumber = new Dictionary<string, MasterPart?>(partsInfo.PartNumbers.Length);

        for (var i = 0; i < partsInfo.PartNumbers.Length; i++)
        {
            var partNumber = partsInfo.PartNumbers[i];

            var masterPartsBySuffix = masterPartsInfo.SuffixesByLength[partNumber.Length];
            var match = masterPartsBySuffix?.GetValueOrDefault(partNumber);
            if (match is not null)
            {
                masterPartsByPartNumber.TryAdd(partNumber, match);
                continue;
            }

            masterPartsBySuffix = masterPartsInfo.SuffixesByNoHyphensLength[partNumber.Length];
            match = masterPartsBySuffix?.GetValueOrDefault(partNumber);
            if (match is not null)
            {
                masterPartsByPartNumber.TryAdd(partNumber, match);
            }
        }

        for (var i = masterPartsInfo.MasterParts.Length - 1; i >= 0; i--)
        {
            var masterPart = masterPartsInfo.MasterParts[i];

            var partsBySuffix = partsInfo.SuffixesByLength[masterPart.PartNumber.Length];
            var originalPartIndices = partsBySuffix?.GetValueOrDefault(masterPart.PartNumber);
            if (originalPartIndices is not null)
            {
                for (var j = originalPartIndices.Count - 1; j >= 0; j--)
                {
                    var index = originalPartIndices[j];
                    masterPartsByPartNumber.TryAdd(partsInfo.PartNumbers[index], masterPart);
                }
            }
        }

        return masterPartsByPartNumber;
    }

    private sealed class MasterPartsInfo
    {
        public MasterPart[] MasterParts { get; }
        public MasterPart[] MasterPartsNoHyphens { get; }
        public Dictionary<string, MasterPart>?[] SuffixesByLength { get; }
        public Dictionary<string, MasterPart>?[] SuffixesByNoHyphensLength { get; }

        public MasterPartsInfo(MasterPart[] masterParts)
        {
            MasterParts = masterParts
                .OrderBy(x => x.PartNumber.Length)
                .ToArray();

            MasterPartsNoHyphens = masterParts
                .Where(x => x.PartNumberNoHyphens.Length > 2 && x.PartNumber.Contains('-'))
                .OrderBy(x => x.PartNumberNoHyphens.Length)
                .ToArray();

            SuffixesByLength = new Dictionary<string, MasterPart>?[MAX_STRING_LENGTH];
            SuffixesByNoHyphensLength = new Dictionary<string, MasterPart>?[MAX_STRING_LENGTH];

            BuildSuffixDictionaries(SuffixesByLength, MasterParts, false);
            BuildSuffixDictionaries(SuffixesByNoHyphensLength, MasterPartsNoHyphens, true);
        }

        private static void BuildSuffixDictionaries(Dictionary<string, MasterPart>?[] suffixesByLength, MasterPart[] masterParts, bool useNoHyphen)
        {
            // Create and populate start indices.
            var startIndexesByLength = new int?[MAX_STRING_LENGTH];
            for (var i = 0; i < MAX_STRING_LENGTH; i++)
            {
                startIndexesByLength[i] = null;
            }
            for (var i = 0; i < masterParts.Length; i++)
            {
                var length = useNoHyphen
                    ? masterParts[i].PartNumberNoHyphens.Length
                    : masterParts[i].PartNumber.Length;

                if (startIndexesByLength[length] is null)
                    startIndexesByLength[length] = i;
            }
            BackwardFill(startIndexesByLength);

            // Create and populate suffix dictionaries.
            Parallel.For(MIN_STRING_LENGTH, MAX_STRING_LENGTH, length =>
            {
                var startIndex = startIndexesByLength[length];
                if (startIndex is not null)
                {
                    var tempDictionary = new Dictionary<string, MasterPart>(masterParts.Length - startIndex.Value);
                    var altLookup = tempDictionary.GetAlternateLookup<ReadOnlySpan<char>>();
                    for (var i = startIndex.Value; i < masterParts.Length; i++)
                    {
                        var suffix = useNoHyphen
                            ? masterParts[i].PartNumberNoHyphens.AsSpan()[^length..]
                            : masterParts[i].PartNumber.AsSpan()[^length..];
                        altLookup.TryAdd(suffix, masterParts[i]);
                    }
                    suffixesByLength[length] = tempDictionary;
                }
            });
        }
    }

    private sealed class PartsInfo
    {
        public string[] PartNumbers { get; }
        public Dictionary<string, List<int>>?[] SuffixesByLength { get; }

        public PartsInfo(Part[] parts)
        {
            PartNumbers = parts
                .Select(x => x.PartNumber.Trim().ToUpper())
                .Where(x => x.Length > 2)
                .OrderBy(x => x.Length)
                .ToArray();

            SuffixesByLength = new Dictionary<string, List<int>>?[MAX_STRING_LENGTH];
            BuildSuffixDictionaries(SuffixesByLength, PartNumbers);
        }

        private static void BuildSuffixDictionaries(Dictionary<string, List<int>>?[] suffixesByLength, string[] partNumbers)
        {
            // Create and populate start indices.
            var startIndexByLength = new int?[MAX_STRING_LENGTH];
            for (var i = 0; i < MAX_STRING_LENGTH; i++)
            {
                startIndexByLength[i] = null;
            }
            for (var i = 0; i < partNumbers.Length; i++)
            {
                var length = partNumbers[i].Length;
                if (startIndexByLength[length] is null)
                    startIndexByLength[length] = i;
            }
            BackwardFill(startIndexByLength);

            // Create and populate suffix dictionaries.
            Parallel.For(MIN_STRING_LENGTH, MAX_STRING_LENGTH, length =>
            {
                var startIndex = startIndexByLength[length];
                if (startIndex is not null)
                {
                    var tempDictionary = new Dictionary<string, List<int>>(partNumbers.Length - startIndex.Value);
                    var altLookup = tempDictionary.GetAlternateLookup<ReadOnlySpan<char>>();
                    for (var i = startIndex.Value; i < partNumbers.Length; i++)
                    {
                        var suffix = partNumbers[i].AsSpan()[^length..];
                        if (altLookup.TryGetValue(suffix, out var originalPartIndices))
                        {
                            originalPartIndices.Add(i);
                        }
                        else
                        {
                            altLookup.TryAdd(suffix, [i]);
                        }
                    }
                    suffixesByLength[length] = tempDictionary;
                }
            });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BackwardFill(int?[] array)
    {
        var temp = array[MAX_STRING_LENGTH - 1];
        for (var i = MAX_STRING_LENGTH - 1; i >= 0; i--)
        {
            if (array[i] is null)
            {
                array[i] = temp;
            }
            else
            {
                temp = array[i];
            }
        }
    }
}
