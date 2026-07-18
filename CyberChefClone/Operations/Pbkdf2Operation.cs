using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class Pbkdf2Operation : IOperation
{
    public string Name => "PBKDF2";
    public string Description => "Generate key from password using PBKDF2 (RFC 2898)";
    public string Category => "Encryption";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["Password"] = "mysecretpassword",
        ["Salt"] = "salt",
        ["Iterations"] = 10000,
        ["KeyLength"] = 32,
        ["Algorithm"] = "SHA-256",
        ["OutputFormat"] = "Hex"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["Password"] = new ParameterInfo
        {
            Type = "string",
            Label = "Password",
            DefaultValue = "mysecretpassword",
            Description = "Password for key generation"
        },
        ["Salt"] = new ParameterInfo
        {
            Type = "string",
            Label = "Salt",
            DefaultValue = "salt",
            Description = "Salt for PBKDF2 (recommended 16+ bytes)"
        },
        ["Iterations"] = new ParameterInfo
        {
            Type = "number",
            Label = "Iterations",
            DefaultValue = 10000,
            Description = "Number of iterations (recommended 10000+)"
        },
        ["KeyLength"] = new ParameterInfo
        {
            Type = "number",
            Label = "Key length",
            DefaultValue = 32,
            Description = "Generated key length in bytes"
        },
        ["Algorithm"] = new ParameterInfo
        {
            Type = "select",
            Label = "Algorithm",
            DefaultValue = "SHA-256",
            Options = new List<string> { "SHA-1", "SHA-256", "SHA-384", "SHA-512" },
            Description = "Hash algorithm for PBKDF2"
        },
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "Hex",
            Options = new List<string> { "Hex", "Base64" },
            Description = "Generated key output format"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        Console.WriteLine($"=== PBKDF2 ExecuteAsync ===");

        // Get parameters
        var password = GetStringParameter("Password");
        var salt = GetStringParameter("Salt");
        var iterations = GetIntParameter("Iterations", 10000);
        var keyLength = GetIntParameter("KeyLength", 32);
        var algorithm = GetStringParameter("Algorithm");
        var outputFormat = GetStringParameter("OutputFormat");

        Console.WriteLine($"Password: '{password}'");
        Console.WriteLine($"Salt: '{salt}'");
        Console.WriteLine($"Iterations: {iterations}");
        Console.WriteLine($"KeyLength: {keyLength}");
        Console.WriteLine($"Algorithm: {algorithm}");
        Console.WriteLine($"OutputFormat: {outputFormat}");

        // Validate parameters
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password is required");

        if (iterations < 1)
            throw new ArgumentException("Iterations must be at least 1");

        if (keyLength < 1)
            throw new ArgumentException("KeyLength must be at least 1");

        // Convert password and salt to bytes
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);

        // Get hash algorithm
        var hashAlgorithm = GetHashAlgorithm(algorithm);

        // Generate key
        using var pbkdf2 = new Rfc2898DeriveBytes(
            passwordBytes,
            saltBytes,
            iterations,
            hashAlgorithm
        );

        var key = pbkdf2.GetBytes(keyLength);

        // Convert based on output format
        byte[] result;
        if (outputFormat == "Base64")
        {
            var base64String = Convert.ToBase64String(key);
            result = Encoding.UTF8.GetBytes(base64String);
        }
        else // Hex (default)
        {
            var hexString = BitConverter.ToString(key).Replace("-", "").ToLower();
            result = Encoding.UTF8.GetBytes(hexString);
        }

        Console.WriteLine($"Generated key length: {key.Length} bytes");
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

    private int GetIntParameter(string key, int defaultValue = 0)
    {
        if (!Parameters.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
                return jsonElement.GetInt32();
            if (jsonElement.ValueKind == JsonValueKind.String)
                return int.TryParse(jsonElement.GetString(), out int result) ? result : defaultValue;
            return defaultValue;
        }

        return Convert.ToInt32(value);
    }

    private HashAlgorithmName GetHashAlgorithm(string algorithm)
    {
        return algorithm?.ToUpper() switch
        {
            "SHA-1" => HashAlgorithmName.SHA1,
            "SHA-256" => HashAlgorithmName.SHA256,
            "SHA-384" => HashAlgorithmName.SHA384,
            "SHA-512" => HashAlgorithmName.SHA512,
            _ => HashAlgorithmName.SHA256
        };
    }
}