using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class XorOperation : IOperation
{
    public string Name => "XOR";
    public string Description => "Apply XOR encryption with a key (0-255)";
    public string Category => "Encryption";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["Key"] = 0x42
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["Key"] = new ParameterInfo
        {
            Type = "number",
            Label = "Key (0-255)",
            DefaultValue = 0x42,
            Description = "XOR key value (0-255)"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var key = ParameterHelper.GetByte(Parameters, "Key", 0x42);
        var result = input.Select(b => (byte)(b ^ key)).ToArray();
        return Task.FromResult(result);
    }
}