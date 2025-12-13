# BigBeerData Environment Setup Instructions

These instructions are intended for an AI agent or developer to set up the development environment from scratch.

## Prerequisites

1.  **Docker Desktop** or **Podman** (must be running).
2.  **.NET 8 SDK** (or later).
3.  **SqlPackage** command line tool.
    *   *To install globally:* `dotnet tool install -g Microsoft.SqlPackage`

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
