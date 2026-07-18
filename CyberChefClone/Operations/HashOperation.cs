using System.Security.Cryptography;
using System.Text;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class HashOperation : IOperation
{
    public string Name => "Hash";
    public string Description => "Calculate hash of input using various algorithms";
    public string Category => "Hashing";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["Algorithm"] = "SHA-256",
        ["OutputFormat"] = "Hex"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["Algorithm"] = new ParameterInfo
        {
            Type = "select",
            Label = "Algorithm",
            DefaultValue = "SHA-256",
            Options = new List<string> { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" },
            Description = "Hash algorithm"
        },
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "Hex",
            Options = new List<string> { "Hex", "Base64" },
            Description = "Hash output format"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var algorithm = ParameterHelper.GetString(Parameters, "Algorithm", "SHA-256");
        var format = ParameterHelper.GetString(Parameters, "OutputFormat", "Hex");

        byte[] hash;
        using (var hasher = GetHashAlgorithm(algorithm))
        {
            hash = hasher.ComputeHash(input);
        }

        string result;
        if (format == "Base64")
        {
            result = Convert.ToBase64String(hash);
        }
        else // Hex
        {
            result = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(result));
    }

    private HashAlgorithm GetHashAlgorithm(string name)
    {
        return name switch
        {
            "MD5" => MD5.Create(),
            "SHA-1" => SHA1.Create(),
            "SHA-256" => SHA256.Create(),
            "SHA-384" => SHA384.Create(),
            "SHA-512" => SHA512.Create(),
            _ => SHA256.Create()
        };
    }
}