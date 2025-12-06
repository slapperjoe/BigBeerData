# .NET 10 upgrade notes

## Scope
- Moved solution targets to `net10.0` and pinned SDK with `global.json`.
- Began migration off legacy packages for Azure Functions isolated worker; set extension bundle guidance in `host.json`.
- Added initial Blazor Web App (Interactive Auto) project (`WebApp`) as the future UI path.
- Tracking further steps for Aspire/OTEL wiring and EF/package roll-forward.

## Changes applied
- `global.json`: pin SDK `10.0.100-preview.1` with roll-forward to latest feature.
- `Api/Api.csproj`: target `net10.0`, add `AnalysisLevel`, update Functions worker packages, remove older preview/legacy packages.
- `Api/host.json`: add `extensionBundle` id/version range `[4.*, 5.0.0)`.
- `Client/Client.csproj`: target `net10.0`, add `AnalysisLevel` (keeps WASM client during transition).
- `Shared/Shared.csproj`: target `net10.0`, add `AnalysisLevel`.
- `WebApp/`: new Blazor Interactive Auto app scaffold (`WebApp.csproj`, `Program.cs`, components, minimal layout, wwwroot).
- `BigBeerData.sln`: added `WebApp` and new `AppHost` projects to the solution.
- `AppHost/`: initial Aspire host wiring `Api` and `WebApp` via `Aspire.Hosting.AppHost` package (version pinned to `8.0.0` — adjust to latest compatible release for .NET 10).

## Next steps
- Wire Aspire AppHost to host Functions + WebApp, configure OTLP/console exporters.
- Update EF Core/Storage/MudBlazor and other deps to the .NET 10-compatible wave once package versions are finalized.
- Port existing pages and services from `Client` into `WebApp`, decommission WASM when parity is reached.
- Refactor streaming in `Api/Update` to `HttpResponseData` flushes; add OTEL instrumentation for HTTP/Untappd calls.
- Run `dotnet build` with the .NET 10 SDK and adjust package versions based on available feeds.
- Add OTEL instrumentation in Api and WebApp (Aspire host provides pipeline; services still need `AddOpenTelemetry` for HTTP/EF/client traces and metrics).
