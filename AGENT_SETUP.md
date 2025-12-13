# BigBeerData Environment Setup Instructions

These instructions are intended for an AI agent or developer to set up the development environment from scratch.

## Tooling Prerequisites

Ensure the following tools and versions are installed. Versions listed are those used in the reference environment.

| Tool | Version | Installation |
| :--- | :--- | :--- |
| **.NET SDK** | 10.0.100 | [Download .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **Node.js** | v18.19.1 | [Download Node.js](https://nodejs.org/) (LTS recommended) |
| **Docker** | 28.2.2 | [Get Docker](https://docs.docker.com/get-docker/) |
| **Azure Functions Core Tools** | 4.5.0 | `npm install -g azure-functions-core-tools@4` |
| **Azurite** | Latest | [Docker Image](https://hub.docker.com/_/microsoft-azure-storage-azurite) (via `docker-compose`) |
| **SqlPackage** | Latest | `dotnet tool install -g Microsoft.SqlPackage` |

> [!NOTE]
> **SqlPackage Requirement**: The `Microsoft.SqlPackage` tool requires the **.NET 8 Runtime** to execute, even if you have the .NET 10 SDK installed.
> If you encounter exit code 150, please install the [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

## Configuration & Secrets

The application requires specific configuration to function fully.

### Environment Variables / Secrets
The **Api** requires the following secrets (e.g., set via User Secrets or Environment Variables):

| Variable | Description |
| :--- | :--- |
| `UNTAPPD_CLIENT_ID` | Client ID for Untappd API integration. |
| `UNTAPPD_CLIENT_SECRET` | Client Secret for Untappd API integration. |
| `UNTAPPD_BASE_URL` | (Optional) Base URL for BeerBot/Untappd. |

### Connection Strings
Managed by `AppHost` but required for isolated runs:
*   `bigbeerdb`: SQL Server (localhost, 1433).
*   `AzureWebJobsStorage`: Azurite (UseDevelopmentStorage=true).

## Setup Steps

### 1. Start Infrastructure
Start the SQL Server and Azurite (Blob Storage) containers using Docker Compose.

```bash
docker-compose up -d
```

*Wait for the containers to be healthy (approx 10-15 seconds).*

### 2. Deploy Database Schema
Deploy the provided `.dacpac` file to the running SQL Server container.

```bash
sqlpackage /Action:Publish \
  /SourceFile:BigBeerData.Core.dacpac \
  /TargetConnectionString:"Data Source=localhost,1433;Initial Catalog=BigBeerData.Core;User Id=sa;Password=BigBeerStrong2024;TrustServerCertificate=True"
```

### 3. Run Application

**Option A: Command Line**
Start the application using the Aspire AppHost.
```bash
dotnet run --project BigBeerData.AppHost/BigBeerData.AppHost.csproj
```

**Option B: VS Code Task**
1.  Open the Command Palette (`Ctrl+Shift+P`).
2.  Type **"Tasks: Run Task"**.
3.  Select **"Start AppHost"**.

The Dashboard will launch, providing links to the API and WebApp endpoints.

## Troubleshooting & Common "Headaches"

The following issues were encountered during development and may recur if the setup deviates from the standard configuration.

### 1. Build Failures in `Api` (OpenTelemetry)
*   **Symptom**: `NU1605: Detected package downgrade: OpenTelemetry...`.
*   **Cause**: The `Api` project references older OpenTelemetry packages (e.g., 1.9.0) than the shared `ServiceDefaults` project (which requires >= 1.13.1).
*   **Fix**: Update all `OpenTelemetry.*` package references in `Api/Api.csproj` to match the version used in `BigBeerData.ServiceDefaults` (currently **1.13.1**).

### 2. Aspire Startup Crashes (`CliPath` / DCP Errors)
*   **Symptom**: `AppHost` starts but immediately crashes with `Property CliPath... is required`.
*   **Cause**: Missing Aspire Dashboard binaries (DCP) for the current runtime/OS combination. This often happens if only `Aspire.Hosting.AppHost` is referenced without the full SDK workload or correct metapackages.
*   **Fix**: Ensure you have run `aspire init` or are using the correct `Aspire.Hosting.AppHost` version (e.g., 13.0.2) that aligns with your .NET SDK.

### 3. Invalid Aspire Resource Names
*   **Symptom**: `ArgumentException` when starting AppHost.
*   **Cause**: Using dots or special characters in resource names (e.g., `builder.AddConnectionString("BigBeerData.Core")`).
*   **Fix**: Rename resources to use only alphanumeric characters and hyphens (e.g., use **"bigbeerdb"**).

### 4. Azure Functions Storage
*   **Symptom**: The API functions host fails to start or connect to storage.
*   **Cause**: Missing or incorrect connection string for Azurite.
*   **Fix**: Ensure `AppHost.cs` injects the **"AzureWebJobsStorage"** connection string pointing to `"UseDevelopmentStorage=true"` (or the active Azurite instance).

### 5. CORS Errors (Blazor WebApp)
*   **Symptom**: WebApp loads but fails to fetch data from the API (Network Error / CORS).
*   **Cause**: The API is not configured to allow requests from the WebApp's origin.
*   **Fix**: Ensure the `Api` project's CORS policy includes the `ASPNETCORE_URLS` of the `WebApp` (e.g., `http://localhost:5000`).

### 6. API Silent Failure (No Endpoints / No Logs)
*   **Symptom**: The `Api` project starts (exit code 0 or running), but no endpoints appear in the Dashboard, and no functions are listed in the console output.
*   **Cause**:
    1.  **Missing `AzureWebJobsStorage`**: The Isolated Worker runtime requires a valid connection string to manage triggers and leases. If missing or invalid, the host waits indefinitely without error.
    2.  **Missing `FunctionsMetadataGenerator`**: The build process failed to generate the function metadata.
*   **Fix**:
    *   Verify `AppHost.cs` injects `builder.AddConnectionString("AzureWebJobsStorage")`.
    *   Ensure the `Api` project has a valid `local.settings.json` (even if empty `{ "IsEncrypted": false, "Values": {...} }`) if running locally without AppHost.
    *   **Crucial**: Make sure the Azure Storage Emulator (Azurite) is actually running (`docker ps`) and accessible.
