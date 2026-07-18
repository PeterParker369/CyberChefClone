using System.IO.Compression;
using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class InflateOperation : IOperation
{
    public string Name => "Inflate";
    public string Description => "Decompress data using Deflate algorithm (RFC 1951)";
    public string Category => "Compression";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["InputFormat"] = "Base64",
        ["OutputFormat"] = "String"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["InputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Input format",
            DefaultValue = "Base64",
            Options = new List<string> { "Base64", "Hex" },
            Description = "Compressed data input format"
        },
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "String",
            Options = new List<string> { "String", "Base64", "Hex" },
            Description = "Decompressed data output format"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        Console.WriteLine($"=== Inflate ExecuteAsync ===");

        var inputFormat = GetStringParameter("InputFormat");
        var outputFormat = GetStringParameter("OutputFormat");

        // Get compressed data
        byte[] compressedData;
        var inputString = Encoding.UTF8.GetString(input);

        switch (inputFormat)
        {
            case "Hex":
                var hex = inputString.Replace(" ", "").Replace("-", "");
                if (hex.Length % 2 != 0)
                    throw new ArgumentException("Invalid Hex string length");

                compressedData = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    compressedData[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                Console.WriteLine($"InputFormat Hex: {compressedData.Length} bytes");
                break;
            default: // Base64
                compressedData = Convert.FromBase64String(inputString);
                Console.WriteLine($"InputFormat Base64: {compressedData.Length} bytes");
                break;
        }

        // Decompress data
        byte[] decompressedData;

        using (var inputStream = new MemoryStream(compressedData))
        using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            deflateStream.CopyTo(outputStream);
            decompressedData = outputStream.ToArray();
        }

        Console.WriteLine($"Decompressed: {decompressedData.Length} bytes");

        // Convert based on output format
        byte[] result;
        switch (outputFormat)
        {
            case "Hex":
                var hexString = BitConverter.ToString(decompressedData).Replace("-", "").ToLower();
                result = Encoding.UTF8.GetBytes(hexString);
                break;
            case "Base64":
                var decompressedBase64 = Convert.ToBase64String(decompressedData);
                result = Encoding.UTF8.GetBytes(decompressedBase64);
                break;
            default: // String
                // Try to decode as UTF-8
                try
                {
                    var text = Encoding.UTF8.GetString(decompressedData);
                    result = Encoding.UTF8.GetBytes(text);
                }
                catch
                {
                    // If it fails, output as Base64
                    var fallbackBase64 = Convert.ToBase64String(decompressedData);
                    result = Encoding.UTF8.GetBytes(fallbackBase64);
                }
                break;
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