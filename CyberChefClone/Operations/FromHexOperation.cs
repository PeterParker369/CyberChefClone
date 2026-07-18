using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class FromHexOperation : IOperation
{
    public string Name => "From Hex";
    public string Description => "Convert hexadecimal string to text or bytes";
    public string Category => "Data manipulation";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["OutputFormat"] = "String"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "String",
            Options = new List<string> { "String", "Base64" },
            Description = "Output data format"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        Console.WriteLine($"=== From Hex ExecuteAsync ===");

        var outputFormat = GetStringParameter("OutputFormat");
        var inputString = Encoding.UTF8.GetString(input);

        Console.WriteLine($"Input: '{inputString}'");
        Console.WriteLine($"OutputFormat: '{outputFormat}'");

        // Remove spaces, dashes, and other separators
        var hexString = inputString.Replace(" ", "").Replace("-", "").Replace(":", "").Replace(",", "").Replace("\n", "").Replace("\r", "");
        hexString = hexString.Trim();

        Console.WriteLine($"Cleaned Hex: '{hexString}'");

        if (string.IsNullOrEmpty(hexString))
        {
            Console.WriteLine("Empty hex string, returning empty result");
            return Task.FromResult(Array.Empty<byte>());
        }

        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException($"Invalid Hex string length: {hexString.Length}. Hex string must have even length.");
        }

        // Convert Hex to bytes
        var data = new byte[hexString.Length / 2];
        for (int i = 0; i < hexString.Length; i += 2)
        {
            data[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
        }

        Console.WriteLine($"Decoded {data.Length} bytes");

        // Convert based on output format
        byte[] result;
        if (outputFormat == "Base64")
        {
            var base64String = Convert.ToBase64String(data);
            result = Encoding.UTF8.GetBytes(base64String);
            Console.WriteLine($"Output as Base64: '{base64String}'");
        }
        else // String
        {
            // Try to decode as UTF-8, fallback to Latin-1 if it fails
            try
            {
                var text = Encoding.UTF8.GetString(data);
                result = Encoding.UTF8.GetBytes(text);
                Console.WriteLine($"Output as String: '{text}'");
            }
            catch
            {
                var text = Encoding.Latin1.GetString(data);
                result = Encoding.UTF8.GetBytes(text);
                Console.WriteLine($"Output as String (Latin1): '{text}'");
            }
        }

        return Task.FromResult(result);
    }

    private string GetStringParameter(string key)
    {
        if (!Parameters.TryGetValue(key, out var value))
            return "";

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
                return jsonElement.GetString() ?? "";
            return jsonElement.ToString() ?? "";
        }

        return value?.ToString() ?? "";
    }
}