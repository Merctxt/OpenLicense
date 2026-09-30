# Environment Setup

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional but recommended)
- [VS Code](https://code.visualstudio.com/) or Visual Studio 2022
- [Git](https://git-scm.com/)

## Clone the Project

```bash
git clone https://github.com/Merctxt/OpenLicense.git
cd OpenLicense
```

## Environment Configuration

### 1. .env File

In the project root, copy the template:

```bash
cp .env.example .env
```

Edit `.env` with your credentials, setting values for:

- **PostgreSQL:** `DATABASE_CONNECTION` with the full connection string
- **JWT:** `JWT_SECRET_KEY` (a secure random secret), `JWT_ISSUER`, `JWT_AUDIENCE`
- **Registration:** `REGISTRATION_ENABLED` (true/false)
- **Email (SMTP):** `Email__Host`, `Email__Port`, `Email__Secure`, `Email__Username`, `Email__Password`, `Email__From`
- **OpenTelemetry:** `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_HEADERS` for trace/metric export
- **Frontend URLs:** `VITE_API_URL`, `VITE_STATUS_URL`, `VITE_SOURCE_URL`

### 2. Backend Configuration

The backend reads configuration with this priority:

1. **Environment variables** (highest)
2. `appsettings.Development.json`
3. `appsettings.json` (base)

Important settings in `appsettings.json`:

```text
database_connection=...  # or DATABASE_CONNECTION (env var)
Jwt__SecretKey=...
Jwt__Issuer=OpenLicenseApi
Jwt__Audience=OpenLicenseApiUsers
REGISTRATION_ENABLED=true
FrontendUrl=http://localhost:3000
ReportSettings__IntervalMinutes=4320
ReportSettings__ExpiryWarningDays=7
ReportSettings__Enabled=true
```

### 3. Frontend Configuration

In `Frontend/.env.development`, set:

```text
VITE_API_URL=http://localhost:5000
VITE_STATUS_URL=https://status.example.com
VITE_SOURCE_URL=https://github.com/Merctxt/OpenLicense
VITE_REGISTRATION_ENABLED=true
```

## Database Setup

### Option A: Local PostgreSQL

Install PostgreSQL 15 or later using your platform's package manager (`apt install postgresql-15` on Ubuntu, `brew install postgresql` on macOS, or `choco install postgresql` on Windows). Then create the database using the `psql` command-line tool.

### Option B: PostgreSQL via Docker

Run PostgreSQL in a Docker container using `docker compose up` from the project root, which starts a PostgreSQL 15 instance on port 5432 with the configured password.

### Option C: Managed PostgreSQL (Production)

Use a cloud provider such as Azure Database for PostgreSQL, AWS RDS PostgreSQL, DigitalOcean Managed PostgreSQL, or Heroku Postgres.


## Environment Variables in Docker

The `.env` file in the root is read by `docker-compose.yml` and exposes variables to the container including `ASPNETCORE_ENVIRONMENT`, `DATABASE_CONNECTION`, `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `REGISTRATION_ENABLED`, all the `Email__*` settings, `FRONTEND_PORT`, `API_PORT`, the `VITE_*` frontend variables, and the `OTEL_EXPORTER_*` OpenTelemetry settings.
