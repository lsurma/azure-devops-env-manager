using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AzureDevOpsEnvManager.Models;
using AzureDevOpsEnvManager.Services;

namespace AzureDevOpsEnvManager.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AzureDevOpsService _azureDevOpsService;

    public List<PipelineInfo> Pipelines { get; set; } = new();
    public List<VariableLibrary> VariableLibraries { get; set; } = new();
    public List<VariableValue> AllVariables { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    [BindProperty]
    public int VariableGroupId { get; set; }
    
    [BindProperty]
    public string VariableName { get; set; } = string.Empty;
    
    [BindProperty]
    public string VariableValue { get; set; } = string.Empty;

    // Define the specific fields we want to display
    public readonly List<string> ExpectedFields = new()
    {
        "addressFrontIMG",
        "addressMigrationsIMG",
        "addressPanelIMG",
        "addressPerconaIMG",
        "app1Port",
        "app2Port",
        "domena",
        "environment",
        "postgresPort"
    };

    public IndexModel(ILogger<IndexModel> logger, AzureDevOpsService azureDevOpsService)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Pipelines = await _azureDevOpsService.GetPipelinesAsync();
            VariableLibraries = await _azureDevOpsService.GetVariableLibrariesAsync();
            AllVariables = await _azureDevOpsService.GetAllVariablesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
            _logger.LogError(ex, "Error loading Azure DevOps data");
        }
    }
    
    public async Task<IActionResult> OnPostUpdateVariableAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(VariableName))
            {
                ErrorMessage = "Variable name is required.";
                await OnGetAsync();
                return Page();
            }

            var success = await _azureDevOpsService.UpdateVariableAsync(VariableGroupId, VariableName, VariableValue);
            
            if (success)
            {
                SuccessMessage = $"Successfully updated variable '{VariableName}' in variable group.";
                _logger.LogInformation("Updated variable '{VariableName}' in group {VariableGroupId}", VariableName, VariableGroupId);
            }
            else
            {
                ErrorMessage = $"Failed to update variable '{VariableName}'.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating variable: {ex.Message}";
            _logger.LogError(ex, "Error updating variable");
        }
        
        await OnGetAsync();
        return Page();
    }
}
