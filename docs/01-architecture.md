# Architecture Overview



## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **API Framework** | ASP.NET Core | 9.0 |
| **Language** | C# | 12+ |
| **ORM** | Entity Framework Core | 9.0.17 |
| **Database** | PostgreSQL | 15+ |
| **Database Driver** | Npgsql | 9.0.4 |
| **Auth** | JWT Bearer + Custom API Key | - |
| **Email** | MailKit / MimeKit | 4.17.0 |
| **API Docs** | Scalar (OpenAPI) | 2.16.3 |
| **Environment** | DotNetEnv | 3.1.1 |
| **Frontend** | React | 19.2 |
| **Build** | Vite | 8.0 |
| **HTTP Client** | Axios | 1.18 |
| **Routing** | React Router DOM | 7.18 |
| **Testing** | xUnit + FluentAssertions | 2.9.2 |
| **Container** | Docker Compose | - |

## Data Flow

### Request Lifecycle

1. **Request arrives** at the API (directly in dev, or through Nginx reverse proxy in production)
2. **ExceptionHandlingMiddleware** — wraps all requests, catches unhandled exceptions, returns proper HTTP codes
3. **RateLimitMiddleware** — checks IP-based rate limits on `/api/auth` POST endpoints (login, register, password recovery)
4. **CookieToBearerMiddleware** — reads `auth_token` HttpOnly cookie and injects `Authorization: Bearer <token>` header (bridges React's `withCredentials: true` to JWT auth)
5. **Authentication** — JWT Bearer or API Key authentication via hybrid policy scheme (`SmartAuth`)
6. **Authorization** — checks `[Authorize]` policy on controllers/actions
7. **Controller** → **Service** → **Database**
8. **Response** returned as JSON

### Authentication Flow

```
User Login
    │
    ▼
POST /api/auth/login { email, password }
    │
    ▼
AuthService.LoginAsync()
    │
    ├── Verify credentials
    │
    ▼
JwtTokenService.GenerateToken(user)
    │
    ▼
Return { token: "eyJhbGci..." }
    │
    ▼
Frontend stores token in memory
    │
    ▼
Subsequent requests:
  - Axios interceptor adds Bearer token to Authorization header
  - Cookie bridge middleware picks up token from cookie (if present)
  - JWT middleware validates and populates HttpContext.User
```

### Hybrid Authentication (JWT + API Key)

The API uses a **policy scheme** called `SmartAuth` that can authenticate via either:
- **JWT Bearer**: `Authorization: Bearer <token>`
- **API Key**: `X-Api-Key: api_<key>`

The scheme checks both headers and auto-selects the authentication scheme that matches.

```csharp
// Policy scheme registration (AuthExtensions.cs)
services.AddAuthentication("SmartAuth")
    .AddScheme<AuthenticationSchemeOptions, JwtBearerHandler>("Bearer", ...)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyHandler>("ApiKey", ...)
    .AddPolicyScheme("SmartAuth", null, options => {
        options.ForwardSelection = (ctx) =>
            ctx.Request.Headers.ContainsKey("X-Api-Key") ? "ApiKey" : "Bearer";
    });
```

## Configuration Layers

| Source | Priority | Purpose |
|--------|----------|---------|
| `appsettings.json` | Low | Default config, connection strings, JWT secret, email settings |
| `appsettings.Development.json` | Medium | Dev overrides (e.g., log level) |
| `.env` file (DotNetEnv) | High | Environment variables loaded by DotNetEnv |
| Environment variables | Highest | Docker/production config (`ASPNETCORE_*`, custom vars) |
