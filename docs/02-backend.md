# Backend

## Overview

The OpenLicense backend is a REST API built with .NET 9 and ASP.NET Core. It handles authentication, products, licenses, activations, and e-mail flows.

The application is organized in clear layers:

- Controllers: expose HTTP endpoints
- Services: implement business logic
- Data: database access through EF Core
- Models: domain entities
- Middleware: authentication, rate limiting, error handling, and proxy compatibility

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

1. `ExceptionHandlingMiddleware` — catches exceptions and converts them into HTTP responses
2. `RateLimitMiddleware` — limits requests per IP and route
3. `CookieToBearerMiddleware` — reads the session cookie and forwards it as a Bearer token
4. `Authentication` — validates JWT or API key
5. `Authorization` — checks permission rules
6. `Controllers` — executes the final business logic

## Authentication

The API supports two auth flows:

- JWT Bearer for authenticated users
- X-Api-Key for external license validation clients

## Controllers and endpoints

### AuthController

Base path: `/api/auth`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| POST | `/api/auth/register` | None | User registration |
| POST | `/api/auth/login` | None | Login and token response |
| POST | `/api/auth/logout` | None | Clear auth cookie |
| GET | `/api/auth/me` | JWT | Current user profile |
| PUT | `/api/auth` | JWT | Update profile |
| DELETE | `/api/auth` | JWT | Delete account |
| POST | `/api/auth/apikey` | JWT | Create API key |
| DELETE | `/api/auth/apikey` | JWT | Remove API key |
| POST | `/api/auth/forgot-password` | None | Request password recovery |
| POST | `/api/auth/reset-password/verify` | None | Validate reset token |
| POST | `/api/auth/reset-password` | None | Reset password |
| PUT | `/api/auth/report-preferences` | JWT | Update email report opt-in preference |

### ProductsController

Base path: `/api/products`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| GET | `/api/products/all` | JWT | List user products |
| POST | `/api/products/create` | JWT | Create product |
| PUT | `/api/products/update` | JWT | Update product |
| DELETE | `/api/products` | JWT | Delete product |

### LicensesController

Base path: `/api/licenses`

| Method | Endpoint | Authentication | Description |
|---|---|---|---|
| GET | `/api/licenses` | JWT | List licenses |
| POST | `/api/licenses` | JWT | Create license |
| PUT | `/api/licenses` | JWT | Update license |
| DELETE | `/api/licenses` | JWT | Delete license |
| GET | `/api/licenses/activations` | JWT | List activations |
| POST | `/api/licenses/validate` | API Key | Validate license and activate hardware |
| POST | `/api/licenses/deactivate` | API Key | Deactivate hardware |
| POST | `/api/licenses/deactivate-by-jwt` | JWT | Deactivate by user |

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

The API also supports OTLP export to OpenObserve/OpenTelemetry.

Relevant variables:

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
