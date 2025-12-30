# BigBeerData

BigBeerData is a solution designed to track, scrape, and visualize beer check-in data for specific establishments. It is built using a modern .NET stack on Azure.

## Project Structure

The solution consists of four main projects:

*   **`Client`**: A **Blazor WebAssembly** application that serves as the frontend user interface. Hosted via **Azure Static Web Apps**.
*   **`Api`**: An **Azure Functions** (Isolated Worker) project serving as the backend. It handles data processing, API endpoints, and the core scraping logic (`Update.cs`) to fetch data.
*   **`Shared`**: A C# Class Library containing shared data models (`Checkin`, `Beer`, `Establishment`, etc.), DTOs, and the Entity Framework Core context (`BigBeerContext`).
*   **`VisualisationInterop`**: A TypeScript/JavaScript project that likely handles complex visualizations (e.g., maps, charts) and interops with the Blazor client.

## Technologies

*   **Framework**: .NET 8.0
*   **Frontend**: Blazor WebAssembly
*   **Backend**: Azure Functions (v4, Isolated)
*   **Database**: SQL Server (Entity Framework Core)
*   **External APIs**: Untappd API
*   **Hosting**: Azure Static Web Apps (Client + Api)

## Key Features

*   **Data Scraping**: The `Api` project contains a function (`Update`) that periodically scrapes check-in datafor configured establishments.
*   **Data Visualization**: The `Client` displays this data, likely showing what beers are being drunk at various locations.
*   **Real-time Updates**: The `Update` function supports Server-Sent Events (SSE) via `text/event-stream` to stream update progress to the client.

## Getting Started

### Prerequisites

*   .NET 8.0 SDK
*   Node.js (for `VisualisationInterop`)
*   Azure Functions Core Tools (for running the API locally)
*   SQL Server (or LocalDB)

### Setup

1.  **Database**: Configure the connection string in `local.settings.json` (Api) or environment variables. Run EF Core migrations to create the schema.
2.  **API**:
    *   Navigate to `Api`
    *   Create `local.settings.json` with necessary keys (`client_id`, `client_secret` for Untappd, `SqlConnectionString`).
    *   Run `func start`
3.  **Client**:
    *   Navigate to `Client`
    *   Run `dotnet watch run`
4.  **Interop**:
    *   Navigate to `VisualisationInterop`
    *   Run `npm install` and `npm run build` (or watch) if modifying JS logic.
