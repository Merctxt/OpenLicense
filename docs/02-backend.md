# Backend

## Overview

The OpenLicense backend is a REST API built with .NET 9 and ASP.NET Core. It handles authentication, products, licenses, activations, and e-mail flows.

The application is organized in clear layers:

- Controllers: expose HTTP endpoints
- Services: implement business logic
- Data: database access through EF Core
- Models: domain entities
- Middleware: authentication, rate limiting, error handling, and proxy compatibility

## API Versioning

The API uses **URL path versioning** (`/api/v{version}`). The current version is **v1**.

All routes are prefixed with `/api/v1/...`:
- Auth: `/api/v1/auth/...`
- Products: `/api/v1/products/...`
- Licenses: `/api/v1/licenses/...`

Future versions will be deployed alongside v1 during transition periods. Breaking changes will only occur in new major versions (e.g., `/api/v2/`).


## Main structure

```text
Backend/
├── Controllers/
├── Services/
├── Models/
├── Data/
├── DTOs/
├── Middleware/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── OpenLicenseApi.csproj
```

## Request flow

```text
HTTP request
  ↓
Middleware pipeline
  ↓
Authentication / Authorization
  ↓
Controller
  ↓
Service
  ↓
Database / Email / JWT / API Key flow
```

## Middleware

Execution order matters:

1. `AuditMiddleware` — resolves client IP from proxy headers, logs all requests (IP, method, path, status, duration), emits audit events for critical endpoints. All logs sent to OpenObserve via OTLP.
2. `ExceptionHandlingMiddleware` — catches exceptions and converts them into HTTP responses
3. `RateLimitMiddleware` — limits requests per IP and route
4. `CookieToBearerMiddleware` — reads the session cookie and forwards it as a Bearer token
5. `Authentication` — validates JWT or API key
6. `Authorization` — checks permission rules
7. `Controllers` — executes the final business logic

## Authentication

The API supports two auth flows:

- JWT Bearer for authenticated users
- X-Api-Key for external license validation

## Controllers and endpoints

### AuthController

Base path: `/api/v{version}/auth`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| POST | `/api/v1/auth/register` | None | User registration |
| POST | `/api/v1/auth/login` | None | Login and token response |
| POST | `/api/v1/auth/logout` | None | Clear auth cookie |
| GET | `/api/v1/auth/me` | JWT | Current user profile |
| PUT | `/api/v1/auth` | JWT | Update profile |
| DELETE | `/api/v1/auth` | JWT | Delete account |
| POST | `/api/v1/auth/apikey` | JWT | Create API key |
| DELETE | `/api/v1/auth/apikey` | JWT | Remove API key |
| PUT | `/api/v1/auth/apikey/toggle` | JWT | Toggle API key status |
| POST | `/api/v1/auth/forgot-password` | None | Request password recovery |
| POST | `/api/v1/auth/reset-password/verify` | None | Validate reset token |
| POST | `/api/v1/auth/reset-password` | None | Reset password |
| PUT | `/api/v1/auth/report-preferences` | JWT | Update email report opt-in preference |

### ProductsController

Base path: `/api/v{version}/products`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| GET | `/api/v1/products/all` | JWT | List user products |
| POST | `/api/v1/products/create` | JWT | Create product |
| PUT | `/api/v1/products/update` | JWT | Update product |
| DELETE | `/api/v1/products` | JWT | Delete product |

### LicensesController

Base path: `/api/v{version}/licenses`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| GET | `/api/v1/licenses` | JWT | List licenses |
| POST | `/api/v1/licenses` | JWT | Create license |
| PUT | `/api/v1/licenses` | JWT | Update license |
| DELETE | `/api/v1/licenses` | JWT | Delete license |
| GET | `/api/v1/licenses/activations` | JWT | List activations |
| POST | `/api/v1/licenses/validate` | API Key | Validate license and activate hardware |
| POST | `/api/v1/licenses/deactivate` | API Key | Deactivate hardware |
| POST | `/api/v1/licenses/deactivate-by-jwt` | JWT | Deactivate by user |
| PUT | `/api/v1/licenses/activations/toggle` | JWT | Toggle activation status |

## Main services

### AuthService

Responsible for:

- user registration
- login with password validation
- JWT generation and validation
- API key creation
- password reset
- e-mail-based recovery

### ProductService

Responsible for:

- product creation and editing
- quantity limits per user
- ownership enforcement

### LicenseService

Responsible for:

- license creation
- key validation
- hardware activation tracking
- activation limits and expiration handling

### EmailService

Sends password recovery e-mails through SMTP.

### ReportService

Generates license status reports for opted-in users. Called by the background service.

- Eagerly loads `User -> Products -> Licenses -> Activations`
- Computes per-product summaries and counts
- Collects expiring and expired licenses
- Returns a `ReportDataDto` with all report data

### ReportEmailBackgroundService

A .NET `BackgroundService` that runs on a configurable interval to send report emails to opted-in users.

- Registered in `Program.cs` via `AddHostedService<ReportEmailBackgroundService>()`
- Uses `PeriodicTimer` with `IServiceScopeFactory` for DI scope per execution

### RateLimiterService

Implements in-memory rate limiting to avoid abuse of authentication and password reset endpoints.


## Database

The backend uses PostgreSQL through EF Core and `Npgsql`.

In local development, the database is usually run in a container or in an external managed instance.

## Observability

The API exports Traces, Metrics, and Logs to OpenObserve via OpenTelemetry OTLP.

### Audit Logging

`AuditMiddleware` runs on every request and captures:

**Request log (every request):**
- Client IP (resolved from proxy headers)
- HTTP method and path
- Status code
- Duration in ms
- User agent
- Correlation ID
- Authenticated user (if any)

**Audit events (critical endpoints only):**
- `user_register` / `user_login` / `user_logout`
- `api_key_operation` / `license_operation` / `product_operation`
- `password_reset_requested` / `password_reset`
- `license_validate` / `license_deactivate`

All logs are structured and sent to OpenObserve via OTLP for centralized querying.

### IP Resolution Priority

Headers checked in order:
1. `cf-connecting-ip` (Cloudflare)
2. `x-real-ip` (Nginx)
3. `x-forwarded-for` (multi-hop proxies — first valid IP used)
4. `RemoteIpAddress` (direct connection)

IPv4 and IPv6 are both supported. Private/reserved IPs are filtered.

### Telemetry Configuration

Relevant environment variables:

- `OTEL_EXPORTER_OTLP_ENDPOINT`
- `OTEL_EXPORTER_OTLP_HEADERS`

These variables are read directly by ASP.NET Core and used by the OpenTelemetry exporters.

## Important business rules

- minimum password length: 8 characters, including uppercase, lowercase, number, and special character
- e-mail normalized to lowercase
- maximum number of products and licenses per user
- API keys stored with secure hashing
- password reset through a temporary token


## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=...;Database=...;Username=...;Password=..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "OpenLicenseApi",
    "Audience": "OpenLicenseApiUsers"
  },
  "FrontendUrl": "http://localhost:3000",
  "RegistrationEnabled": true,
  "Email": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Secure": false,
    "Username": "...",
    "Password": "...",
    "From": "noreply@example.com"
  },
  "ReportSettings": {
    "IntervalMinutes": 4320,
    "ExpiryWarningDays": 7,
    "Enabled": true
  },
  "OTEL_EXPORTER_OTLP_ENDPOINT": "https://.../api/default",
  "OTEL_EXPORTER_OTLP_HEADERS": "Authorization=Basic ..."
}
```


## Account Suspension

Suspended users cannot log in or make authenticated requests:
- Checked during login (returns "Account is suspended")
- Checked during SmartAuth middleware (returns 401)

## Security Features

1. **Password Hashing**: BCrypt with cost factor 12
2. **Token Hashing**: SHA-256 for both password reset tokens and API keys
3. **HttpOnly Cookies**: Prevents XSS token theft
4. **SameSite Strict**: Prevents CSRF
5. **RS256 JWT**: Asymmetric signing for key rotation support
6. **Rate Limiting**: Brute force protection on auth endpoints
7. **No Plain Text Secrets**: API keys and reset tokens never stored in plain text
8. **Email Normalization**: Lowercase email addresses prevent case-based duplication
9. **UTC Timestamps**: All timestamps are stored in UTC
10. **JWT Expiration**: Tokens expire in 30 minutes with 30s clock skew
11. **Password Complexity**: Minimum 8 characters, including uppercase, lowercase, number, and special character
12. **Account Suspension**: Suspended users cannot log in or make authenticated requests
13. **Max email changes**: Users can only change their email a limited number of times per month
