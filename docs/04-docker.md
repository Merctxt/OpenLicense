# Docker


Docker Compose orchestrates the application stack with two services:
- **API** 
- **Frontend**

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

All variables are defined in `.env.example` — copy it to `.env` and fill in the values you need.

| Variable | Default | Description |
|-----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | App environment (`Development`, `Production`) |
| `API_PORT` | `5000` | Host port for API |
| `DATABASE_CONNECTION` | (required) | PostgreSQL connection string |
| `JWT_SECRET_KEY` | (required) | JWT signing secret |
| `JWT_ISSUER` | `OpenLicenseApi` | JWT issuer |
| `JWT_AUDIENCE` | `OpenLicenseApiUsers` | JWT audience |
| `REGISTRATION_ENABLED` | `true` | Allow new user registrations (`true`/`false`) |
| `EMAIL_HOST` | (empty) | SMTP host |
| `EMAIL_PORT` | `587` | SMTP port |
| `EMAIL_SECURE` | `false` | SMTP SSL/TLS |
| `EMAIL_USERNAME` | (empty) | SMTP username |
| `EMAIL_PASSWORD` | (empty) | SMTP password |
| `EMAIL_FROM` | (empty) | Sender email |
| `FRONTEND_PORT` | `3000` | Host port for frontend |
| `VITE_API_URL` | (empty) | API base URL for frontend build |
| `VITE_STATUS_URL` | (empty) | Status page URL |
| `VITE_SOURCE_URL` | (empty) | Source repository URL |
| `VITE_REGISTRATION_ENABLED` | `true` | Frontend registration toggle (`true`/`false`) |


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


## Network

Services communicate via the Docker Compose internal network:
- Frontend is built with `VITE_API_URL` pointing to the API
- Frontend serves static files and makes browser requests to `/api/*` on the frontend port
- The Nginx config proxies `/api/*` to the Backend API
