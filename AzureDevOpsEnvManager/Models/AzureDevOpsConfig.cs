namespace AzureDevOpsEnvManager.Models;

public class AzureDevOpsConfig
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
}
