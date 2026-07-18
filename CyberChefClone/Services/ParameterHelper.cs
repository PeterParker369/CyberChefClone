using System.Text;
using System.Text.Json;

namespace CyberChefClone.Services;

public static class ParameterHelper
{
    /// <summary>
    /// Safely gets a parameter as string
    /// </summary>
    public static string GetString(Dictionary<string, object> parameters, string key, string defaultValue = "")
    {
        if (!parameters.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                return jsonElement.GetString() ?? defaultValue;
            }
            return jsonElement.ToString() ?? defaultValue;
        }

        return value?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Safely gets a parameter as integer
    /// </summary>
    public static int GetInt(Dictionary<string, object> parameters, string key, int defaultValue = 0)
    {
        if (!parameters.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.String => int.TryParse(jsonElement.GetString(), out int result) ? result : defaultValue,
                _ => defaultValue
            };
        }

        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Safely gets a parameter as byte (0-255)
    /// </summary>
    public static byte GetByte(Dictionary<string, object> parameters, string key, byte defaultValue = 0)
    {
        if (!parameters.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetByte(),
                JsonValueKind.String => byte.TryParse(jsonElement.GetString(), out byte result) ? result : defaultValue,
                _ => defaultValue
            };
        }

        return Convert.ToByte(value);
    }

    /// <summary>
    /// Safely gets a parameter as bool
    /// </summary>
    public static bool GetBool(Dictionary<string, object> parameters, string key, bool defaultValue = false)
    {
        if (!parameters.TryGetValue(key, out var value))
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

        return Convert.ToBoolean(value);
    }

    /// <summary>
    /// Safely gets a parameter as byte array
    /// </summary>
    public static byte[] GetBytes(Dictionary<string, object> parameters, string key, byte[] defaultValue = null)
    {
        if (!parameters.TryGetValue(key, out var value))
            return defaultValue ?? Array.Empty<byte>();

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                var str = jsonElement.GetString();
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        return Convert.FromBase64String(str);
                    }
                    catch
                    {
                        return Encoding.UTF8.GetBytes(str);
                    }
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                return jsonElement.EnumerateArray()
                    .Select(e => e.GetByte())
                    .ToArray();
            }
        }

        return defaultValue ?? Array.Empty<byte>();
    }
}