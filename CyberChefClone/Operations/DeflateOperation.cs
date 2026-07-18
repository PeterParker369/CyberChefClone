using System.IO.Compression;
using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class DeflateOperation : IOperation
{
    public string Name => "Deflate";
    public string Description => "Compress data using Deflate algorithm (RFC 1951)";
    public string Category => "Compression";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["InputFormat"] = "String",
        ["OutputFormat"] = "Base64",
        ["CompressionLevel"] = "Optimal"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["InputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Input format",
            DefaultValue = "String",
            Options = new List<string> { "String", "Base64", "Hex" },
            Description = "Input data format"
        },
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "Base64",
            Options = new List<string> { "Base64", "Hex" },
            Description = "Compressed data output format"
        },
        ["CompressionLevel"] = new ParameterInfo
        {
            Type = "select",
            Label = "Compression level",
            DefaultValue = "Optimal",
            Options = new List<string> { "Fastest", "Optimal", "NoCompression" },
            Description = "Compression level: Fastest, Optimal, or NoCompression"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        Console.WriteLine($"=== Deflate ExecuteAsync ===");

        var inputFormat = GetStringParameter("InputFormat");
        var outputFormat = GetStringParameter("OutputFormat");
        var compressionLevelStr = GetStringParameter("CompressionLevel");

        // Get data to compress
        byte[] dataToCompress;
        var inputString = Encoding.UTF8.GetString(input);

        switch (inputFormat)
        {
            case "Hex":
                var hex = inputString.Replace(" ", "").Replace("-", "");
                if (hex.Length % 2 != 0)
                    throw new ArgumentException("Invalid Hex string length");

                dataToCompress = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    dataToCompress[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                Console.WriteLine($"InputFormat Hex: {dataToCompress.Length} bytes");
                break;
            case "Base64":
                dataToCompress = Convert.FromBase64String(inputString);
                Console.WriteLine($"InputFormat Base64: {dataToCompress.Length} bytes");
                break;
            default: // String
                dataToCompress = Encoding.UTF8.GetBytes(inputString);
                Console.WriteLine($"InputFormat String: {dataToCompress.Length} bytes");
                break;
        }

        // Compress data
        var compressionLevel = GetCompressionLevel(compressionLevelStr);
        byte[] compressedData;

        using (var outputStream = new MemoryStream())
        {
            using (var deflateStream = new DeflateStream(outputStream, compressionLevel, true))
            {
                deflateStream.Write(dataToCompress, 0, dataToCompress.Length);
            }
            compressedData = outputStream.ToArray();
        }

        Console.WriteLine($"Compressed: {compressedData.Length} bytes (was {dataToCompress.Length})");

        // Convert based on output format
        byte[] result;
        if (outputFormat == "Hex")
        {
            var hexString = BitConverter.ToString(compressedData).Replace("-", "").ToLower();
            result = Encoding.UTF8.GetBytes(hexString);
        }
        else // Base64 (default)
        {
            var compressedBase64 = Convert.ToBase64String(compressedData);
            result = Encoding.UTF8.GetBytes(compressedBase64);
        }

        return Task.FromResult(result);
    }

    private CompressionLevel GetCompressionLevel(string level)
    {
        return level?.ToLower() switch
        {
            "fastest" => CompressionLevel.Fastest,
            "optimal" => CompressionLevel.Optimal,
            "nocompression" => CompressionLevel.NoCompression,
            _ => CompressionLevel.Optimal
        };
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