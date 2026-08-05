# TaskFlow.Api

ASP.NET Core 10 Web API using Clean Architecture, EF Core code-first, Oracle 19c, and ASP.NET Core Identity.

## Run locally

```powershell
dotnet run --project .\TaskFlow.Api.csproj
```

Health and task endpoints:

- `GET /health` — framework health check
- `GET /api/health` — JSON service status
- `GET /api/tasks?search=payment` — task search
- `POST /api/tasks` — create a task
- `PATCH /api/tasks/{id}/status` — transition workflow status

Run migrations after restoring packages with `dotnet ef migrations add InitialCreate --project ..\TaskFlow.Infrastructure --startup-project .`.
For production, move the connection string to a secret store or environment variable.
