# Copilot Instructions for BigBeerData

Quick context
- Mono-repo .NET 8 solution `BigBeerData.sln` with two runnable apps:
  - `Api/` — Azure Functions (isolated worker) that scrapes Untappd and writes to SQL Server (`Api/Update.cs`).
  - `Client/` — Blazor WebAssembly UI (references `Shared/`).
- Active intent: Port the `Client` UI into the `WebApp` Blazor app (with `WebApp.Client` for client-side components). Favor changes that move UI/API integration and shared components toward `WebApp`/`WebApp.Client` rather than further investing in the legacy `Client` project.

Primary goals for an AI coding agent
- Understand data flow: Untappd API -> `Api` function -> `Shared.BigBeerContext` (SQL Server) -> `Client` reads data via HTTP.
- Preserve EF Core model constraints defined in `Shared/BigBeerContext.cs` (computed columns, relationships).
- Keep Azure Functions streaming behavior intact: `Update` streams Server-Sent Events (see `Api/Update.cs`).

How to build & run locally (explicit)
- Build solution: `dotnet build BigBeerData.sln`
- Run the Functions host (Api): install Azure Functions Core Tools, then from `Api/` run:
  - `func start` or run via Visual Studio using the solution.
  - Ensure environment variables are set: `DBConnection`, `client_id`, `client_secret` (used by `Api/Program.cs` and `Api/Update.cs`).
- Run the Client (Blazor WASM dev server):
  - `dotnet run --project Client` (uses `Microsoft.AspNetCore.Components.WebAssembly.DevServer` in development)

Database / EF Core notes
- Models and DbContext live in `Shared/BigBeerContext.cs`. A design-time factory `BigBeerDatatFactory` is present for `dotnet ef` commands.
- Local dev connection string in the factory targets LocalDB; production uses `DBConnection` env var.
- To add/run migrations (use the design-time factory):
  - `dotnet ef migrations add <Name> --project Shared --startup-project Api`
  - `dotnet ef database update --project Shared --startup-project Api`

Important environment variables and secrets
- `DBConnection` — full SQL Server connection string used by `Api` at runtime.
- `client_id` and `client_secret` — credentials for Untappd calls (lookups in `Api/Update.cs`).
- When running locally, set these in your shell or in `local.settings.json` if using `func`.

Project-specific patterns and conventions
- Streaming updates: `Api/Update.cs` uses `PushStreamContent` to stream SSE lines (`data: ...`) — preserve the streaming loop and incremental writes when refactoring.
- Checkin processing: `ProcessCheckins` writes to DB synchronously inside a `Task.Run` call — be cautious when changing save semantics; deduplication relies on `CheckinTime`/IDs.
- Shared models include computed columns (see `Beer.BaseStyle` computed SQL) and explicit FK config in `OnModelCreating` — prefer model-first changes via EF migrations.
- Client UI uses `MudBlazor` (see `Client/Client.csproj`) and registers a `BrowserService` in `Client/Program.cs` — inspect `Client/Services/BrowserService.cs` for HTTP patterns.

External integrations
- Untappd API: base URL `https://api.untappd.com/v4/` configured in `Api/Program.cs` under the `BeerBot` HttpClient.
- Client includes Azure Storage packages (blobs/queues/files) — check `Client/Client.csproj` for versions and usage.

Files to inspect first when making changes
- `Api/Update.cs` — scraping, streaming, DB writes.
- `Api/Program.cs` — Functions host setup and HttpClient registration.
- `Shared/BigBeerContext.cs` — models, relationships, computed columns, design-time factory.
- `Client/Program.cs` and `Client/Services/BrowserService.cs` — client startup and HTTP integration.
- `Shared/Migrations/` — existing EF migrations; review before adding migrations.

When proposing changes
- If altering DB schema, include EF migration steps and don't change models without migration files.
- If changing streaming endpoints, keep SSE format (`data:` lines) to avoid breaking existing consumers.
- Include explicit commands to reproduce your change locally (build + run instructions above).

If any section is unclear or you need access to runtime secrets, tell me which part to expand or which env values you can share for a deeper runbook.
