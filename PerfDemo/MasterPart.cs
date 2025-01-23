namespace PerfDemo;

public class MasterPart
{
    public string PartNumberOriginal { get; }
    public string PartNumber { get; }
    public string PartNumberNoHyphens { get; }

    public MasterPart(string partNumber)
    {
        PartNumberOriginal = partNumber.Trim();
        PartNumber = partNumber.ToUpper().Trim();
        PartNumberNoHyphens = PartNumber.Replace("-", "");
    }
}

