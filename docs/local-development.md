# TrackCubed Local Development

This local development path avoids Azure SQL, the deployed API, and Entra sign-in for day-to-day Windows development.

## Defaults

- Database: SQL Server 2022 in Docker.
- Database port: `localhost,14333`.
- Database name: `TrackCubedDev`.
- Persistent data: Docker volume `trackcubed-sql-data`.
- API auth: Development-only fake user when `TrackCubed:Auth:Mode` is `Development`.
- API database selection: Development mode uses `TrackCubed:LocalDevelopment:ConnectionString` so older Azure SQL user secrets do not override local Docker SQL.
- MAUI auth: Debug Windows builds use a local fake auth session.
- MAUI API URL: Debug Windows builds use `http://localhost:5231`.
- Production behavior: Release builds keep the deployed Azure API and MSAL auth.

## Start The Local Stack

Run these commands from the repo root:

```powershell
docker compose up -d
dotnet ef database update --project TrackCubed.Api
dotnet run --project TrackCubed.Api --launch-profile local-http
```

The EF command defaults to the Development Docker SQL configuration through `ApplicationDbContextFactory`. If you want to be explicit in a shell, run `$env:ASPNETCORE_ENVIRONMENT='Development'` first.

Then run `TrackCubed.Maui` with the `Windows Machine` profile from Visual Studio.

## Reset Local Data

Stop the database and delete the persistent volume:

```powershell
docker compose down
docker volume rm trackcubed_trackcubed-sql-data
```

The next `docker compose up -d` plus `dotnet ef database update --project TrackCubed.Api` will recreate a clean database.

## Docker Notes

If Docker cannot connect to the Docker Desktop engine, start Docker Desktop and make sure your Windows user has permission to use it.
