namespace CyberChefClone.Services;

public interface IOperation
{
    string Name { get; }
    string Description { get; }
    string Category { get; }
    Dictionary<string, object> Parameters { get; set; }
    Dictionary<string, ParameterInfo> ParameterTypes { get; }
    Task<byte[]> ExecuteAsync(byte[] input);
}

public class ParameterInfo
{
    public string Type { get; set; } = "string";
    public string Label { get; set; } = "";
    public object DefaultValue { get; set; } = "";
    public List<string> Options { get; set; } = new();
    public string Description { get; set; } = "";
}