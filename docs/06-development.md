# Development Guide

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional, but useful)
- [VS Code](https://code.visualstudio.com/) or Visual Studio 2022

## Project structure

```text
OpenLicense/
├── Backend/               # .NET API
├── Frontend/              # React/Vite app
├── Tests/                 # integration tests
├── docs/                  # documentation
├── docker-compose.yml     # Docker environment
├── .env.example           # root environment template
├── .gitignore
├── README.md
├── start.ps1              # starts backend + frontend locally
└── LICENSE.md
```

## Getting started

### 1. Clone the project

```bash
git clone <repo-url>
cd OpenLicense
```

### 2. Create the environment file

At the project root, copy the template:

```bash
cp .env.example .env
```

This `.env` file is used by Docker Compose. It does not replace the local backend/frontend environment variables during direct local development.

### 3. Configure the database and e-mail settings

Edit the `.env` file with:

- PostgreSQL connection string
- JWT secret
- SMTP host and credentials
- OpenObserve/OTLP endpoint if enabled

## Running locally

### Option A: use the project script

In PowerShell:

```powershell
.\start.ps1
```

This script starts:

- `dotnet run` in the backend
- `npm run dev` in the frontend
- stops both when the session ends

### Option B: run manually

#### Backend

```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

#### Frontend

```bash
cd Frontend
npm install
npm run dev
```

## Backend configuration

The backend reads configuration in this priority order:

1. environment variables
2. `appsettings.Development.json`
3. `appsettings.json`

Relevant configuration examples:

```text
database_connection=Host=...;Database=...;
Jwt__SecretKey=...
Jwt__Issuer=OpenLicenseApi
Jwt__Audience=OpenLicenseApiUsers
REGISTRATION_ENABLED=true
```

## Local frontend

The client runs at:

```text
http://localhost:3000
```

The API runs at:

```text
http://localhost:5000
```

## Debugging

### Backend

In VS Code or Visual Studio, open the project and run the API in debug mode.

### Frontend

Use the VS Code launch configuration to open the app in Edge with source maps enabled.

## Migrations

```bash
cd Backend
dotnet ef migrations add MigrationName
dotnet ef database update
```

If you need to remove the most recent local migration:

```bash
dotnet ef migrations remove
```

## Tests

```bash
cd OpenLicense
dotnet test
```

## Project conventions

- keep services small and focused
- use DTOs for request and response payloads
- validate business rules in the backend
- prefer integration tests for API flows
- document environment variables and deployment changes


### Run tests with coverage

```bash
dotnet test /p:CollectCoverage=true
```

### Filter tests

```bash
dotnet test --filter "Category=Integration"
dotnet test --filter "FullyQualifiedName~RegisterTests"
```

## Debugging tips

### Backend

- Check logs: `dotnet run` writes to the console
- Use `ILogger<T>` for structured logging
- Enable detailed errors in development: `appsettings.Development.json`

### Frontend

- React DevTools browser extension
- Browser Network tab to inspect API calls
- Check Axios interceptors in `Frontend/src/api/client.js`

### Database

- Connect with any PostgreSQL client (DBeaver, pgAdmin, VS Code pg extension)
- Use `dotnet ef dbcontextinfo` to inspect the schema
- Enable EF Core logging by setting `logging:LogLevel:Microsoft.EntityFrameworkCore` to `Debug`

## Common issues

### CORS errors

Ensure the frontend origin is included in `Cors:AllowedOrigins` in `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": "http://localhost:3000"
  }
}
```

### JWT authentication failures

- Check that `Jwt:SecretKey` matches in both backend and test configuration
- Verify the token has not expired (default: 30 minutes)
- Check cookie settings for browser-based auth

### Database connection issues

- Verify the connection string format: `Host=...;Port=5432;Database=...;Username=...;Password=...`
- Ensure PostgreSQL is reachable from the host
- Check firewall rules for external databases

### Port conflicts

- Backend default: 5000 (configure via `Properties/launchSettings.json`)
- Frontend default: 3000 (configure in `Frontend/vite.config.js`)
- PostgreSQL default: 5432
