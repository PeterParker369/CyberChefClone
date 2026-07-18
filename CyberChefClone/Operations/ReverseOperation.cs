using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class ReverseOperation : IOperation
{
    public string Name => "Reverse";
    public string Description => "Reverse the input data";
    public string Category => "Data manipulation";

    public Dictionary<string, object> Parameters { get; set; } = new();

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var result = input.Reverse().ToArray();
        return Task.FromResult(result);
    }
}