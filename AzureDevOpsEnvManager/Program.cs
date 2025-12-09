using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AzureDevOpsEnvManager;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Azure DevOps Environment Manager");
        Console.WriteLine("=================================\n");

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var organizationUrl = configuration["AzureDevOps:OrganizationUrl"] ?? 
                             Environment.GetEnvironmentVariable("AZDO_ORG_URL");
        var personalAccessToken = configuration["AzureDevOps:PersonalAccessToken"] ?? 
                                  Environment.GetEnvironmentVariable("AZDO_PAT");
        var projectName = configuration["AzureDevOps:ProjectName"] ?? 
                         Environment.GetEnvironmentVariable("AZDO_PROJECT");

        if (string.IsNullOrEmpty(organizationUrl) || 
            string.IsNullOrEmpty(personalAccessToken) || 
            string.IsNullOrEmpty(projectName))
        {
            Console.WriteLine("Error: Required configuration missing!");
            Console.WriteLine("Please provide either via appsettings.json or environment variables:");
            Console.WriteLine("  - OrganizationUrl (AZDO_ORG_URL)");
            Console.WriteLine("  - PersonalAccessToken (AZDO_PAT)");
            Console.WriteLine("  - ProjectName (AZDO_PROJECT)");
            return;
        }

        var manager = new AzureDevOpsManager(organizationUrl, personalAccessToken, projectName);

        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        try
        {
            var command = args[0].ToLower();

            switch (command)
            {
                case "list-variables":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: list-variables <libraryId>");
                        return;
                    }
                    await manager.ListVariables(int.Parse(args[1]));
                    break;

                case "get-variable":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: get-variable <libraryId> <variableName>");
                        return;
                    }
                    await manager.GetVariable(int.Parse(args[1]), args[2]);
                    break;

                case "update-variable":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Usage: update-variable <libraryId> <variableName> <value>");
                        return;
                    }
                    await manager.UpdateVariable(int.Parse(args[1]), args[2], args[3]);
                    break;

                case "add-variable":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Usage: add-variable <libraryId> <variableName> <value>");
                        return;
                    }
                    await manager.AddVariable(int.Parse(args[1]), args[2], args[3]);
                    break;

                case "list-libraries":
                    await manager.ListLibraries();
                    break;

                case "trigger-pipeline":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: trigger-pipeline <pipelineId> [branch]");
                        return;
                    }
                    var branch = args.Length > 2 ? args[2] : "main";
                    await manager.TriggerPipeline(int.Parse(args[1]), branch);
                    break;

                case "list-pipelines":
                    await manager.ListPipelines();
                    break;

                default:
                    ShowUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  list-libraries                              - List all variable libraries");
        Console.WriteLine("  list-variables <libraryId>                  - List all variables in a library");
        Console.WriteLine("  get-variable <libraryId> <variableName>     - Get a specific variable value");
        Console.WriteLine("  update-variable <libraryId> <name> <value>  - Update a variable");
        Console.WriteLine("  add-variable <libraryId> <name> <value>     - Add a new variable");
        Console.WriteLine("  list-pipelines                              - List all pipelines");
        Console.WriteLine("  trigger-pipeline <pipelineId> [branch]      - Trigger a pipeline (default branch: main)");
    }
}

class AzureDevOpsManager
{
    private readonly VssConnection _connection;
    private readonly string _projectName;

    public AzureDevOpsManager(string organizationUrl, string personalAccessToken, string projectName)
    {
        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        _connection = new VssConnection(new Uri(organizationUrl), credentials);
        _projectName = projectName;
    }

    public async Task ListLibraries()
    {
        Console.WriteLine("Fetching variable libraries...\n");
        
        var taskAgentClient = await _connection.GetClientAsync<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskAgentHttpClient>();
        var variableGroups = await taskAgentClient.GetVariableGroupsAsync(_projectName);

        if (!variableGroups.Any())
        {
            Console.WriteLine("No variable libraries found.");
            return;
        }

        Console.WriteLine($"Found {variableGroups.Count} variable libraries:\n");
        foreach (var group in variableGroups)
        {
            Console.WriteLine($"ID: {group.Id}");
            Console.WriteLine($"Name: {group.Name}");
            Console.WriteLine($"Description: {group.Description}");
            Console.WriteLine($"Variables Count: {group.Variables.Count}");
            Console.WriteLine();
        }
    }

    public async Task ListVariables(int libraryId)
    {
        Console.WriteLine($"Fetching variables from library {libraryId}...\n");
        
        var taskAgentClient = await _connection.GetClientAsync<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskAgentHttpClient>();
        var variableGroup = await taskAgentClient.GetVariableGroupAsync(_projectName, libraryId);

        if (variableGroup.Variables == null || !variableGroup.Variables.Any())
        {
            Console.WriteLine("No variables found in this library.");
            return;
        }

        Console.WriteLine($"Library: {variableGroup.Name}");
        Console.WriteLine($"Variables:\n");
        
        foreach (var variable in variableGroup.Variables)
        {
            var isSecret = variable.Value.IsSecret == true ? " (Secret)" : "";
            var value = variable.Value.IsSecret == true ? "***" : variable.Value.Value;
            Console.WriteLine($"  {variable.Key}: {value}{isSecret}");
        }
    }

    public async Task GetVariable(int libraryId, string variableName)
    {
        Console.WriteLine($"Fetching variable '{variableName}' from library {libraryId}...\n");
        
        var taskAgentClient = await _connection.GetClientAsync<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskAgentHttpClient>();
        var variableGroup = await taskAgentClient.GetVariableGroupAsync(_projectName, libraryId);

        if (variableGroup.Variables.TryGetValue(variableName, out var variable))
        {
            if (variable.IsSecret == true)
            {
                Console.WriteLine($"{variableName}: *** (Secret - value hidden)");
            }
            else
            {
                Console.WriteLine($"{variableName}: {variable.Value}");
            }
        }
        else
        {
            Console.WriteLine($"Variable '{variableName}' not found in library {libraryId}");
        }
    }

    public async Task UpdateVariable(int libraryId, string variableName, string value)
    {
        Console.WriteLine($"Updating variable '{variableName}' in library {libraryId}...\n");
        
        var taskAgentClient = await _connection.GetClientAsync<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskAgentHttpClient>();
        var variableGroup = await taskAgentClient.GetVariableGroupAsync(_projectName, libraryId);

        if (variableGroup.Variables.ContainsKey(variableName))
        {
            variableGroup.Variables[variableName].Value = value;
            var parameters = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableGroupParameters
            {
                Name = variableGroup.Name,
                Description = variableGroup.Description,
                Type = variableGroup.Type,
                Variables = variableGroup.Variables
            };
            await taskAgentClient.UpdateVariableGroupAsync(libraryId, parameters);
            Console.WriteLine($"Successfully updated variable '{variableName}'");
        }
        else
        {
            Console.WriteLine($"Variable '{variableName}' not found. Use 'add-variable' to create it.");
        }
    }

    public async Task AddVariable(int libraryId, string variableName, string value)
    {
        Console.WriteLine($"Adding variable '{variableName}' to library {libraryId}...\n");
        
        var taskAgentClient = await _connection.GetClientAsync<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskAgentHttpClient>();
        var variableGroup = await taskAgentClient.GetVariableGroupAsync(_projectName, libraryId);

        if (variableGroup.Variables.ContainsKey(variableName))
        {
            Console.WriteLine($"Variable '{variableName}' already exists. Use 'update-variable' to modify it.");
            return;
        }

        variableGroup.Variables.Add(variableName, new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableValue
        {
            Value = value,
            IsSecret = false
        });

        var parameters = new Microsoft.TeamFoundation.DistributedTask.WebApi.VariableGroupParameters
        {
            Name = variableGroup.Name,
            Description = variableGroup.Description,
            Type = variableGroup.Type,
            Variables = variableGroup.Variables
        };
        await taskAgentClient.UpdateVariableGroupAsync(libraryId, parameters);
        Console.WriteLine($"Successfully added variable '{variableName}'");
    }

    public async Task ListPipelines()
    {
        Console.WriteLine("Fetching pipelines...\n");
        
        var buildClient = await _connection.GetClientAsync<BuildHttpClient>();
        var definitions = await buildClient.GetDefinitionsAsync(project: _projectName);

        if (!definitions.Any())
        {
            Console.WriteLine("No pipelines found.");
            return;
        }

        Console.WriteLine($"Found {definitions.Count} pipelines:\n");
        foreach (var definition in definitions)
        {
            Console.WriteLine($"ID: {definition.Id}");
            Console.WriteLine($"Name: {definition.Name}");
            Console.WriteLine($"Path: {definition.Path}");
            Console.WriteLine();
        }
    }

    public async Task TriggerPipeline(int pipelineId, string branch)
    {
        Console.WriteLine($"Triggering pipeline {pipelineId} on branch '{branch}'...\n");
        
        var buildClient = await _connection.GetClientAsync<BuildHttpClient>();
        
        var build = new Build
        {
            Definition = new DefinitionReference { Id = pipelineId },
            SourceBranch = $"refs/heads/{branch}"
        };

        var queuedBuild = await buildClient.QueueBuildAsync(build, _projectName);
        
        Console.WriteLine($"Pipeline triggered successfully!");
        Console.WriteLine($"Build ID: {queuedBuild.Id}");
        Console.WriteLine($"Build Number: {queuedBuild.BuildNumber}");
        Console.WriteLine($"Status: {queuedBuild.Status}");
        Console.WriteLine($"URL: {queuedBuild.Links?.Links["web"]?.GetType().GetProperty("Href")?.GetValue(queuedBuild.Links.Links["web"])}");
    }
}
