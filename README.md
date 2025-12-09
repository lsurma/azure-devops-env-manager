# Azure DevOps Environment Manager

A simple .NET console application that connects to Azure DevOps using a Personal Access Token (PAT) to manage variable libraries and trigger pipelines.

## Features

- **Variable Library Management**
  - List all variable libraries in a project
  - List all variables in a specific library
  - Get a specific variable value
  - Update existing variables
  - Add new variables to a library

- **Pipeline Management**
  - List all pipelines in a project
  - Trigger pipeline builds with branch selection

## Prerequisites

- .NET 8.0 SDK or later
- Azure DevOps account with appropriate permissions
- Personal Access Token (PAT) with the following scopes:
  - Build (Read & Execute)
  - Variable Groups (Read, Create, & Manage)

## Setup

### 1. Clone the Repository

```bash
git clone https://github.com/lsurma/azure-devops-env-manager.git
cd azure-devops-env-manager
```

### 2. Build the Application

```bash
cd AzureDevOpsEnvManager
dotnet build
```

### 3. Configure Connection Settings

You can configure the application using either a configuration file or environment variables.

#### Option A: Using Configuration File

1. Copy the example configuration file:
   ```bash
   cp appsettings.json.example appsettings.json
   ```

2. Edit `appsettings.json` and provide your Azure DevOps details:
   ```json
   {
     "AzureDevOps": {
       "OrganizationUrl": "https://dev.azure.com/your-organization",
       "PersonalAccessToken": "your-pat-token-here",
       "ProjectName": "your-project-name"
     }
   }
   ```

#### Option B: Using Environment Variables

Set the following environment variables:

```bash
export AZDO_ORG_URL="https://dev.azure.com/your-organization"
export AZDO_PAT="your-pat-token-here"
export AZDO_PROJECT="your-project-name"
```

## Usage

Run the application from the project directory:

```bash
dotnet run -- <command> [arguments]
```

### Available Commands

#### List Variable Libraries
```bash
dotnet run -- list-libraries
```
Lists all variable libraries in the configured project.

#### List Variables in a Library
```bash
dotnet run -- list-variables <libraryId>
```
Lists all variables in the specified library. Secret values are hidden.

**Example:**
```bash
dotnet run -- list-variables 42
```

#### Get a Specific Variable
```bash
dotnet run -- get-variable <libraryId> <variableName>
```
Retrieves the value of a specific variable from a library.

**Example:**
```bash
dotnet run -- get-variable 42 DatabaseConnectionString
```

#### Update a Variable
```bash
dotnet run -- update-variable <libraryId> <variableName> <value>
```
Updates an existing variable in the library.

**Example:**
```bash
dotnet run -- update-variable 42 ApiUrl "https://api.example.com"
```

#### Add a New Variable
```bash
dotnet run -- add-variable <libraryId> <variableName> <value>
```
Adds a new variable to the library.

**Example:**
```bash
dotnet run -- add-variable 42 NewVariable "newValue"
```

#### List Pipelines
```bash
dotnet run -- list-pipelines
```
Lists all pipelines in the configured project.

#### Trigger a Pipeline
```bash
dotnet run -- trigger-pipeline <pipelineId> [branch]
```
Triggers a pipeline build. If branch is not specified, defaults to "main".

**Examples:**
```bash
# Trigger on default branch (main)
dotnet run -- trigger-pipeline 123

# Trigger on specific branch
dotnet run -- trigger-pipeline 123 develop
```

## Creating a Personal Access Token

1. Go to Azure DevOps: `https://dev.azure.com/{your-organization}`
2. Click on your profile icon (top right) → **Personal access tokens**
3. Click **+ New Token**
4. Set a name for your token (e.g., "AzureDevOps EnvManager")
5. Select the required scopes:
   - **Build**: Read & execute
   - **Variable Groups**: Read, create, & manage
6. Click **Create**
7. **Important**: Copy the token immediately - it won't be shown again!

## Security Notes

- **Never commit** your `appsettings.json` file with real credentials to version control
- The `.gitignore` file is configured to exclude `appsettings.json`
- Always use the example file (`appsettings.json.example`) as a template
- Consider using environment variables in CI/CD environments
- Rotate your PAT regularly for security

## Building for Release

To create a release build:

```bash
dotnet publish -c Release -o publish
```

The compiled application will be in the `publish` directory.

## Troubleshooting

### "Required configuration missing" Error
Ensure you have properly configured either the `appsettings.json` file or set the environment variables.

### Authentication Errors
- Verify your PAT is valid and hasn't expired
- Ensure your PAT has the required scopes
- Check that your organization URL is correct (should be `https://dev.azure.com/your-organization`)

### Permission Errors
Ensure your Azure DevOps account has the necessary permissions in the project:
- For variable libraries: Contributor access to the variable group
- For pipelines: Queue builds permission

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

