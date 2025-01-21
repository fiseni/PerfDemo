using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace PerfDemo.Processors;

public class Processor6
{
    public string Identifier { get; } = nameof(Processor6);

    private const int MIN_STRING_LENGTH = 3;
    private const int MAX_STRING_LENGTH = 50;
    private static readonly MemoryComparer _memoryComparer = new();

    private readonly Dictionary<Memory<byte>, MasterPart6?> _masterPartsByPartNumber;
    private readonly Dictionary<Memory<byte>, MasterPart6?>.AlternateLookup<ReadOnlySpan<byte>> _masterPartsByPartNumberAltLookup;

    public Processor6(SourceData6 sourceData)
    {
        var masterPartsInfo = new MasterPartsInfo(sourceData.MasterParts);
        var partsInfo = new PartsInfo(sourceData.Parts);

        _masterPartsByPartNumber = BuildDictionary(masterPartsInfo, partsInfo);
        _masterPartsByPartNumberAltLookup = _masterPartsByPartNumber.GetAlternateLookup<ReadOnlySpan<byte>>();
    }

    public MasterPart6? FindMatchedPart(Memory<byte> partNumber)
    {
        Span<byte> buffer = stackalloc byte[partNumber.Length];
        var trimmed = SourceData6.ToUpperTrim(partNumber, buffer);

        if (trimmed.Length < MIN_STRING_LENGTH) return null;

        _masterPartsByPartNumberAltLookup.TryGetValue(trimmed, out var match);
        return match;
    }

    private static Dictionary<Memory<byte>, MasterPart6?> BuildDictionary(MasterPartsInfo masterPartsInfo, PartsInfo partsInfo)
    {
        var masterPartsByPartNumber = new Dictionary<Memory<byte>, MasterPart6?>(partsInfo.Parts.Length, _memoryComparer);

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
        public MasterPart6[] MasterParts { get; }
        public MasterPart6[] MasterPartsNoHyphens { get; }
        public Dictionary<Memory<byte>, MasterPart6>?[] SuffixesByLength { get; }
        public Dictionary<Memory<byte>, MasterPart6>?[] SuffixesByNoHyphensLength { get; }

        public MasterPartsInfo(MasterPart6[] masterParts)
        {
            MasterParts = masterParts
                .OrderBy(x => x.PartNumber.Length)
                .ToArray();

            MasterPartsNoHyphens = masterParts
                .Where(x => x.PartNumber.Length != x.PartNumberNoHyphens.Length && x.PartNumberNoHyphens.Length > 2)
                .OrderBy(x => x.PartNumberNoHyphens.Length)
                .ToArray();

            SuffixesByLength = new Dictionary<Memory<byte>, MasterPart6>?[MAX_STRING_LENGTH];
            SuffixesByNoHyphensLength = new Dictionary<Memory<byte>, MasterPart6>?[MAX_STRING_LENGTH];

            BuildSuffixDictionaries(SuffixesByLength, MasterParts, false);
            BuildSuffixDictionaries(SuffixesByNoHyphensLength, MasterPartsNoHyphens, true);
        }

        private static void BuildSuffixDictionaries(Dictionary<Memory<byte>, MasterPart6>?[] suffixesByLength, MasterPart6[] masterParts, bool useNoHyphen)
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
                    var tempDictionary = new Dictionary<Memory<byte>, MasterPart6>(masterParts.Length - startIndex.Value, _memoryComparer);
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
        public Part6[] Parts { get; }
        public Dictionary<Memory<byte>, List<int>>?[] SuffixesByLength { get; }

        public PartsInfo(Part6[] parts)
        {
            // Allocate block for all strings so they're in a contiguous memory;
            var block = new byte[parts.Length * MAX_STRING_LENGTH];
            var newParts = new Part6[parts.Length];
            for (int i = 0; i < newParts.Length; i++)
            {
                newParts[i] = new Part6();
            }

            // Populate Parts
            var blockIndex = 0;
            var k = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                var partNumber = parts[i].PartNumber;
                var trimmed = SourceData6.ToUpperTrim(partNumber, block.AsMemory().Slice(blockIndex, partNumber.Length));
                if (trimmed.Length > 2)
                {
                    newParts[k].PartNumber = trimmed;
                    k++;
                    blockIndex += trimmed.Length;
                }
            }

            Array.Sort(newParts, (x, y) => x.PartNumber.Length.CompareTo(y.PartNumber.Length));
            Parts = newParts;

            SuffixesByLength = new Dictionary<Memory<byte>, List<int>>?[MAX_STRING_LENGTH];
            BuildSuffixDictionaries(SuffixesByLength, Parts);
        }

        private static void BuildSuffixDictionaries(Dictionary<Memory<byte>, List<int>>?[] suffixesByLength, Part6[] parts)
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
                    var tempDictionary = new Dictionary<Memory<byte>, List<int>>(parts.Length - startIndex.Value, _memoryComparer);
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

    public class MemoryComparer : IEqualityComparer<Memory<byte>>, IAlternateEqualityComparer<ReadOnlySpan<byte>, Memory<byte>>
    {
        // In our case we will never add using ReadOnlySpan<byte>
        public Memory<byte> Create(ReadOnlySpan<byte> alternate)
            => throw new NotImplementedException();

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

        public bool Equals(ReadOnlySpan<byte> alternate, Memory<byte> other)
        {
            if (alternate.Length != other.Span.Length)
            {
                return false;
            }

            for (int i = 0; i < alternate.Length; i++)
            {
                if (alternate[i] != other.Span[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] Memory<byte> obj)
        {
            int hash = 16777619;
            for (int i = 0; i < obj.Length; i++)
            {
                hash = (hash * 31) ^ obj.Span[i];
            }
            return hash;
        }

        public int GetHashCode(ReadOnlySpan<byte> alternate)
        {
            int hash = 16777619;
            for (int i = 0; i < alternate.Length; i++)
            {
                hash = (hash * 31) ^ alternate[i];
            }
            return hash;
        }
    }
}

[DebuggerDisplay("{System.Text.Encoding.ASCII.GetString(PartNumber.Span)} | {System.Text.Encoding.ASCII.GetString(PartNumberNoHyphens.Span)}")]
public class MasterPart6
{
    public Memory<byte> PartNumber;
    public Memory<byte> PartNumberNoHyphens;
}

[DebuggerDisplay("{System.Text.Encoding.ASCII.GetString(PartNumber.Span)}")]
public class Part6
{
    public Memory<byte> PartNumber;
}
public class SourceData6
{
    // BenchmarkDotNet creates a new folder on each run named with an arbitrary GUID.
    // This is a workaround to get the correct path to the data files. Don't ask :)
    public static string MasterPartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "masterParts.txt");
    public static string PartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "parts.txt");

    public const byte LF = 10;
    public const byte CR = 13;
    public const byte DASH = 45;

    public MasterPart6[] MasterParts = default!;
    public Part6[] Parts = default!;

    public static SourceData6 LoadForBenchmark()
        => Load(MasterPartsFilePath, PartsFilePath);

    public static SourceData6 Load(string masterPartsFilePath, string partsFilePath)
    {
        var masterParts = BuildMasterParts(masterPartsFilePath);
        var parts = BuildParts(partsFilePath);
        return new SourceData6
        {
            MasterParts = masterParts,
            Parts = parts
        };
    }

    private static MasterPart6[] BuildMasterParts(string masterPartsFilePath)
    {
        var fileSize = new FileInfo(masterPartsFilePath).Length;
        var content = File.ReadAllBytes(masterPartsFilePath);
        byte[] block = new byte[fileSize];
        content.CopyTo(block, 0);
        byte[] blockNoHyphens = new byte[fileSize];

        var lines = 0;
        for (int i = 0; i < block.Length; i++)
        {
            if (block[i] == LF)
            {
                lines++;
            }
        }

        var masterParts = new MasterPart6[lines];
        for (int i = 0; i < lines; i++)
        {
            masterParts[i] = new MasterPart6();
        }

        var masterPartsIndex = 0;
        var startStringIndex = 0;
        var masterPartsNoHyphensIndex = 0;
        int dashCount = 0;
        for (int i = 0; i < block.Length; i++)
        {
            if (block[i] == DASH)
            {
                dashCount++;
            }
            if (block[i] == LF)
            {
                Memory<byte> line;
                if (i > 0 && block[i - 1] == CR)
                {
                    line = block.AsMemory()[startStringIndex..(i - 1)];
                }
                else
                {
                    line = block.AsMemory()[startStringIndex..i];
                }
                var trimmedLine = ToUpperTrimInPlace(line);

                if (!trimmedLine.IsEmpty)
                {
                    masterParts[masterPartsIndex].PartNumber = trimmedLine;
                    if (dashCount > 0)
                    {
                        var dashRemoved = RemoveDashes(trimmedLine, blockNoHyphens.AsMemory().Slice(masterPartsNoHyphensIndex, trimmedLine.Length));
                        masterParts[masterPartsIndex].PartNumberNoHyphens = dashRemoved;
                        masterPartsNoHyphensIndex += dashRemoved.Length;
                    }
                    else
                    {
                        masterParts[masterPartsIndex].PartNumberNoHyphens = masterParts[masterPartsIndex].PartNumber;
                    }
                    masterPartsIndex++;
                    startStringIndex = i + 1;
                }
                dashCount = 0;
            }
        }

        return masterParts;
    }

    private static Part6[] BuildParts(string partsFilePath)
    {
        var content = File.ReadAllLines(partsFilePath);
        var parts = new Part6[content.Length];
        for (int i = 0; i < content.Length; i++)
        {
            parts[i] = new Part6
            {
                PartNumber = Encoding.ASCII.GetBytes(content[i])
            };
        }

        return parts;
    }

    public static Memory<byte> RemoveDashes(Memory<byte> partNumber, Memory<byte> buffer)
    {
        var k = 0;
        for (int i = 0; i < partNumber.Length; i++)
        {
            if (partNumber.Span[i] != DASH)
            {
                buffer.Span[k] = partNumber.Span[i];
                k++;
            }
        }
        var output = buffer[..k];
        return output;
    }

    public static Memory<byte> ToUpperTrimInPlace(Memory<byte> partNumber)
    {
        for (int i = 0; i < partNumber.Length; i++)
        {
            partNumber.Span[i] = (byte)char.ToUpper((char)partNumber.Span[i]);
        }
        var output = partNumber.Trim((byte)' ');
        return output;
    }

    public static Memory<byte> ToUpperTrim(Memory<byte> partNumber, Memory<byte> buffer)
    {
        for (int i = 0; i < partNumber.Length; i++)
        {
            buffer.Span[i] = (byte)char.ToUpper((char)partNumber.Span[i]);
        }
        var output = buffer.Trim((byte)' ');
        return output;
    }

    public static Span<byte> ToUpperTrim(Memory<byte> partNumber, Span<byte> buffer)
    {
        for (int i = 0; i < partNumber.Length; i++)
        {
            buffer[i] = (byte)char.ToUpper((char)partNumber.Span[i]);
        }
        var output = buffer.Trim((byte)' ');
        return output;
    }
}
