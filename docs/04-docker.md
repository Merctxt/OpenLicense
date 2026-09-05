# Docker

## Overview

Docker Compose orchestrates the application stack with two services:
- **API** (.NET 9.0 API)
- **Frontend** (React/Vite static files served by Nginx)

**Note:** PostgreSQL is NOT managed by this docker-compose. The database must be available externally (e.g., Azure Database for PostgreSQL, self-hosted).

## Docker Compose Configuration

### Services

#### API

| Property | Value |
|----------|-------|
| Build Context | `./Backend` |
| Image Tag | `openlicense-api` |
| Container Name | `openlicense-api` |
| Ports | `5000:5000` (HTTP) |
| Environment | ASPNETCORE_ENVIRONMENT, database_connection, Jwt settings |
| Restart | unless-stopped |
| Health Check | GET /health every 15s (10 retries, 60s start period) |
| Depends On | none (expects external PostgreSQL) |

#### Frontend

| Property | Value |
|----------|-------|
| Build Context | `./Frontend` |
| Image Tag | `openlicense-frontend` |
| Container Name | `openlicense-frontend` |
| Ports | `3000:3000` |
| Build Args | VITE_API_URL (passed at build time) |
| Restart | unless-stopped |
| Depends On | api (health check) |

### Environment Variables

Set in `.env` file (copied from `.env.example`):

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | App environment |
| `API_PORT` | `5000` | Host port for API |
| `DATABASE_CONNECTION` | (required) | PostgreSQL connection string |
| `JWT_SECRET_KEY` | (required) | JWT signing secret |
| `JWT_ISSUER` | `OpenLicenseApi` | JWT issuer |
| `JWT_AUDIENCE` | `OpenLicenseApiUsers` | JWT audience |
| `FRONTEND_PORT` | `3000` | Host port for frontend |
| `VITE_API_URL` | (empty) | API base URL for frontend build |



## Usage

### Build and Start

```bash
docker compose up --build -d
```

### Stop

```bash
docker compose down
```

### View Logs

```bash
docker compose logs -f
```

### API Logs

```bash
docker compose logs -f api
```

### Frontend Logs

```bash
docker compose logs -f frontend
```

### Restart a Service

```bash
docker compose restart api
```

### Run Commands Inside Container

```bash
docker compose exec api dotnet --info
```

## Network

Services communicate via the Docker Compose internal network:
- Frontend is built with `VITE_API_URL` pointing to the API
- Frontend serves static files and makes browser requests to `/api/*` on the frontend port
- The Nginx config proxies `/api/*` to the Backend API
