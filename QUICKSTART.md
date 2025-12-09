# Quick Start Guide

## Setup

1. **Build the application:**
   ```bash
   dotnet build
   ```

2. **Configure credentials (choose one method):**

   **Option A - Configuration file:**
   ```bash
   cd AzureDevOpsEnvManager
   cp appsettings.json.example appsettings.json
   # Edit appsettings.json with your credentials
   ```

   **Option B - Environment variables:**
   ```bash
   export AZDO_ORG_URL="https://dev.azure.com/your-organization"
   export AZDO_PAT="your-personal-access-token"
   export AZDO_PROJECT="your-project-name"
   ```

## Common Commands

### Variable Library Management

```bash
# List all variable libraries
cd AzureDevOpsEnvManager
dotnet run -- list-libraries

# List variables in a library
dotnet run -- list-variables 42

# Get a specific variable
dotnet run -- get-variable 42 MyVariable

# Update a variable
dotnet run -- update-variable 42 MyVariable "new value"

# Add a new variable
dotnet run -- add-variable 42 NewVariable "value"
```

### Pipeline Management

```bash
# List all pipelines
cd AzureDevOpsEnvManager
dotnet run -- list-pipelines

# Trigger a pipeline (default branch: main)
dotnet run -- trigger-pipeline 123

# Trigger a pipeline on specific branch
dotnet run -- trigger-pipeline 123 develop
```

## Creating a Personal Access Token

1. Go to: `https://dev.azure.com/{your-organization}`
2. User Settings → Personal access tokens
3. New Token
4. Required scopes:
   - Build: Read & execute
   - Variable Groups: Read, create, & manage

## Troubleshooting

**"Required configuration missing" error:**
- Ensure either appsettings.json exists or environment variables are set

**Authentication errors:**
- Verify PAT is valid and has required scopes
- Check organization URL format: `https://dev.azure.com/organization-name`

**Permission errors:**
- Ensure your Azure DevOps account has proper permissions in the project
