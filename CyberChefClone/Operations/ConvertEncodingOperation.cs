using System.Text;
using CyberChefClone.Services;

namespace CyberChefClone.Operations;

public class ConvertEncodingOperation : IOperation
{
    public string Name => "Convert Encoding";
    public string Description => "Convert text between different character encodings";
    public string Category => "Encoding";

    public Dictionary<string, object> Parameters { get; set; } = new()
    {
        ["FromEncoding"] = "UTF-8",
        ["ToEncoding"] = "Windows-1251"
    };

    public Dictionary<string, ParameterInfo> ParameterTypes => new()
    {
        ["FromEncoding"] = new ParameterInfo
        {
            Type = "select",
            Label = "From encoding",
            DefaultValue = "UTF-8",
            Options = new List<string> { "UTF-8", "UTF-16", "UTF-32", "ASCII", "Windows-1251", "Windows-1252", "ISO-8859-1", "KOI8-R" },
            Description = "Input encoding"
        },
        ["ToEncoding"] = new ParameterInfo
        {
            Type = "select",
            Label = "To encoding",
            DefaultValue = "Windows-1251",
            Options = new List<string> { "UTF-8", "UTF-16", "UTF-32", "ASCII", "Windows-1251", "Windows-1252", "ISO-8859-1", "KOI8-R" },
            Description = "Output encoding"
        }
    };

    public Task<byte[]> ExecuteAsync(byte[] input)
    {
        var fromEncoding = ParameterHelper.GetString(Parameters, "FromEncoding", "UTF-8");
        var toEncoding = ParameterHelper.GetString(Parameters, "ToEncoding", "Windows-1251");

        var from = GetEncoding(fromEncoding);
        var to = GetEncoding(toEncoding);

        var text = from.GetString(input);
        var result = to.GetBytes(text);

        return Task.FromResult(result);
    }

    private Encoding GetEncoding(string name)
    {
        return name switch
        {
            "UTF-8" => Encoding.UTF8,
            "UTF-16" => Encoding.Unicode,
            "UTF-32" => Encoding.UTF32,
            "ASCII" => Encoding.ASCII,
            "Windows-1251" => Encoding.GetEncoding("windows-1251"),
            "Windows-1252" => Encoding.GetEncoding("windows-1252"),
            "ISO-8859-1" => Encoding.GetEncoding("iso-8859-1"),
            "KOI8-R" => Encoding.GetEncoding("koi8-r"),
            _ => Encoding.UTF8
        };
    }
}