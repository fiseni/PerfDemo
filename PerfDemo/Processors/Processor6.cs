using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace PerfDemo.Processors;

public class Processor6
{
    public string Identifier { get; } = nameof(Processor6);

    private const int MIN_STRING_LENGTH = 3;
    private const int MAX_STRING_LENGTH = 50;
    private static readonly MyComparer _myComparer = new MyComparer();

    private readonly Dictionary<Memory<byte>, MasterPartX?> _masterPartsByPartNumber;

    public Processor6(SourceDataX sourceData)
    {
        var masterPartsInfo = new MasterPartsInfo(sourceData.MasterParts);
        var partsInfo = new PartsInfo(sourceData.Parts);

        _masterPartsByPartNumber = BuildDictionary(masterPartsInfo, partsInfo);
    }

    public MasterPartX? FindMatchedPart(Memory<byte> partNumber)
    {
        if (partNumber.Length < MIN_STRING_LENGTH) return null;

        _masterPartsByPartNumber.TryGetValue(partNumber, out var match);
        return match;
    }

    private static Dictionary<Memory<byte>, MasterPartX?> BuildDictionary(MasterPartsInfo masterPartsInfo, PartsInfo partsInfo)
    {
        var masterPartsByPartNumber = new Dictionary<Memory<byte>, MasterPartX?>(partsInfo.Parts.Length, _myComparer);

        for (var i = 0; i < partsInfo.Parts.Length; i++)
        {
            var partNumber = partsInfo.Parts[i].PartNumber;

            var masterPartsBySuffix = masterPartsInfo.SuffixesByLength[partNumber.Length];
            if (masterPartsBySuffix is not null && masterPartsBySuffix.TryGetValue(partNumber, out var match))
            {
                masterPartsByPartNumber.TryAdd(partNumber, match);
                continue;
            }

            masterPartsBySuffix = masterPartsInfo.SuffixesByNoHyphensLength[partNumber.Length];
            if (masterPartsBySuffix is not null && masterPartsBySuffix.TryGetValue(partNumber, out var match2))
            {
                masterPartsByPartNumber.TryAdd(partNumber, match2);
                continue;
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
                    masterPartsByPartNumber.TryAdd(partsInfo.Parts[index].PartNumber, masterPart);
                }
            }
        }

        return masterPartsByPartNumber;
    }

    private sealed class MasterPartsInfo
    {
        public MasterPartX[] MasterParts { get; }
        public MasterPartX[] MasterPartsNoHyphens { get; }
        public Dictionary<Memory<byte>, MasterPartX>?[] SuffixesByLength { get; }
        public Dictionary<Memory<byte>, MasterPartX>?[] SuffixesByNoHyphensLength { get; }

        public MasterPartsInfo(MasterPartX[] masterParts)
        {
            MasterParts = masterParts
                .OrderBy(x => x.PartNumber.Length)
                .ToArray();

            MasterPartsNoHyphens = masterParts
                .Where(x => x.PartNumber.Length != x.PartNumberNoHyphens.Length && x.PartNumberNoHyphens.Length > 2)
                .OrderBy(x => x.PartNumberNoHyphens.Length)
                .ToArray();

            SuffixesByLength = new Dictionary<Memory<byte>, MasterPartX>?[MAX_STRING_LENGTH];
            SuffixesByNoHyphensLength = new Dictionary<Memory<byte>, MasterPartX>?[MAX_STRING_LENGTH];

            BuildSuffixDictionaries(SuffixesByLength, MasterParts, false);
            BuildSuffixDictionaries(SuffixesByNoHyphensLength, MasterPartsNoHyphens, true);
        }

        private static void BuildSuffixDictionaries(Dictionary<Memory<byte>, MasterPartX>?[] suffixesByLength, MasterPartX[] masterParts, bool useNoHyphen)
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
                    var tempDictionary = new Dictionary<Memory<byte>, MasterPartX>(masterParts.Length - startIndex.Value, _myComparer);
                    for (var i = startIndex.Value; i < masterParts.Length; i++)
                    {
                        var suffix = useNoHyphen
                            ? masterParts[i].PartNumberNoHyphens[^length..]
                            : masterParts[i].PartNumber[^length..];
                        tempDictionary.TryAdd(suffix, masterParts[i]);
                    }
                    suffixesByLength[length] = tempDictionary;
                }
            });
        }
    }

    private sealed class PartsInfo
    {
        public PartX[] Parts { get; }
        public Dictionary<Memory<byte>, List<int>>?[] SuffixesByLength { get; }

        public PartsInfo(PartX[] parts)
        {
            Parts = parts
                //.Select(x => x.PartNumber.Trim().ToUpper())
                .Where(x => x.PartNumber.Length > 2)
                .OrderBy(x => x.PartNumber.Length)
                .ToArray();

            SuffixesByLength = new Dictionary<Memory<byte>, List<int>>?[MAX_STRING_LENGTH];
            BuildSuffixDictionaries(SuffixesByLength, Parts);
        }

        private static void BuildSuffixDictionaries(Dictionary<Memory<byte>, List<int>>?[] suffixesByLength, PartX[] parts)
        {
            // Create and populate start indices.
            var startIndexByLength = new int?[MAX_STRING_LENGTH];
            for (var i = 0; i < MAX_STRING_LENGTH; i++)
            {
                startIndexByLength[i] = null;
            }
            for (var i = 0; i < parts.Length; i++)
            {
                var length = parts[i].PartNumber.Length;
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
                    var tempDictionary = new Dictionary<Memory<byte>, List<int>>(parts.Length - startIndex.Value, _myComparer);
                    for (var i = startIndex.Value; i < parts.Length; i++)
                    {
                        var suffix = parts[i].PartNumber[^length..];
                        if (tempDictionary.TryGetValue(suffix, out var originalPartIndices))
                        {
                            originalPartIndices.Add(i);
                        }
                        else
                        {
                            tempDictionary.TryAdd(suffix, [i]);
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

    public class MyComparer : IEqualityComparer<Memory<byte>>
    {
        public bool Equals(Memory<byte> x, Memory<byte> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (x.Span[i] != y.Span[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] Memory<byte> obj)
        {
            var x = Encoding.UTF8.GetString(obj.Span);

            return x.GetHashCode();
        }
    }

}
