# .NET 10 upgrade notes

## Scope
- Moved solution targets to `net10.0` and pinned SDK with `global.json`.
- Began migration off legacy packages for Azure Functions isolated worker; set extension bundle guidance in `host.json`.
- Added initial Blazor Web App (currently Interactive Server) project (`WebApp`) as the future UI path; target render mode can move to Interactive Auto once stable packages land.
- Tracking further steps for Aspire/OTEL wiring and EF/package roll-forward.

## Changes applied
- `global.json`: pin SDK `10.0.100` with roll-forward to latest feature.
- `Api/Api.csproj`: target `net10.0`, add `AnalysisLevel`, update Functions worker packages, remove older preview/legacy packages.
- `Api/host.json`: add `extensionBundle` id/version range `[4.*, 5.0.0)`.
- `Client/Client.csproj`: target `net10.0`, add `AnalysisLevel` (keeps WASM client during transition).
- `Shared/Shared.csproj`: target `net10.0`, add `AnalysisLevel`.
- `WebApp/`: Blazor Interactive Server scaffold (`WebApp.csproj`, `Program.cs`, components, minimal layout, wwwroot), upgraded `Microsoft.AspNetCore.Components.WebAssembly.Server` to `10.0.0`.
- `BigBeerData.sln`: added `WebApp` and new `AppHost` projects to the solution.
- `AppHost/`: initial Aspire host wiring `Api` and `WebApp` via `Aspire.Hosting.AppHost` package (version pinned to `8.0.0` — adjust to latest compatible release for .NET 10); `KubernetesClient` pinned to `18.0.13` for advisory remediation.
- OTEL: `Api` and `WebApp` now include OpenTelemetry packages and configuration (OTLP exporter configurable via `OTLP_ENDPOINT`/`Telemetry:OtlpEndpoint`); `Api` includes HTTP client + EF instrumentation; `WebApp` includes ASP.NET Core + HTTP client instrumentation.
- Streaming: `Api/Update` refactored to `HttpResponseData` SSE streaming (no `PushStreamContent`), and `RunUpdate` split for reduced complexity.
- JSON: moved primary deserialization to `System.Text.Json` 9.0 (`Api/FileOps/FileUpload` now uses STJ; `Api` and `Shared` reference STJ 9.0). `Newtonsoft.Json` kept only as a pinned dependency (13.0.3) to satisfy `Microsoft.Azure.NotificationHubs`.
- Security: lifted transitive vulnerabilities by pinning `Azure.Identity` 1.13.1, `Microsoft.Data.SqlClient` 5.2.2, `Microsoft.Extensions.*` 9.0.0 where needed, and `Microsoft.IdentityModel.*` 7.1.2; removed unused `Microsoft.Extensions.Azure` from `Client`.

## Next steps
- Wire Aspire AppHost to host Functions + WebApp, configure OTLP/console exporters.
- Update EF Core/Storage/MudBlazor and other deps to the .NET 10-compatible wave once package versions are finalized.
- Port existing pages and services from `Client` into `WebApp`, decommission WASM when parity is reached; consider moving to Interactive Auto when the package lineup stabilizes.
- Refactor streaming in `Api/Update` to `HttpResponseData` flushes; add OTEL instrumentation for HTTP/Untappd calls.
- Run `dotnet build` with the .NET 10 SDK and adjust package versions based on available feeds.
- Add OTEL instrumentation in Api and WebApp (Aspire host provides pipeline; services still need `AddOpenTelemetry` for HTTP/EF/client traces and metrics).

## WebApp map rewrite plan (agreed)
- Page & data flow: new `Pages/Map.razor` owns lifecycle; loads styles/bounds, runs TileMath in C#, holds loading/error/selection state, renders diagnostics drawer; Map as default route/nav item.
- JS interop redesign: `wwwroot/js/interop.ts` (Vite/TS) compiled to `interop.js`; modules for bootstrap (Mapbox + Deck, resize, init/dispose), layers (map styles, columns/labels, venue drilldown to arcs/pies), telemetry (init/layer rebuild/zoom/error events to .NET). No `window.interop`; use `IJSObjectReference`.
- Blazor-side logic: data shaping stays in C#; interop calls (`InitDeckGL`, `SetMapState`, `UpdateLayers`, `FlyTo`, `DrillIntoVenue`); JSInvokable handlers for resize/zoom/layer-built/error events.
- Telemetry & diagnostics UI: `ActivitySource` around data fetch, TileMath, interop calls, resize, selection/drilldown; bounded in-memory log surfaced via toggleable diagnostics drawer (last N events, averages, last error). OTEL tags include location ID, zoom, counts.
- Styling/bundling: move interop bundle to Vite + TS output `wwwroot/js/interop.js`; switch CSS to Sass entry (shell, map, diagnostics) compiled to `site.css`; keep map container `calc(100vh - 64px)`; load Mapbox CSS in host.
- API base & hosting: read `ApiBaseUrl` from config/env; default `http://localhost:7071/` when running `func start`. Streaming `Update` endpoint stays unchanged.
