namespace PerfDemo;

public class MasterPart
{
    public string PartNumber { get; }
    public string PartNumberNoHyphens { get; }

    public MasterPart(string partNumber)
    {
        PartNumber = partNumber.ToUpper().Trim();
        PartNumberNoHyphens = PartNumber.Replace("-", "");
    }
}

