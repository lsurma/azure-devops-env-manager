# Azure DevOps Environment Manager

A simple ASP.NET Core Razor Pages application to manage and view Azure DevOps pipelines and variable libraries.

## Features

- **View Available Pipelines**: Display all pipelines in your Azure DevOps project
- **Variable Libraries Management**: View all variable libraries with their environment variables
- **Update Variable Values**: Edit and update variable values directly from the web interface
- **Organized Display**: Shows specific environment variables including:
  - addressFrontIMG
  - addressMigrationsIMG
  - addressPanelIMG
  - addressPerconaIMG
  - app1Port
  - app2Port
  - domena
  - environment
  - postgresPort

## Prerequisites

- .NET 8.0 SDK or later
- Azure DevOps account with access to your organization and project
- Personal Access Token (PAT) with appropriate permissions

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/lsurma/azure-devops-env-manager.git
   cd azure-devops-env-manager
   ```

2. Configure your Azure DevOps settings in `AzureDevOpsEnvManager/appsettings.json`:
   ```json
   {
     "AzureDevOps": {
       "OrganizationUrl": "https://dev.azure.com/your-organization",
       "PersonalAccessToken": "your-pat-token",
       "ProjectName": "your-project"
     }
   }
   ```

   **Important**: For production, use User Secrets or Environment Variables instead of storing the PAT in appsettings.json:
   
   ```bash
   cd AzureDevOpsEnvManager
   dotnet user-secrets set "AzureDevOps:PersonalAccessToken" "your-pat-token"
   ```

3. Generate a Personal Access Token (PAT):
   - Go to Azure DevOps → User Settings → Personal Access Tokens
   - Create a new token with the following scopes:
     - Build: Read
     - Variable Groups: Read & Manage (required for updating variables)

## Running the Application

1. Navigate to the project directory:
   ```bash
   cd AzureDevOpsEnvManager
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## Project Structure

```
AzureDevOpsEnvManager/
├── Models/
│   ├── AzureDevOpsConfig.cs      # Configuration model
│   ├── PipelineInfo.cs            # Pipeline information model
│   └── VariableLibrary.cs         # Variable library models
├── Services/
│   └── AzureDevOpsService.cs      # Azure DevOps API client
├── Pages/
│   ├── Index.cshtml               # Main page view
│   ├── Index.cshtml.cs            # Main page logic
│   └── Shared/
│       └── _Layout.cshtml         # Layout template
├── appsettings.json               # Application configuration
└── Program.cs                     # Application entry point
```

## Security Considerations

- Never commit your Personal Access Token to source control
- Use User Secrets for local development
- Use Azure Key Vault or similar for production environments
- Regularly rotate your PAT tokens
- Give your PAT only the minimum required permissions

## Troubleshooting

If you encounter authentication issues:
1. Verify your PAT is valid and not expired
2. Check that your PAT has the correct permissions
3. Ensure the Organization URL and Project Name are correct
4. Check the application logs for detailed error messages

