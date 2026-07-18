using System.Text;
using System.Text.Json;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class ToHexOperation : IOperation
{
    public string Name => "To Hex";
    public string Description => "Convert input to hexadecimal representation";
    public string Category => "Data manipulation";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["InputFormat"] = "String",
        ["UpperCase"] = false
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["InputFormat"] = new ParameterInfo
        {
            Type = "select",
            Label = "Input format",
            DefaultValue = "String",
            Options = new List<string> { "String", "Base64" },
            Description = "Input data format"
        },
        ["UpperCase"] = new ParameterInfo
        {
            Type = "boolean",
            Label = "Uppercase",
            DefaultValue = false,
            Description = "Use uppercase letters (A-F) for Hex"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var inputFormat = GetStringParameter("InputFormat");
        var upperCase = GetBoolParameter("UpperCase", false);

        byte[] data;
        var inputString = Encoding.UTF8.GetString(input);

        if (inputFormat == "Base64")
        {
            try
            {
                data = Convert.FromBase64String(inputString);
            }
            catch
            {
                throw new ArgumentException("Invalid Base64 input format");
            }
        }
        else
        {
            data = Encoding.UTF8.GetBytes(inputString);
        }

        var hexString = BitConverter.ToString(data).Replace("-", "");
        if (upperCase)
        {
            hexString = hexString.ToUpper();
        }
        else
        {
            hexString = hexString.ToLower();
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(hexString));
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

    private bool GetBoolParameter(string key, bool defaultValue = false)
    {
        if (!Parameters.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(jsonElement.GetString(), out bool result) && result,
                _ => defaultValue
            };
        }

        return value is bool b ? b : defaultValue;
    }
}