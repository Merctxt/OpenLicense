# Docker

## Overview

The Docker Compose setup runs two main services:

- .NET API
- React/Vite frontend

The database is not managed by this compose file. It must exist externally, such as a local PostgreSQL instance, Azure Database for PostgreSQL, or another provider.

## Deployment modes

Two deployment modes are available, each with its own compose file:

| Mode | Compose file | Build source | Use case |
|---|---|---|---|
| Latest | `docker-compose.yml` | Local `./Backend` and `./Frontend` | Coolify webhook, CI/CD, build from most recent commit |
| Release | `docker-compose.release.yml` | Git tag/branch from GitHub | Pin deployment to a specific release version |

### Deploy with latest (default)

```bash
docker compose up --build -d
```

This is the recommended mode for development, continuous deployment, and Coolify webhook-based deployments. It builds from the local source code.

### Deploy with a release version

Use this mode when you want to deploy from a Git reference (tag, branch, or commit hash) without local source files:

```bash
# Deploy from a specific release tag
RELEASE_VERSION=v1.0.1 docker compose -f docker-compose.release.yml up -d

# Deploy from main branch (latest code)
RELEASE_VERSION=main docker compose -f docker-compose.release.yml up -d
```

The compose file downloads the repository from GitHub at the specified Git reference and builds it in place. No pre-built images or CI workflows are required.

Any valid Git reference works:

| Reference | Example |
|---|---|
| Tag (release) | `v1.0.1` |
| Branch | `main`, `dev` |
| Commit hash | `acb6c32` |

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
| `RELEASE_VERSION` | `v1.0.1` | Git reference for release deployment |
| `RELEASE_VERSION` | `main` | Git reference for latest code |

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
