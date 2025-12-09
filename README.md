# Azure DevOps Environment Manager

A simple ASP.NET Core Razor Pages application to manage and view Azure DevOps pipelines and variable libraries.

## Features

- **View Available Pipelines**: Display all pipelines in your Azure DevOps project
- **Run Pipelines**: Trigger pipeline execution directly from the web interface
- **Variable Libraries Management**: View all variable libraries with their environment variables
- **Update Variable Values**: Edit and update variable values directly from the web interface
- **Clone Variable Groups as Templates**: Create new variable groups based on existing ones with the same variable names but different values
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
     - Build: Read & Execute (required for running pipelines)
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

## Usage

### Running Pipelines

To run a pipeline:

1. Navigate to the "Available Pipelines" section on the main page
2. Find the pipeline you want to execute
3. Click the **Run** button next to the pipeline
4. Confirm the action in the dialog box
5. The application will trigger the pipeline execution and display a success message with the Run ID

**Note**: Make sure your Personal Access Token has the "Build: Read & Execute" permission to enable pipeline execution.

### Viewing Variable Libraries

The main page displays all variable libraries (environment groups) from your Azure DevOps project. Each library shows its variables in a table format, with the expected fields listed first, followed by any additional variables.

### Updating Variable Values

To update a variable value in an environment group:

1. Navigate to the variable library you want to modify
2. Find the variable you want to update
3. Click the **Edit** button next to the variable
4. Enter the new value in the input field
5. Click **Save** to apply the changes, or **Cancel** to discard them

The application will immediately update the variable in Azure DevOps, and a success message will appear at the top of the page confirming the update.

**Note**: Make sure your Personal Access Token has the "Variable Groups: Read & Manage" permission to enable variable updates.

### Cloning Variable Groups as Templates

To create a new variable group using an existing one as a template:

1. Navigate to the variable library you want to use as a template
2. Click the **Clone as Template** button in the library's header
3. In the modal dialog that appears:
   - Enter a unique name for the new variable group
   - Fill in new values for each variable (or leave blank to keep original values)
4. Click **Create Variable Group** to create the new group

This feature is useful for creating multiple environments (dev, staging, production) with the same variable structure but different values. The new group will have the same variable names and structure as the template, but with your specified values.

**Note**: Make sure your Personal Access Token has the "Variable Groups: Read & Manage" permission to enable creating variable groups.

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

