---
description: Start the database, API, and WebApp
---

1. Start the SQL Server container
// turbo
`docker compose up -d`

2. Start the Azure Functions API (in a separate terminal)
cd Api
// turbo
`func start`

3. **CRITICAL**: Verify API is healthy (HTTP 200) (in a seperate terminal)
// turbo
`until curl -s -f http://localhost:7071/api/location/current > /dev/null; do echo "Waiting for API to be ready..."; sleep 2; done`

4. Start the WebApp (in same terminal as 3.)
`dotnet run --project WebApp --urls http://localhost:5200`