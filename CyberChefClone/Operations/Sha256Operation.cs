using System.Security.Cryptography;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class Sha256Operation : IOperation
{
    public string Name => "SHA-256 Hash";
    public string Description => "Calculate SHA-256 hash of input";
    public string Category => "Hashing";

    public Dictionary<string, object> Parameters { get; set; } = new();

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(input);
        return Task.FromResult(hash);
    }
}