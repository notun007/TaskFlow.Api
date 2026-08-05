# TaskFlow

Bank-specific work-management platform for software ownership, support responsibility, vendors, audit, and governance.

## Projects

- `TaskFlow.Domain` — business entities and workflow enums.
- `TaskFlow.Application` — use cases, DTOs, and application abstractions.
- `TaskFlow.Infrastructure` — Oracle EF Core code-first persistence and ASP.NET Core Identity.
- `TaskFlow.Api` — ASP.NET Core 10 HTTP API.
- `TaskFlow.Web` — Angular dashboard client.

## API setup

1. Restore packages when NuGet access is available: `dotnet restore TaskFlow.slnx`.
2. Create the first schema migration: `dotnet ef migrations add InitialCreate --project TaskFlow.Infrastructure --startup-project TaskFlow.Api`.
3. Apply it: `dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.Api`.
4. Run: `dotnet run --project TaskFlow.Api`.

The Oracle 19c connection is configured under `TaskFlow.Api/appsettings.json`. Move it to a secret store or environment variable before committing to a shared environment.

## Web setup

```powershell
cd .\TaskFlow.Web
npm install
npm start
```

The initial Angular surface includes a dashboard, task queue search, navigation state, audit activity, and a task creation modal. The API currently exposes task listing, creation, status transitions, and health checks.
