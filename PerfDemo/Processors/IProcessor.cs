namespace PerfDemo.Processors;

public interface IProcessor
{
    string Identifier { get; }
    MasterPart? FindMatchedPart(string partNumber);
}
