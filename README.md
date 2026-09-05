# OpenLicense

Self-hosted software licensing platform for managing product licenses, API keys, and client activations.

## Stack

| Layer | Technology |
|-------|-----------|
| **API** | .NET 9, ASP.NET Core, EF Core, PostgreSQL |
| **Web Dashboard** | React 19, Vite, Axios |
| **Auth** | JWT Bearer + API Key (hybrid policy) |
| **Docs** | Scalar (OpenAPI/Swagger UI) |
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

## Project Structure

```
OpenLicense/
├── Backend/                  # .NET 9 Web API
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Models/               # Entity models
│   ├── DTOs/                 # Request/response types
│   ├── Middleware/           # Auth, rate limiting, error handling
│   ├── Data/                 # EF Core DbContext
│   └── Migrations/           # Database migrations
├── Frontend/                 # React 19 dashboard
│   ├── src/
│   │   ├── pages/            # Page components
│   │   ├── components/       # Shared UI components
│   │   ├── context/          # Auth & theme providers
│   │   └── api/              # Axios client + endpoints
├── Tests/                    # xUnit integration tests (74 tests)
├── docs/                     # Full documentation
│   ├── 01-architecture.md
│   ├── 02-backend.md
│   ├── 03-frontend.md
│   ├── 04-api-reference.md
│   ├── 05-database.md
│   ├── 06-authentication.md
│   ├── 07-configuration.md
│   ├── 08-docker.md
│   ├── 09-testing.md
│   ├── 10-development.md
│   └── 11-migration-roadmap.md
├── docker-compose.yml        # API + frontend services
└── .env.example              # Environment template
```

## Key Features

- **User management** — registration, login, profile, account deletion
- **Product management** — CRUD for software products (limit: 3 per user)
- **License management** — generate license keys, set activation limits, expiration
- **Client validation** — `/api/licenses/validate` endpoint for client-side checks
- **API key authentication** — manage scoped API keys for integrations
- **Hybrid auth** — JWT for dashboard, API Key for client endpoints
- **Password recovery** — forgot/verify/reset flow with email
- **Rate limiting** — per-IP rate limiting on sensitive endpoints

## Documentation

| Doc | Description |
|-----|-------------|
| [Architecture](docs/01-architecture.md) | System overview, diagrams, data flow |
| [Backend](docs/02-backend.md) | API architecture, controllers, services, middleware |
| [Frontend](docs/03-frontend.md) | Pages, components, contexts, routing |
| [API Reference](docs/04-api-reference.md) | Complete endpoint reference |
| [Database](docs/05-database.md) | Entity models, relationships, migrations |
| [Authentication](docs/06-authentication.md) | JWT + API Key, rate limiting, security |
| [Configuration](docs/07-configuration.md) | Environment variables, app settings |
| [Docker](docs/08-docker.md) | Container setup, images, deployment |
| [Testing](docs/09-testing.md) | Test suite, structure, how to run |
| [Development](docs/10-development.md) | Conventions, hot-reload, debugging |
| [Migration Roadmap](docs/11-migration-roadmap.md) | Bootstrap → Tailwind plan |

## License

Custom Non-Commercial Software License. See [LICENSE.md](LICENSE.md). Commercial use prohibited without written permission.
