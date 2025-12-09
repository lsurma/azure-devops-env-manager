namespace AzureDevOpsEnvManager.Models;

public class PipelineInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
