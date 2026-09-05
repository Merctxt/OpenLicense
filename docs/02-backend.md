# Backend Architecture

## Overview

The Backend is a .NET 9 Web API built with ASP.NET Core. It provides a RESTful API for managing software licensing, products, users, and activations.


## Architecture Layers

```
┌──────────────────────────────────────────────────────────────┐
│                    HTTP Request/Response                     │
└──────────────────────────────┬───────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────┐
│                    Middleware Pipeline                        │
│  ExceptionHandling → RateLimit → CookieBridge → Auth →       │
│                    Authorization → Controllers                │
└──────────────────────────────┬───────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────┐
│                       Controllers                             │
│  AuthController │ ProductsController │ LicensesController     │
└──────────────────────────────┬───────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────┐
│                      Services                                 │
│  AuthService │ ProductService │ LicenseService │ EmailService │
│              RateLimiterService                              │
└──────────────────────────────┬───────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────┐
│                      Database                                 │
│              PostgreSQL (via EF Core)                         │
└──────────────────────────────────────────────────────────────┘
```

## Controllers

### AuthController

Routes under `/api/auth` — User authentication and management.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | None | Register new user |
| POST | `/api/auth/login` | None | Login, returns JWT + sets `auth_token` cookie |
| POST | `/api/auth/logout` | None | Clears auth cookie |
| GET | `/api/auth/me` | JWT | Get current user profile + API keys |
| PUT | `/api/auth` | JWT | Update profile (name, email, password) |
| DELETE | `/api/auth` | JWT | Delete account |
| POST | `/api/auth/apikey` | JWT | Create API key (max 3 per user) |
| DELETE | `/api/auth/apikey` | JWT | Delete API key |
| POST | `/api/auth/forgot-password` | None | Send password reset email |
| POST | `/api/auth/reset-password/verify` | None | Verify reset token |
| POST | `/api/auth/reset-password` | None | Reset password with token |

### ProductsController

Routes under `/api/products` — Product CRUD.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/products/all` | JWT | List all products (includes licenses) |
| POST | `/api/products/create` | JWT | Create product (limit: 3 per user) |
| PUT | `/api/products/update` | JWT | Update product |
| DELETE | `/api/products` | JWT | Delete product |

### LicensesController

Routes under `/api/licenses` — License management and validation.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/licenses` | JWT | List licenses by productId |
| POST | `/api/licenses` | JWT | Create license |
| PUT | `/api/licenses` | JWT | Update license (name, status, limits) |
| DELETE | `/api/licenses` | JWT | Delete license |
| GET | `/api/licenses/activations` | JWT | List activations for a license |
| POST | `/api/licenses/validate` | API Key | Validate license key + activate hardware |
| POST | `/api/licenses/deactivate` | API Key | Deactivate hardware |
| POST | `/api/licenses/deactivate-by-jwt` | JWT | Deactivate hardware (JWT auth) |

## Services

### AuthService

Handles user authentication, registration, and API key management.

**Key methods:**
- `RegisterAsync(name, email, password)` — Creates user, hashes password, validates rules
- `LoginAsync(email, password)` — Verifies credentials, generates JWT, sets cookie
- `GetMeAsync(userId)` — Returns user profile with related API keys
- `UpdateAsync(userId, name, email, password)` — Updates profile, enforces uniqueness
- `DeleteAsync(userId)` — Soft delete (cascades via FK)
- `CreateApiKeyAsync(userId, request)` — Generates secure API key (64-char random), stores SHA-256 hash
- `DeleteApiKeyAsync(userId, apiKeyId)` — Deletes API key if owned by user
- `ForgotPasswordAsync(email)` — Generates reset token, sends email, stores hashed token
- `VerifyResetTokenAsync(email, token)` — Validates token (15-min expiry)
- `ResetPasswordAsync(email, token, newPassword)` — Resets password, clears token

**Validation rules:**
- Password: 8-128 chars, must contain uppercase, lowercase, digit, special character
- Name/Email: Max 40 chars, email normalized to lowercase
- API keys: Max 3 per user, name max 40 chars

### ProductService

Manages product CRUD with ownership enforcement.

**Key methods:**
- `GetProductsByUserIdAsync(userId)` — Returns all user's products with licenses
- `CreateProductAsync(userId, request)` — Enforces product limit (3 per user)
- `UpdateProductAsync(userId, productId, request)` — Ensures ownership
- `DeleteProductAsync(userId, productId)` — Ensures ownership

### LicenseService

Manages licenses, activations, and hardware validation.

**Key methods:**
- `GetLicensesByProductIdAsync(userId, productId)` — List licenses (ownership check)
- `CreateLicenseAsync(userId, productId, request)` — Generates license key (4x4 alphanumeric format)
- `UpdateLicenseAsync(userId, licenseId, request)` — Updates name, status, max activations, expiration
- `DeleteLicenseAsync(userId, licenseId)` — Ownership check via product relationship
- `GetLicenseActivationsAsync(userId, licenseId)` — List hardware activations
- `ValidateLicenseAsync(userId, request)` — Validates license key, checks activation limit, creates activation
- `DeactivateLicenseAsync(userId, productId, request)` — Deactivates hardware
- `GenerateLicenseKey()` — Creates keys in `XXXX-XXXX-XXXX-XXXX` format (A-Z, 0-9)

### EmailService

Sends password recovery emails via SMTP (MailKit/MimeKit).

**Method:**
- `SendPasswordResetEmailAsync(toEmail, token)` — Sends HTML + plain text email with recovery token

### RateLimiterService

In-memory sliding-window rate limiter. Thread-safe, no external dependencies.

**Method:**
- `IsAllowed(key, maxRequests, window)` — Returns true if request is within limits

**Cleanup:** Runs every 5 minutes to remove expired entries from memory.

## DTOs

### Authentication
- `RegisterRequest` — name, email, password
- `LoginRequest` — email, password
- `UpdateRequest` — name?, email?, password?
- `CreateApiKeyRequest` — name
- `DeleteApiKeyRequest` — apiKeyId (Guid)
- `CreateApiKeyResponse` — id, name, apiKey, createdAt, isActive
- `ForgotPasswordRequest` — email
- `VerifyTokenRequest` — email, token
- `ResetPasswordRequest` — email, token, password

### Products
- `CreateProductRequest` — name, description?
- `UpdateProductRequest` — productId, name?, description?
- `DeleteProductRequest` — productId

### Licenses
- `CreateLicenseRequest` — productId, name?, expiresAt?, maxActivations
- `UpdateLicenseRequest` — licenseId, name?, expiresAt?, maxActivations?, status?
- `DeleteLicenseRequest` — licenseId
- `ValidateLicenseRequest` — licenseKey, hardwareId
- `DeactivateLicenseRequest` — licenseKey, hardwareId
- `ValidateLicenseResponse` — isValid, message, reusedActivation, currentActivations, maxActivations, expiresAt

## Middleware Pipeline

Order is critical — middleware executes in registration order:

1. **ExceptionHandlingMiddleware** — Catches all exceptions, maps to HTTP status codes (404, 401, 400)
2. **RateLimitMiddleware** — Applies IP-based rate limiting to `/api/auth` POST endpoints
3. **CookieToBearerMiddleware** — Reads `auth_token` cookie, injects as `Authorization: Bearer` header
4. **Authentication** — JWT Bearer or API Key via `SmartAuth` policy scheme
5. **Authorization** — Checks `[Authorize]` policies
6. **Controllers** — Request handling

### SmartAuth Hybrid Scheme

```
Request arrives
    │
    ├── Has "Authorization: Bearer ..." header?
    │   └── Yes → JWT Bearer authentication
    │
    └── Has "X-Api-Key" header?
        └── Yes → API Key authentication
```

The policy scheme auto-selects the correct authentication handler based on request headers.

### Rate Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| `/api/auth/login` | 10 | 1 minute |
| `/api/auth/register` | 5 | 5 minutes |
| `/api/auth/forgot-password` | 3 | 5 minutes |
| `/api/auth/reset-password/verify` | 6 | 5 minutes |
| `/api/auth/reset-password` | 3 | 5 minutes |

### Exception Mapping

| Exception | HTTP Status |
|-----------|-------------|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| Other exceptions | 400 Bad Request |

## Models

### Users

| Column | Type | Constraints |
|--------|------|-------------|
| Id | Guid | PK |
| Name | string | Required, max 40 |
| Email | string | Required, unique, normalized lowercase |
| PasswordHash | string | Required, bcrypt hash |
| CreatedAt | DateTime | UTC |
| IsSuspended | bool | Default false |
| ProductLimit | int | Default 3 |
| LicenseLimit | int | Default 450 |
| PasswordResetToken | string? | SHA-256 hash, null by default |
| PasswordResetTokenExpiry | DateTime? | 15-minute expiry |

**Relationships:** HasMany Products, HasMany ApiKeys

### Products

| Column | Type | Constraints |
|--------|------|-------------|
| Id | Guid | PK |
| UserId | Guid | FK → Users, required |
| Name | string | Required, max 40 |
| Description | string? | Optional, max 200 |
| CreatedAt | DateTime | UTC |

**Relationships:** BelongsTo User, HasMany Licenses

### Licenses

| Column | Type | Constraints |
|--------|------|-------------|
| Id | Guid | PK |
| ProductId | Guid | FK → Products, required |
| Name | string | Required, max 40 |
| LicenseKey | string | Required, unique, 4x4 format |
| Status | bool | Default true (active) |
| CreatedAt | DateTime | UTC |
| ExpiresAt | DateTime? | Nullable expiration |
| MaxActivations | int | Required, ≥ 1 |

**Relationships:** BelongsTo Product, HasMany Activations

### ApiKeys

| Column | Type | Constraints |
|--------|------|-------------|
| Id | Guid | PK |
| UserId | Guid | FK → Users, required |
| Name | string | Required, max 40 |
| KeyHash | string | Required, unique, SHA-256 hash |
| CreatedAt | DateTime | UTC |
| LastUsedAt | DateTime? | Updated on each authentication |
| IsActive | bool | Default true |

**Relationships:** BelongsTo User

### Activations

| Column | Type | Constraints |
|--------|------|-------------|
| Id | Guid | PK |
| LicenseId | Guid | FK → Licenses, required |
| HardwareId | string | Required, machine identifier |
| ActivatedAt | DateTime | UTC |
| LastSeenAt | DateTime? | Updated on validation |
| IsActive | bool | Default true |

**Relationships:** BelongsTo License

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
  "Email": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Secure": false,
    "Username": "...",
    "Password": "...",
    "From": "noreply@example.com"
  }
}
```

### Environment Variables (DotNetEnv)

Loaded from `.env` file:
- `database_connection` — PostgreSQL connection string
- `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`
- `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`

## Entity Relationships

```
Users (1) ────< (N) Products (1) ────< (N) Licenses (1) ────< (N) Activations
    │
    └───< (N) ApiKeys
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
