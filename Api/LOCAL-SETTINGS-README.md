Copy this template to `local.settings.json` before running the Functions host.

DO NOT commit `local.settings.json` — it may contain secrets.

Steps:

1. Copy the template to `local.settings.json` (PowerShell):

```powershell
cd d:\BigBeerData\Api
cp local.settings.json.template local.settings.json
```

2. Edit `local.settings.json` and replace placeholders:
- `DBConnection` — connection string for your dev SQL Server / LocalDB
- `client_id` and `client_secret` — Untappd API credentials

3. (Optional) Start Azurite for `AzureWebJobsStorage` if you use `UseDevelopmentStorage=true`:

```powershell
npm i -g azurite
azurite
```

4. Start the Functions host:

```powershell
func start
```

**Host information and CORS**

- **Functions host (default):** `http://localhost:7071`
- **Client dev server (example):** `https://localhost:7168`

If your client runs on a different origin you should allow it in the Functions host CORS settings. Example (from repo root):

```powershell
func cors add https://localhost:7168
```

If you changed the Functions host URL (for example via `ASPNETCORE_URLS` or other env var), update the `FUNCTIONS_HOST` value in `local.settings.json` accordingly.

If you prefer environment variables instead of a `local.settings.json`, set them in PowerShell before starting:

```powershell
$env:DBConnection = 'Data Source=(localdb)\\ProjectsV13;Initial Catalog=BigBeerData.Core;Integrated Security=True;'
$env:client_id = '...'
$env:client_secret = '...'
func start
```
