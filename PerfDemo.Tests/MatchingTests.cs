using PerfDemo.Services;

namespace PerfDemo.Tests;

public class MatchingTests
{
    private static readonly MasterPart[] _masterParts =
    [
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

    private sealed record PartNumberSeed(string? ExpectedMatch, string PartNumber);

    private static readonly PartNumberSeed[] _partNumbersSeed =
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
        new("CDEFGHIJKLMNO", "defghijklmno"),
        new("CDEFGHIJKLMNO", "DEFGHIJKLMNO"),
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

    public static IEnumerable<object?[]> GetPartNumbers() => _partNumbersSeed.Select(x => (new object?[] { x.ExpectedMatch, x.PartNumber }));

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option1_Tests(string? expectedMatch, string partNumber)
    {
        // Option 1 (the original code) will fail on many cases since it will return the first match.
        // All other options are improved to return the best match.

        var service = new Service1(_masterParts);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option2_Tests(string? expectedMatch, string partNumber)
    {
        var service = new Service2(_masterParts);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option3_Tests(string? expectedMatch, string partNumber)
    {
        var partNumbers = _partNumbersSeed.Select(x => new Part(x.PartNumber)).ToArray();

        var service = new Service3(_masterParts, partNumbers);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option4_Tests(string? expectedMatch, string partNumber)
    {
        var partNumbers = _partNumbersSeed.Select(x => new Part(x.PartNumber)).ToArray();

        var service = new Service4(_masterParts, partNumbers);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option5_Tests(string? expectedMatch, string partNumber)
    {
        var partNumbers = _partNumbersSeed.Select(x => new Part(x.PartNumber)).ToArray();

        var service = new Service5(_masterParts);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }

    [Theory]
    [MemberData(nameof(GetPartNumbers))]
    public void Option6_Tests(string? expectedMatch, string partNumber)
    {
        var partNumbers = _partNumbersSeed.Select(x => new Part(x.PartNumber)).ToArray();

        var service = new Service6(_masterParts);
        var result = service.FindMatchedPart(partNumber);

        Assert.Equal(expectedMatch, result?.PartNumber);
    }
}
