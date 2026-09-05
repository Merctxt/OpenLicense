# OpenLicense

Self-hosted software licensing platform for managing product licenses, API keys, and client activations.

## Stack

| Layer | Technology |
|-------|-----------|
| **API** | .NET 9, ASP.NET Core, EF Core, PostgreSQL |
| **Web Dashboard** | React 19, Vite, Axios |
| **Auth** | JWT Bearer + API Key (hybrid policy) |
| **Docs** | Scalar (OpenAPI/Scalar UI) |
| **Testing** | xUnit, 74 integration tests |
| **Deployment** | Docker Compose (API + Nginx) |

## Quick Start

```bash
# 1. Clone and configure
git clone <repo-url>
cd OpenLicense
cp .env.example .env
# Edit .env with your database connection and JWT secret

# 2. Run with Docker
docker compose up -d

# API: http://localhost:5000
# Dashboard: http://localhost:3000
# API Docs: http://localhost:5000/scalar/v1
```

See [docs/01-architecture.md](docs/01-architecture.md) for the full architecture overview.


## API Endpoints

All API endpoints under `/api/*` require authentication (JWT or API Key) unless marked `None`.

| Group | Base Path | Description |
|-------|-----------|-------------|
| Auth | `/api/auth/*` | Registration, login, profile, password reset, API keys |
| Products | `/api/products/*` | CRUD for products |
| Licenses | `/api/licenses/*` | License CRUD, activations, validation |

Full endpoint reference: [docs/04-api-reference.md](docs/04-api-reference.md)

## Documentation

| Doc | Description |
|-----|-------------|
| [Architecture](docs/01-architecture.md) | System overview, data flow, auth diagram |
| [Backend](docs/02-backend.md) | Controllers, services, models, middleware |
| [API Reference](docs/03-api-reference.md) | Complete endpoint reference with request/response |
| [Docker](docs/04-docker.md) | Docker Compose setup, images, deployment |
| [Testing](docs/05-testing.md) | Test suite, scenarios, how to run |
| [Development](docs/06-development.md) | Prerequisites, workflow, conventions, debugging |

## License

Custom Non-Commercial Software License. See [LICENSE.md](LICENSE.md). Commercial use prohibited without written permission.
