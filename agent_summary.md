# Agent Summary: BigBeerData

This document provides a high-level technical overview for AI agents working on this codebase.

## Architecture

*   **Type**: Monorepo-style .NET Solution.
*   **Pattern**: Blazor WebAssembly Frontend + Azure Functions Backend (BFF/Serverless pattern).

## Components

### 1. Api (`/Api`)
*   **Type**: Azure Functions (Isolated Worker).
*   **Role**: Backend API & Background Jobs.
*   **Key Files**:
    *   `Update.cs`: HTTP Trigger function. Performs the heavy lifting of scraping Untappd. It flows: `Establishment` -> `CheckinsGet` (Untappd API) -> `ProcessCheckins` -> Save to DB.
    *   `Program.cs`: DI configuration.
*   **Data Access**: EF Core (`BigBeerContext`).
*   **Configuration**: `local.settings.json` (local), Azure App Settings (prod).

### 2. Client (`/Client`)
*   **Type**: Blazor WebAssembly.
*   **Role**: UI.
*   **Hosting**: Azure Static Web Apps.
*   **Key Files**: `App.razor`, `Program.cs`. Pages are in `Pages/`.

### 3. Shared (`/Shared`)
*   **Type**: .NET Class Library.
*   **Role**: Shared schemas and logic.
*   **Key Models**:
    *   `Checkin`: A user checking in a beer.
    *   `Beer`: Beer details.
    *   `Brewer`: Brewery details.
    *   `Establishment`: A venue being tracked.
    *   `BigBeerContext`: DbContext.

### 4. VisualisationInterop (`/VisualisationInterop`)
*   **Type**: TypeScript / Webpack.
*   **Role**: Custom JS visualizations for Blazor.
*   **Build**: Produces JS bundles used by the Client.

## Common Tasks

*   **Database Changes**: Modify entities in `Shared`. Add migration in `Api` (since it holds the connection logic/host) or a separate design-time project if executed differently.
*   **New Scraper Logic**: Modify `Api/Update.cs`. Note the rate limiting and pagination logic (`MAX_REQUESTS`, `max_id`).
*   **UI Updates**: Modify `.razor` files in `Client`.

## conventions
*   **Line Endings**: Windows (CRLF) implied by existing files.
*   **Indentation**: Tabs (based on `Api/Update.cs`).
