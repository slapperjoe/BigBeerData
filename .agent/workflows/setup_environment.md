---
description: Set up the local development environment using Aspire
---
# Setup Environment

This workflow will start the Aspire AppHost, which automatically provisions SQL Server and Azurite containers, and runs the application stack.
The Database schema will be automatically migrated on startup.

1. Ensure Docker Desktop or Podman is running.
2. Run the AppHost:
// turbo
3. Start the application stack
   ```bash
   dotnet run --project AppHost/AppHost.csproj
   ```
