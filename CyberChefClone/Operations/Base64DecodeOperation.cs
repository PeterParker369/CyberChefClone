using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class Base64DecodeOperation : IOperation
{
    public string Name => "Base64 Decode";
    public string Description => "Decode Base64 input";
    public string Category => "Encoding";

    public Dictionary<string, object> Parameters { get; set; } = new();

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var base64String = System.Text.Encoding.UTF8.GetString(input);
        var result = Convert.FromBase64String(base64String);
        return Task.FromResult(result);
    }
}