# Architecture Overview

## Overview

OpenLicense follows **Clean Architecture** with **CQRS** (Command Query Responsibility Segregation) via MediatR. The application is divided into 4 independent layers with well-defined responsibilities.

## Dependency Diagram

```mermaid
graph TD
    subgraph "OpenLicense"
        subgraph "Api Layer"
            Api["OpenLicense.Api\n(Controllers, Program.cs, Middleware)"]
        end
        subgraph "Application Layer"
            App["OpenLicense.Application\n(Interfaces, Commands,\nQueries, Handlers, DTOs)"]
        end
        subgraph "Domain Layer"
            Domain["OpenLicense.Domain\n(Entities only)\nNO dependencies"]
        end
        subgraph "Infrastructure Layer"
            Infra["OpenLicense.Infrastructure\n(DB, Auth, Background\nServices)"]
        end
    end

    Api --> App
    Api --> Infra
    App --> Domain
    Infra --> App
    Infra --> Domain

    classDef api fill:#e1f5fe,stroke:#01579b
    classDef app fill:#fff3e0,stroke:#e65100
    classDef domain fill:#e8f5e9,stroke:#1b5e20
    classDef infra fill:#f3e5f5,stroke:#4a148c
    classDef nodetext color:#333;

    class Api api,nodetext
    class App app,nodetext
    class Domain domain,nodetext
    class Infra infra,nodetext
```

**Fundamental rule:** Dependencies point inward. `Api -> Application -> Domain`. Never the reverse.

## Layers

### 1. Domain (`OpenLicense.Domain`)

- **Responsibility:** Entities and intrinsic business rules
- **Dependencies:** None
- **Contains:** POCO entities / DTOs


### 2. Application (`OpenLicense.Application`)

- **Responsibility:** Application boundary — defines interfaces, commands, queries, handlers
- **Dependencies:** Domain
- **Contains:** DTOs, Service Interfaces, MediatR Commands/Queries, Handlers


### 3. Infrastructure (`OpenLicense.Infrastructure`)

- **Responsibility:** Concrete implementations — database, email, auth, background services
- **Dependencies:** Application, Domain
- **Contains:** Implemented services, DbContext, Auth handlers


### 4. Api (`OpenLicense.Api`)

- **Responsibility:** HTTP entry point, controllers, startup
- **Dependencies:** Application, Infrastructure, Domain
- **Contains:** Controllers, Program.cs, Middleware


## Request Lifecycle

```mermaid
flowchart TD
    A["HTTP Request"] --> B["Middleware Pipeline\n(order matters!)"]
    B --> C["1. AuditMiddleware\nLogs request\n(IP, method, status, duration)"]
    C --> D["2. ExceptionHandlingMiddleware\nCatches exceptions then HTTP responses"]
    D --> E["3. RateLimitMiddleware\nIP-based rate limiting\non auth endpoints"]
    E --> F["4. CookieToBearerMiddleware\nConverts auth_token\ncookie to Bearer header"]
    F --> G["5. Authentication\nJWT Bearer or API Key"]
    G --> H["6. Authorization\nPolicy checks"]
    H --> I["Controller, Command/Query\nMediatR, Handler\nService, Database"]
    I --> J["JSON Response"]

    classDef nodetext color:#333;

    class A,B,C,D,E,F,G,H,I,J nodetext
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant AuthController
    participant AuthService
    participant JwtTokenService
    participant Database
    participant Frontend

    Client->>AuthController: POST /api/v1/auth/login
    AuthController->>AuthService: LoginAsync(email, password)
    AuthService->>Database: Find user by email
    Database-->>AuthService: User record
    AuthService->>AuthService: Verify credentials (BCrypt)
    AuthService->>AuthService: Check account not suspended
    AuthService->>JwtTokenService: GenerateToken(user)
    JwtTokenService-->>AuthService: JWT token
    AuthService-->>AuthController: { token: "eyJhbGci..." }
    AuthController-->>Client: { token: "eyJhbGci..." }
    Client->>Frontend: Store token in memory
    Note over Frontend: Via Axios interceptor

    loop Subsequent Requests
        Client->>Frontend: Axios interceptor adds Bearer token
        Frontend->>AuthController: GET /api/v1/auth/me
        AuthController->>AuthService: GetProfileAsync()
        AuthService->>Database: Find user
        Database-->>AuthService: User with API keys
        AuthService-->>AuthController: User profile
        AuthController-->>Frontend: { id, name, email, apiKeys, ... }
    end
```

## Hybrid Authentication (JWT + API Key)

The API uses a **policy scheme** called `SmartAuth` that authenticates via:

- **JWT Bearer:** `Authorization: Bearer <token>`
- **API Key:** `X-Api-Key: api_<key>`

The scheme automatically detects which method to use.

## Observability

- **Traces/Metrics/Logs** -> OpenObserve via OpenTelemetry OTLP
- **Audit Logging** -> Middleware captures IP, status, duration, critical events
- **IP Resolution Priority:** Cloudflare -> Nginx -> X-Forwarded-For -> Remote IP
