using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class Base64EncodeOperation : IOperation
{
    public string Name => "Base64 Encode";
    public string Description => "Encode input to Base64 format";
    public string Category => "Encoding";

    public Dictionary<string, object> Parameters { get; set; } = new();

    public Dictionary<string, ParameterInfo> ParameterTypes => new();

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var result = Convert.ToBase64String(input);
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(result));
    }
}