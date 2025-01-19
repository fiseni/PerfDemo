using System.Runtime.CompilerServices;

namespace PerfDemo.Processors;

public class Processor2 : IProcessor
{
    public string Identifier { get; } = nameof(Processor2);

    private readonly MasterPartsInfo _masterPartsInfo;

    public Processor2(SourceData sourceData)
    {
        _masterPartsInfo = new MasterPartsInfo(sourceData.MasterParts);
    }

    public MasterPart? FindMatchedPart(string partNumber)
    {
        partNumber = partNumber.Trim();
        if (partNumber.Length < 3) return null;

        partNumber = partNumber.ToUpper();

        var masterPart = FindMatchForPartNumber(partNumber, _masterPartsInfo.MasterPartNumbers, _masterPartsInfo.StartIndexByPartNumberLength, false);
        masterPart ??= FindMatchForPartNumber(partNumber, _masterPartsInfo.MasterPartNumbersNoHyphens, _masterPartsInfo.StartIndexByPartNumberNoHyphensLength, true);
        masterPart ??= FindMatchInPartNumber(partNumber, _masterPartsInfo.MasterPartNumbers, _masterPartsInfo.StartIndexByPartNumberLengthOpposite);

        return masterPart;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MasterPart? FindMatchForPartNumber(
        ReadOnlySpan<char> partNumber,
        ReadOnlySpan<MasterPart> masterParts,
        Dictionary<int, int?> startIndexByLength,
        bool useNoHyphenNumbers)
    {
        if (startIndexByLength.TryGetValue(partNumber.Length, out var startIndex) && startIndex is not null)
        {
            for (var i = startIndex.Value; i < masterParts.Length; i++)
            {
                var masterPart = masterParts[i];
                var masterPartNumber = useNoHyphenNumbers ? masterPart.PartNumberNoHyphens : masterPart.PartNumber;
                var offset = masterPartNumber.Length - partNumber.Length;

                if (offset >= 0 && IsSuffix(masterPartNumber, partNumber, offset))
                    return masterPart;
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MasterPart? FindMatchInPartNumber(
        ReadOnlySpan<char> partNumber,
        ReadOnlySpan<MasterPart> masterParts,
        Dictionary<int, int?> startIndexByLength)
    {
        if (startIndexByLength.TryGetValue(partNumber.Length - 1, out var startIndex) && startIndex is not null)
        {
            for (var i = startIndex.Value; i >= 0; i--)
            {
                var masterPart = masterParts[i];
                var offset = partNumber.Length - masterPart.PartNumber.Length;

                if (offset >= 0 && IsSuffix(partNumber, masterPart.PartNumber, offset))
                    return masterPart;
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSuffix(ReadOnlySpan<char> source, ReadOnlySpan<char> suffix, int offset)
    {
        var segment = source.Slice(offset);

        return segment[0] == suffix[0] && segment.SequenceEqual(suffix);
    }

    private sealed class MasterPartsInfo
    {
        public MasterPart[] MasterPartNumbers { get; }
        public MasterPart[] MasterPartNumbersNoHyphens { get; }
        public Dictionary<int, int?> StartIndexByPartNumberLength { get; } = new(51);
        public Dictionary<int, int?> StartIndexByPartNumberLengthOpposite { get; } = new(51);
        public Dictionary<int, int?> StartIndexByPartNumberNoHyphensLength { get; } = new(51);

        public MasterPartsInfo(MasterPart[] masterParts)
        {
            MasterPartNumbers = masterParts
                .OrderBy(x => x.PartNumber.Length)
                .DistinctBy(x => x.PartNumber)
                .ToArray();

            MasterPartNumbersNoHyphens = masterParts
                .Where(x => x.PartNumberNoHyphens.Length > 2)
                .OrderBy(x => x.PartNumberNoHyphens.Length)
                .DistinctBy(x => x.PartNumberNoHyphens)
                .ToArray();

            // Initialize the dictionaries
            for (var i = 0; i <= 50; i++)
            {
                StartIndexByPartNumberLength[i] = null;
                StartIndexByPartNumberLengthOpposite[i] = null;
                StartIndexByPartNumberNoHyphensLength[i] = null;
            }

            // Add the start indexes
            for (var i = 0; i < MasterPartNumbers.Length; i++)
            {
                var length = MasterPartNumbers[i].PartNumber.Length;
                if (StartIndexByPartNumberLength[length] is null)
                    StartIndexByPartNumberLength[length] = i;

                StartIndexByPartNumberLengthOpposite[length] = i;
            }

            for (var i = 0; i < MasterPartNumbersNoHyphens.Length; i++)
            {
                var length = MasterPartNumbersNoHyphens[i].PartNumberNoHyphens.Length;
                if (StartIndexByPartNumberNoHyphensLength[length] is null)
                    StartIndexByPartNumberNoHyphensLength[length] = i;
            }

            BackwardFill(StartIndexByPartNumberLength);
            BackwardFill(StartIndexByPartNumberNoHyphensLength);
            ForwardFill(StartIndexByPartNumberLengthOpposite);
        }

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

        private static void ForwardFill(Dictionary<int, int?> dictionary)
        {
            var temp = dictionary[0];
            for (var i = 0; i <= 50; i++)
            {
                if (dictionary[i] == null)
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
}
