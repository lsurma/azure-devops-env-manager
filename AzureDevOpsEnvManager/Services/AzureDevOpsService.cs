using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AzureDevOpsEnvManager.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AzureDevOpsEnvManager.Services;

public class AzureDevOpsService : IDisposable
{
    private readonly VssConnection _connection;
    private readonly string _projectName;
    private readonly HttpClient _httpClient;
    private readonly string _organizationUrl;
    private readonly string _personalAccessToken;
    private readonly ILogger<AzureDevOpsService> _logger;
    private bool _disposed = false;

    public AzureDevOpsService(string organizationUrl, string personalAccessToken, string projectName, ILogger<AzureDevOpsService> logger)
    {
        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        _connection = new VssConnection(new Uri(organizationUrl), credentials);
        _projectName = projectName;
        _organizationUrl = organizationUrl;
        _personalAccessToken = personalAccessToken;
        _logger = logger;
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));
    }

    public async Task<List<PipelineInfo>> GetPipelinesAsync()
    {
        try
        {
            var url = $"{_organizationUrl}/{_projectName}/_apis/build/definitions?api-version=7.1-preview.7";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching pipelines: {StatusCode}", response.StatusCode);
                return new List<PipelineInfo>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var pipelines = new List<PipelineInfo>();

            if (jsonDoc.RootElement.TryGetProperty("value", out var valueArray))
            {
                foreach (var item in valueArray.EnumerateArray())
                {
                    pipelines.Add(new PipelineInfo
                    {
                        Id = item.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                        Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                        Path = item.TryGetProperty("path", out var path) ? path.GetString() ?? "" : "",
                        Type = item.TryGetProperty("type", out var type) ? type.GetString() ?? "" : ""
                    });
                }
            }

            return pipelines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pipelines");
            return new List<PipelineInfo>();
        }
    }

    public async Task<List<VariableLibrary>> GetVariableLibrariesAsync()
    {
        try
        {
            var taskClient = await _connection.GetClientAsync<TaskAgentHttpClient>();
            var variableGroups = await taskClient.GetVariableGroupsAsync(project: _projectName);

            var libraries = new List<VariableLibrary>();
            foreach (var group in variableGroups)
            {
                var library = new VariableLibrary
                {
                    Id = group.Id,
                    Name = group.Name,
                    Variables = group.Variables.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Value ?? string.Empty
                    )
                };
                libraries.Add(library);
            }

            return libraries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching variable libraries");
            return new List<VariableLibrary>();
        }
    }

    public async Task<List<Models.VariableValue>> GetAllVariablesAsync()
    {
        var libraries = await GetVariableLibrariesAsync();
        var allVariables = new List<Models.VariableValue>();

        foreach (var library in libraries)
        {
            foreach (var variable in library.Variables)
            {
                allVariables.Add(new Models.VariableValue
                {
                    Name = variable.Key,
                    Value = variable.Value,
                    LibraryName = library.Name
                });
            }
        }

        return allVariables;
    }

    public async Task<bool> UpdateVariableAsync(int variableGroupId, string variableName, string newValue)
    {
        try
        {
            var taskClient = await _connection.GetClientAsync<TaskAgentHttpClient>();
            
            // Get the current variable group
            var variableGroup = await taskClient.GetVariableGroupAsync(project: _projectName, groupId: variableGroupId);
            
            if (variableGroup == null)
            {
                _logger.LogWarning("Variable group with ID {VariableGroupId} not found", variableGroupId);
                return false;
            }

            // Update or add the variable
            if (variableGroup.Variables.ContainsKey(variableName))
            {
                variableGroup.Variables[variableName].Value = newValue;
            }
            else
            {
                variableGroup.Variables[variableName] = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableValue
                {
                    Value = newValue
                };
            }

            // Create VariableGroupParameters for update
            var parameters = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableGroupParameters
            {
                Name = variableGroup.Name,
                Description = variableGroup.Description,
                Variables = variableGroup.Variables,
                Type = variableGroup.Type,
                VariableGroupProjectReferences = variableGroup.VariableGroupProjectReferences
            };

            // Update the variable group
            var updatedGroup = await taskClient.UpdateVariableGroupAsync(variableGroupId, parameters);
            
            return updatedGroup != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating variable {VariableName} in group {VariableGroupId}", variableName, variableGroupId);
            return false;
        }
    }

    public async Task<int?> RunPipelineAsync(int pipelineId)
    {
        try
        {
            var url = $"{_organizationUrl}/{_projectName}/_apis/pipelines/{pipelineId}/runs?api-version=7.1-preview.1";
            
            // Create the request body with empty resources (minimum required for pipeline run)
            // Azure DevOps API requires 'resources' and 'templateParameters' properties even if empty
            // to queue a run with default branch and parameters
            var requestBody = new
            {
                resources = new { },
                templateParameters = new { }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error running pipeline {PipelineId}: {StatusCode} - {Error}", pipelineId, response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            if (jsonDoc.RootElement.TryGetProperty("id", out var runId))
            {
                return runId.GetInt32();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline {PipelineId}", pipelineId);
            return null;
        }
    }

    public async Task<int?> CreateVariableGroupFromTemplateAsync(int templateGroupId, string newGroupName, Dictionary<string, string> newValues)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(newGroupName))
            {
                _logger.LogWarning("New group name cannot be null or empty");
                return null;
            }

            var taskClient = await _connection.GetClientAsync<TaskAgentHttpClient>();
            
            // Get the template variable group
            var templateGroup = await taskClient.GetVariableGroupAsync(project: _projectName, groupId: templateGroupId);
            
            if (templateGroup == null)
            {
                _logger.LogWarning("Template variable group with ID {VariableGroupId} not found", templateGroupId);
                return null;
            }

            // Create new variables dictionary with same keys but new values
            var newVariables = new Dictionary<string, Microsoft.TeamFoundation.DistributedTask.WebApi.VariableValue>();
            
            foreach (var variable in templateGroup.Variables)
            {
                var newValue = newValues.TryGetValue(variable.Key, out var value) && !string.IsNullOrEmpty(value) 
                    ? value 
                    : variable.Value.Value ?? string.Empty;
                newVariables[variable.Key] = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableValue
                {
                    Value = newValue,
                    IsSecret = variable.Value.IsSecret
                };
            }

            // Create new variable group parameters
            var parameters = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableGroupParameters
            {
                Name = newGroupName,
                Description = $"Created from template: {templateGroup.Name}",
                Variables = newVariables,
                Type = templateGroup.Type
            };

            // Add the variable group
            var newGroup = await taskClient.AddVariableGroupAsync(parameters);
            
            if (newGroup != null)
            {
                _logger.LogInformation("Created new variable group '{NewGroupName}' (ID: {NewGroupId}) from template '{TemplateGroupName}' (ID: {TemplateGroupId})", 
                    newGroupName, newGroup.Id, templateGroup.Name, templateGroupId);
                return newGroup.Id;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating variable group from template {TemplateGroupId}", templateGroupId);
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
