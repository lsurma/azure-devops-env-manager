namespace AzureDevOpsEnvManager.Models;

public class VariableLibrary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class VariableValue
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string LibraryName { get; set; } = string.Empty;
}
