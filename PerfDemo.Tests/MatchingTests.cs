using PerfDemo.Processors;

namespace PerfDemo.Tests;

public class MatchingTests
{
    private static readonly MasterPart[] _masterParts =
    [
        new("699"),
        new("Aqwertyuio"),
        new("QWERTYUIO"),
        new("zxc"),
        new("ZXC"),
        new("xxabcdefghi-jklmno"),
        new("ABCDEFGHI---------------JKLMNO"),
        new("cdefghijklmno"),
        new("CDEFGHIJKLMNO"),
        new("0000abcdefghijklmnoX"),
        new("zx"),
        new("qzp"),
        new("QZP"),
        new("AXqwertyuio"),
        new("AAqwertyuio"),
        new("SSDDFF"),
        new("SSD-DFF"),
        new("XSSD-DFF"),
        new("z"),
        new(""),
    ];

    private sealed record TestPart(string? ExpectedMatch, string PartNumber);

    private static readonly TestPart[] _testParts =
    [
        // expected, partnumber
        new(null, "  "),
        new(null, ""),
        new(null, "a"),
        new(null, "A"),
        new(null, "ab"),
        new(null, "AB"),
        new(null, "xyz"),
        new(null, "XYZ"),
        new(null, "klmn"),
        new(null, "KLMN"),
        new(null, "ijkl"),
        new(null, "IJKL"),
        new("699", "W50-699"),
        new("CDEFGHIJKLMNO", "defghijklmno"),
        new("CDEFGHIJKLMNO", "DEFGHIJKLMNO"),
        new("ABCDEFGHI---------------JKLMNO", "DEFGHI---------------JKLMNO"),
        new("ABCDEFGHI---------------JKLMNO", "bcdefghijklmno"),
        new("ABCDEFGHI---------------JKLMNO", "BCDEFGHIJKLMNO"),
        new("AAQWERTYUIO", "AAAAqwertyuio"),
        new("AAQWERTYUIO", "AAAAQWERTYUIO"),
        new("AAQWERTYUIO", "AAQWERTYUIO"),
        new("AQWERTYUIO", "AQWERTYUIO"),
        new("QZP", "qzp"),
        new("ZXC", "zXC"),
        new("SSD-DFF", "SSD-DFF"),
        new("XSSD-DFF", "XSSDDFF"),
    ];

    private static readonly SourceData _sourceData = new SourceData(_masterParts, _testParts.Select(x => new Part(x.PartNumber)).ToArray());
    public static IEnumerable<object?[]> GetPartNumbers() => _testParts.Select(x => (new object?[] { x.ExpectedMatch, x.PartNumber }));

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Processor1_Tests(string? expectedMatch, string partNumber)
    {
        // Processor 1 (the original code) will fail on many cases since it will return the first match.
        // All other implementations are improved to return the best match.

        var processor = new Processor1(_sourceData);
        var result = processor.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Processor2_Tests(string? expectedMatch, string partNumber)
    {
        var processor = new Processor2(_sourceData);
        var result = processor.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Processor3_Tests(string? expectedMatch, string partNumber)
    {
        var processor = new Processor3(_sourceData);
        var result = processor.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Processor4_Tests(string? expectedMatch, string partNumber)
    {
        var processor = new Processor4(_sourceData);
        var result = processor.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Processor5_Tests(string? expectedMatch, string partNumber)
    {
        var processor = new Processor5(_sourceData);
        var result = processor.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }
}
