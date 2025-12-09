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
    
    [BindProperty]
    public int PipelineId { get; set; }
    
    [BindProperty]
    public int TemplateGroupId { get; set; }
    
    [BindProperty]
    public string NewGroupName { get; set; } = string.Empty;

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
    
    public async Task<IActionResult> OnPostRunPipelineAsync()
    {
        try
        {
            var runId = await _azureDevOpsService.RunPipelineAsync(PipelineId);
            
            if (runId.HasValue)
            {
                SuccessMessage = $"Successfully triggered pipeline. Run ID: {runId.Value}";
                _logger.LogInformation("Triggered pipeline {PipelineId}, Run ID: {RunId}", PipelineId, runId.Value);
            }
            else
            {
                ErrorMessage = $"Failed to trigger pipeline. Please check the logs for details or verify your PAT has 'Build: Read & Execute' permissions.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error triggering pipeline: {ex.Message}";
            _logger.LogError(ex, "Error triggering pipeline");
        }
        
        await OnGetAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostCreateFromTemplateAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewGroupName))
            {
                ErrorMessage = "New group name is required.";
                await OnGetAsync();
                return Page();
            }

            // Parse the form data to get new variable values
            var newValues = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("var_") && key.Length > 4)
                {
                    var variableName = key[4..]; // Remove "var_" prefix
                    var value = Request.Form[key].ToString();
                    newValues[variableName] = value;
                }
            }

            var newGroupId = await _azureDevOpsService.CreateVariableGroupFromTemplateAsync(TemplateGroupId, NewGroupName, newValues);
            
            if (newGroupId.HasValue)
            {
                SuccessMessage = $"Successfully created variable group '{NewGroupName}' (ID: {newGroupId.Value}) from template.";
                _logger.LogInformation("Created variable group '{NewGroupName}' with ID {NewGroupId} from template {TemplateGroupId}", NewGroupName, newGroupId.Value, TemplateGroupId);
            }
            else
            {
                ErrorMessage = $"Failed to create variable group. Please check the logs for details.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating variable group: {ex.Message}";
            _logger.LogError(ex, "Error creating variable group from template");
        }
        
        await OnGetAsync();
        return Page();
    }
}
