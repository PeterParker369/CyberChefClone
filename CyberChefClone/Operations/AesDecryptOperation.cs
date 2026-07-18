using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class AesDecryptOperation : IOperation
{
    public string Name => "AES Decrypt";
    public string Description => "Decrypt data using AES algorithm";
    public string Category => "Encryption";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["Key"] = "defaultkey123456",
        ["KeyFormat"] = "UTF-8",
        ["IV"] = "defaultiv12345678",
        ["IVFormat"] = "UTF-8",
        ["InputFormat"] = "Hex",
        ["Mode"] = "CBC",
        ["Padding"] = "PKCS7",
        ["OutputFormat"] = "String"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["Key"] = new ParameterInfo
        {
            Type = "string",
            Label = "Key",
            DefaultValue = "defaultkey123456",
            Description = "Encryption key in selected format"
        },
        ["KeyFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Key format",
            DefaultValue = "UTF-8",
            Options = new List<string> { "UTF-8", "Hex" },
            Description = "Key input format: UTF-8 string or Hex"
        },
        ["IV"] = new ParameterInfo
        {
            Type = "string",
            Label = "IV",
            DefaultValue = "defaultiv12345678",
            Description = "Initialization vector in selected format (16 bytes)"
        },
        ["IVFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "IV format",
            DefaultValue = "UTF-8",
            Options = new List<string> { "UTF-8", "Hex" },
            Description = "IV input format: UTF-8 string or Hex"
        },
        ["InputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Input data format",
            DefaultValue = "Hex",
            Options = new List<string> { "Hex", "Base64", "String" },
            Description = "Input encrypted data format: Hex, Base64 or string (UTF-8)"
        },
        ["Mode"] = new ParameterInfo
        {
            Type = "select",
            Label = "Encryption mode",
            DefaultValue = "CBC",
            Options = new List<string> { "CBC", "ECB", "CFB", "OFB" },
            Description = "Block cipher mode of operation"
        },
        ["Padding"] = new ParameterInfo
        {
            Type = "select",
            Label = "Padding mode",
            DefaultValue = "PKCS7",
            Options = new List<string> { "PKCS7", "Zeros", "ANSIX923", "ISO10126", "None" },
            Description = "Block padding method"
        },
        ["OutputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Output format",
            DefaultValue = "String",
            Options = new List<string> { "Hex", "Base64", "String" },
            Description = "Decrypted data output format"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        Console.WriteLine($"=== AES Decrypt ExecuteAsync ===");

        // Get parameters
        var keyValue = GetStringParameter("Key");
        var keyFormat = GetStringParameter("KeyFormat");
        var ivValue = GetStringParameter("IV");
        var ivFormat = GetStringParameter("IVFormat");
        var inputFormat = GetStringParameter("InputFormat");
        var mode = GetStringParameter("Mode");
        var padding = GetStringParameter("Padding");
        var outputFormat = GetStringParameter("OutputFormat");

        Console.WriteLine($"Key: '{keyValue}', KeyFormat: '{keyFormat}'");
        Console.WriteLine($"IV: '{ivValue}', IVFormat: '{ivFormat}'");
        Console.WriteLine($"InputFormat: '{inputFormat}'");
        Console.WriteLine($"OutputFormat: '{outputFormat}'");

        // Get key
        var key = GetBytesFromString(keyValue, keyFormat);
        if (key.Length == 0)
            throw new ArgumentException("Key is required");

        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            throw new ArgumentException($"Key must be 16, 24, or 32 bytes. Current length: {key.Length}");

        // Get IV
        var iv = GetBytesFromString(ivValue, ivFormat);

        // Convert input data based on format
        byte[] encryptedData;
        var inputString = Encoding.UTF8.GetString(input);

        switch (inputFormat)
        {
            case "Hex":
                var hex = inputString.Replace(" ", "").Replace("-", "");
                if (hex.Length % 2 != 0)
                    throw new ArgumentException("Invalid Hex string length");

                encryptedData = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    encryptedData[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                Console.WriteLine($"InputFormat Hex: {encryptedData.Length} bytes");
                break;
            case "Base64":
                encryptedData = Convert.FromBase64String(inputString);
                Console.WriteLine($"InputFormat Base64: {encryptedData.Length} bytes");
                break;
            default: // String (UTF-8)
                encryptedData = Encoding.UTF8.GetBytes(inputString);
                Console.WriteLine($"InputFormat String: {encryptedData.Length} bytes");
                break;
        }

        var cipherMode = GetCipherMode(mode);
        if (cipherMode != CipherMode.ECB && iv.Length == 0)
            throw new ArgumentException($"IV is required for {mode} mode (16 bytes)");

        using var aes = Aes.Create();
        aes.Key = key;

        if (cipherMode != CipherMode.ECB)
        {
            if (iv.Length > 0 && iv.Length != 16)
                throw new ArgumentException($"IV must be 16 bytes. Current length: {iv.Length}");

            aes.IV = iv.Length == 16 ? iv : new byte[16];
        }

        aes.Mode = cipherMode;
        aes.Padding = GetPaddingMode(padding);

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        // Convert result based on output format
        byte[] result;
        switch (outputFormat)
        {
            case "Hex":
                var hexString = BitConverter.ToString(decrypted).Replace("-", "").ToLower();
                result = Encoding.UTF8.GetBytes(hexString);
                break;
            case "Base64":
                var base64String = Convert.ToBase64String(decrypted);
                result = Encoding.UTF8.GetBytes(base64String);
                break;
            default: // String
                try
                {
                    var str = Encoding.UTF8.GetString(decrypted);
                    result = Encoding.UTF8.GetBytes(str);
                }
                catch
                {
                    result = Encoding.UTF8.GetBytes(Convert.ToBase64String(decrypted));
                }
                break;
        }

        Console.WriteLine($"Decrypted length: {decrypted.Length} bytes");
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

    private byte[] GetBytesFromString(string value, string format)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<byte>();

        if (format == "Hex")
        {
            var hex = value.Replace(" ", "").Replace("-", "");
            if (hex.Length % 2 != 0)
                throw new ArgumentException($"Invalid Hex string length: {hex.Length}");

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        else // UTF-8
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }

    private CipherMode GetCipherMode(string mode)
    {
        return mode.ToUpper() switch
        {
            "ECB" => CipherMode.ECB,
            "CFB" => CipherMode.CFB,
            "OFB" => CipherMode.OFB,
            _ => CipherMode.CBC
        };
    }

    private PaddingMode GetPaddingMode(string padding)
    {
        return padding.ToUpper() switch
        {
            "ZEROS" => PaddingMode.Zeros,
            "ANSIX923" => PaddingMode.ANSIX923,
            "ISO10126" => PaddingMode.ISO10126,
            "NONE" => PaddingMode.None,
            _ => PaddingMode.PKCS7
        };
    }
}