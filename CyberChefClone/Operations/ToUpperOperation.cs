using System.Text;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class ToUpperOperation : IOperation
{
    public string Name => "To Upper Case";
    public string Description => "Convert text to UPPER CASE";
    public string Category => "Data manipulation";

    public Dictionary<string, object> Parameters { get; set; } = new();

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var text = Encoding.UTF8.GetString(input);
        var result = text.ToUpper();
        return Task.FromResult(Encoding.UTF8.GetBytes(result));
    }
}