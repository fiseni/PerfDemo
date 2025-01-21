namespace PerfDemo;

//[DebuggerDisplay("{System.Text.Encoding.ASCII.GetString(PartNumber.Span)} | {System.Text.Encoding.ASCII.GetString(PartNumberNoHyphens.Span)}")]
public struct MasterPartX
{
    public Memory<byte> PartNumber;
    public Memory<byte> PartNumberNoHyphens;
}

//[DebuggerDisplay("{System.Text.Encoding.ASCII.GetString(PartNumber.Span)}")]
public struct PartX
{
    public Memory<byte> PartNumber;
}

public class SourceDataX
{
    // BenchmarkDotNet creates a new folder on each run named with an arbitrary GUID.
    // This is a workaround to get the correct path to the data files. Don't ask :)
    public static string MasterPartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "masterParts.txt");
    public static string PartsFilePath = Path.Combine("..", "..", "..", "..", "Data", "parts.txt");

    public const byte LF = 10;
    public const byte CR = 13;
    public const byte DASH = 45;

    public MasterPartX[] MasterParts = default!;
    public PartX[] Parts = default!;

    public static SourceDataX LoadForBenchmark()
        => Load(MasterPartsFilePath, PartsFilePath);

    public static SourceDataX Load(string masterPartsFilePath, string partsFilePath)
    {
        var masterParts = BuildMasterParts(masterPartsFilePath);
        var parts = BuildParts(partsFilePath);
        return new SourceDataX
        {
            MasterParts = masterParts,
            Parts = parts
        };
    }

    private static MasterPartX[] BuildMasterParts(string masterPartsFilePath)
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

        var masterParts = new MasterPartX[lines];

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
                    line = block[startStringIndex..(i - 1)];
                }
                else
                {
                    line = block[startStringIndex..i];
                }
                var trimmedLine = ToUpperTrim(line);

                if (!trimmedLine.IsEmpty)
                {
                    masterParts[masterPartsIndex].PartNumber = trimmedLine;
                    masterPartsIndex++;
                    startStringIndex = i + 1;
                    if (dashCount > 0)
                    {
                        var dashRemoved = RemoveDashes(trimmedLine, blockNoHyphens.AsMemory().Slice(masterPartsNoHyphensIndex, trimmedLine.Length));
                        masterParts[masterPartsIndex - 1].PartNumberNoHyphens = dashRemoved;
                        masterPartsNoHyphensIndex += dashRemoved.Length;
                    }
                    else
                    {
                        masterParts[masterPartsIndex - 1].PartNumberNoHyphens = masterParts[masterPartsIndex - 1].PartNumber;
                    }
                }
                dashCount = 0;
            }
        }

        return masterParts;
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
        var x = buffer.Slice(0, k);
        return x;
    }

    public static Memory<byte> ToUpperTrim(Memory<byte> partNumber)
    {
        for (int i = 0; i < partNumber.Length; i++)
        {
            partNumber.Span[i] = (byte)char.ToUpper((char)partNumber.Span[i]);
        }
        var x = partNumber.Trim((byte)' ');
        return x;
    }

    private static PartX[] BuildParts(string partsFilePath)
    {
        var fileSize = new FileInfo(partsFilePath).Length;
        var content = File.ReadAllBytes(partsFilePath);
        byte[] block = new byte[fileSize];
        content.CopyTo(block, 0);

        var lines = 0;
        for (int i = 0; i < block.Length; i++)
        {
            if (block[i] == LF)
            {
                lines++;
            }
        }

        var parts = new PartX[lines];

        var partsIndex = 0;
        var startStringIndex = 0;
        for (int i = 0; i < block.Length; i++)
        {
            if (block[i] == LF)
            {
                Memory<byte> line;
                if (i > 0 && block[i - 1] == CR)
                {
                    line = block[startStringIndex..(i - 1)];
                }
                else
                {
                    line = block[startStringIndex..i];
                }
                var trimmedLine = ToUpperTrim(line);
                if (!trimmedLine.IsEmpty)
                {
                    parts[partsIndex].PartNumber = trimmedLine;
                    partsIndex++;
                    startStringIndex = i + 1;
                }
            }
        }

        return parts;
    }
}

