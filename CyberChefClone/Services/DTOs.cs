namespace CyberChefClone.Services;

public class OperationInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public List<ParameterInfoDto> Parameters { get; set; } = new();
}

public class ParameterInfoDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Label { get; set; }
    public object DefaultValue { get; set; }
    public List<string> Options { get; set; } = new();
    public string Description { get; set; } = "";
}