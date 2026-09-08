# Docker

## Overview

The Docker Compose setup runs two main services:

- .NET API
- React/Vite frontend

The database is not managed by this compose file. It must exist externally, such as a local PostgreSQL instance, Azure Database for PostgreSQL, or another provider.

## Compose structure

```yaml
services:
  api:
    build: ./Backend
  frontend:
    build: ./Frontend
```

## Environment variables

### Root `.env`

| Variable | Example | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Backend runtime environment |
| `API_PORT` | `5000` | API port |
| `DATABASE_CONNECTION` | `Host=...;Database=...` | PostgreSQL connection string |
| `JWT_SECRET_KEY` | `...` | JWT secret key |
| `JWT_ISSUER` | `OpenLicenseApi` | JWT issuer |
| `JWT_AUDIENCE` | `OpenLicenseApiUsers` | JWT audience |
| `REGISTRATION_ENABLED` | `true` | Enables user registration |
| `EMAIL_HOST` | `smtp.mailgun.org` | SMTP host |
| `EMAIL_PORT` | `587` | SMTP port |
| `EMAIL_SECURE` | `false` | TLS/SSL |
| `EMAIL_USERNAME` | `...` | SMTP username |
| `EMAIL_PASSWORD` | `...` | SMTP password |
| `EMAIL_FROM` | `...` | Sender e-mail |
| `FRONTEND_PORT` | `3000` | Frontend port |
| `VITE_API_URL` | `http://localhost:5000` | API URL for the frontend |
| `VITE_STATUS_URL` | `https://status.example.com` | Status page URL |
| `VITE_SOURCE_URL` | `https://github.com/...` | Repository URL |
| `VITE_REGISTRATION_ENABLED` | `true` | Enables frontend registration |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `https://.../api/default` | OTLP endpoint |
| `OTEL_EXPORTER_OTLP_HEADERS` | `Authorization=Basic ...` | OTLP auth header |


### 3. Start the stack

```bash
docker compose up --build -d
```

### 4. View logs

```bash
docker compose logs -f
```

### 5. Stop the services

```bash
docker compose down
```

## Healthcheck

The API exposes a health endpoint at:

```text
http://localhost:5000/health
```

The compose file uses this route to determine when the API is ready to accept requests.

## Important note

PostgreSQL is not provisioned automatically by this compose setup. You must provide a working database before starting the API. 
