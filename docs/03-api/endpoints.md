# API Endpoints

OpenLicense exposes a versioned REST API with the prefix `/api/v1`.

## Base URL

- **Development:** `http://localhost:5049/api/v1`
- **Production:** `https://your-domain.com/api/v1`

## Versioning

URL path versioning (`/v{version}`). Current version: **v1**.

Future versions will be deployed alongside v1 during transition periods.

## Request/Response Format

### Success

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2026-09-05T12:00:00Z"
}
```

### Error

```json
{
  "message": "Password must contain at least one uppercase letter."
}
```

With FluentValidation:

```json
{
  "message": "Validation failed: Name is required. Email is required.",
  "errors": [
    { "PropertyName": "Name", "ErrorMessage": "Name is required." }
  ]
}
```

## Authentication

### JWT Bearer

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### API Key

```http
X-Api-Key: api_aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0kL1mN2oP3
```

### Hybrid "SmartAuth"

The API auto-selects auth type based on the present header:
- If `X-Api-Key` exists -> uses API Key
- If `Authorization: Bearer` exists -> uses JWT

## Endpoint Table

```mermaid
graph TD
    subgraph Auth["Authentication"]
        A1["POST /register\nNone"]
        A2["POST /login\nNone"]
        A3["POST /logout\nNone"]
        A4["GET /me\nJWT"]
        A5["PUT /\nJWT"]
        A6["DELETE /\nJWT"]
        A7["POST /apikey\nJWT"]
        A8["DELETE /apikey\nJWT"]
        A9["PUT /apikey/toggle\nJWT"]
        A10["POST /forgot-password\nNone"]
        A11["POST /reset-password/verify\nNone"]
        A12["POST /reset-password\nNone"]
        A13["PUT /report-preferences\nJWT"]
    end

    subgraph Products["Products"]
        P1["GET /all\nJWT"]
        P2["POST /create\nJWT"]
        P3["PUT /update\nJWT"]
        P4["DELETE /\nJWT"]
    end

    subgraph Licenses["Licenses"]
        L1["GET /\nJWT"]
        L2["POST /\nJWT"]
        L3["PUT /\nJWT"]
        L4["DELETE /\nJWT"]
        L5["GET /activations\nJWT"]
        L6["POST /validate\nAPI Key"]
        L7["POST /deactivate\nAPI Key"]
        L8["POST /deactivate-by-jwt\nJWT"]
        L9["PUT /activations/toggle\nJWT"]
    end

    subgraph Health["Health"]
        H1["GET /health\nNone"]
    end

    classDef auth fill:#e1f5fe,stroke:#01579b
    classDef product fill:#fff3e0,stroke:#e65100
    classDef license fill:#e8f5e9,stroke:#1b5e20
    classDef health fill:#f3e5f5,stroke:#4a148c
    classDef nodetext color:#333;

    class A1,A2,A3,A4,A5,A6,A7,A8,A9,A10,A11,A12,A13 auth,nodetext
    class P1,P2,P3,P4 product,nodetext
    class L1,L2,L3,L4,L5,L6,L7,L8,L9 license,nodetext
    class H1 health,nodetext
```


## Endpoint Details


### Authentication

#### POST `/api/v1/auth/register`

Register a new account.

**Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePass1!"
}
```

**Validation:**
- Name: Required, max 40 chars
- Email: Required, valid format, normalized to lowercase
- Password: Required, 8-128 chars, uppercase, lowercase, digit, special char

**Success (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2026-09-05T12:00:00Z",
  "productLimit": 3,
  "licenseLimit": 450
}
```

**Errors:**
- 400 -- Invalid data / Duplicate email
- 403 -- Registration disabled

#### POST `/api/v1/auth/login`

Login and obtain JWT token.

**Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass1!"
}
```

**Response Cookies:**
| Name | Attributes |
|------|-----------|
| `auth_token` | HttpOnly, Secure, SameSite=Strict, Expires=30min |

**Success (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Errors:**
- 401 -- Invalid credentials / Suspended account

#### GET `/api/v1/auth/me`

User profile with API keys.

**Headers:**
```
Authorization: Bearer <token>
```

**Success (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2026-09-05T12:00:00Z",
  "isSuspended": false,
  "productLimit": 3,
  "licenseLimit": 450,
  "apiKeys": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "name": "My API Key",
      "apiKey": "api_xxxxx...",
      "createdAt": "2026-09-05T13:00:00Z",
      "lastUsedAt": null,
      "isActive": true
    }
  ]
}
```

#### PUT `/api/v1/auth`

Update profile.

**Body:**
```json
{
  "name": "John Updated",
  "email": "john.updated@example.com",
  "password": "NewSecurePass1!"
}
```

**Errors:**
- 400 -- Duplicate email / Invalid data
- 401 -- Not authenticated
- 400 -- Email change rate limited (max 1 per 3 months)

#### DELETE `/api/v1/auth`

Delete account and all associated data.

**Success (204):** No content

---

### API Keys

#### POST `/api/v1/auth/apikey`

Create a new API key (max 3 per user).

**Body:**
```json
{
  "name": "Production API Key"
}
```

**Success (200):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "Production API Key",
  "apiKey": "api_aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0kL1mN2oP3",
  "createdAt": "2026-09-05T13:00:00Z",
  "isActive": true
}
```

**NOTE:** The `apiKey` field is returned only once at creation.

#### DELETE `/api/v1/auth/apikey`

Remove API key.

**Body:**
```json
{
  "apiKeyId": "660e8400-e29b-41d4-a716-446655440001"
}
```

#### PUT `/api/v1/auth/apikey/toggle`

Enable/disable API key.

**Body:**
```json
{
  "apiKeyId": "660e8400-e29b-41d4-a716-446655440001"
}
```

**Success (200):**
```json
{
  "isActive": true
}
```

**Behavior:**
- If active -> becomes inactive (can no longer authenticate)
- If inactive -> becomes active (can authenticate again)
- Soft toggle (does not delete the record)

#### Password Recovery

- **POST** `/api/v1/auth/forgot-password` -- Sends recovery email
- **POST** `/api/v1/auth/reset-password/verify` -- Validates reset token
- **POST** `/api/v1/auth/reset-password` -- Resets password with valid token

#### PUT `/api/v1/auth/report-preferences`

Update preferences for periodic email reports.

**Body:**
```json
{
  "reportsOptIn": true
}
```

---

### Products

#### GET `/api/v1/products/all`

List authenticated user's products.

**Success (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My SaaS Product",
    "createdAt": "2026-09-05T12:00:00Z",
    "licenses": []
  }
]
```

#### POST `/api/v1/products/create`

Create a new product (max 3 per user).

**Body:**
```json
{
  "name": "My New Product",
  "description": "Optional description (max 200 chars)"
}
```

**Success (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My New Product",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2026-09-05T12:00:00Z"
}
```

#### PUT `/api/v1/products/update`

Update product.

**Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Product Name"
}
```

#### DELETE `/api/v1/products`

Delete product and all associated licenses.

**Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

### Licenses

#### GET `/api/v1/licenses`

List licenses for a product.

**Query Parameters:**
| Parameter | Type | Required |
|-----------|------|----------|
| `productId` | Guid | Yes |

**Success (200):**
```json
[
  {
    "id": "aa0e8400-e29b-41d4-a716-446655440000",
    "name": "Premium License",
    "licenseKey": "A1B2-C3D4-E5F6-G7H8",
    "status": true,
    "createdAt": "2026-09-05T12:00:00Z",
    "expiresAt": "2027-01-01T00:00:00Z",
    "maxActivations": 5
  }
]
```

#### POST `/api/v1/licenses`

Create a new license.

**Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Premium License",
  "expiresAt": "2027-01-01T00:00:00Z",
  "maxActivations": 5
}
```

**Validation:**
- ProductId: Required, must belong to user
- Name: Optional, max 40 chars
- MaxActivations: Required, must be >= 1

#### PUT `/api/v1/licenses`

Update license.

**Body:**
```json
{
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Name",
  "status": false,
  "maxActivations": 10
}
```

**Error (400) -- Same Status:**
```json
{
  "message": "License is already in status suspended."
}
```

#### DELETE `/api/v1/licenses`

Delete license.

**Body:**
```json
{
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000"
}
```

#### GET `/api/v1/licenses/activations`

List activations for a license.

**Query Parameters:**
| Parameter | Type | Required |
|-----------|------|----------|
| `licenseId` | Guid | Yes |

**Success (200):**
```json
[
  {
    "id": "bb0e8400-e29b-41d4-a716-446655440000",
    "hardwareId": "HW-12345-ABCDEF",
    "activatedAt": "2026-09-05T12:00:00Z",
    "lastSeenAt": "2026-09-05T14:30:00Z",
    "isActive": true
  }
]
```

#### POST `/api/v1/licenses/validate`

Validate license key and activate hardware (uses API Key).

**Headers:**
```
X-Api-Key: api_xxxxxxxx...
```

**Body:**
```json
{
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "hardwareId": "HW-12345-ABCDEF"
}
```

**Success (200) -- New Activation:**
```json
{
  "isValid": true,
  "message": "License is valid.",
  "reusedActivation": false,
  "currentActivations": 1,
  "maxActivations": 5,
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

**Flow:**
1. Verify API key
2. Verify license key
3. Check license is active
4. Check not expired
5. Check activation limit
6. Create/Update activation

#### POST `/api/v1/licenses/deactivate`

Deactivate hardware (uses API Key).

**Body:**
```json
{
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "hardwareId": "HW-12345-ABCDEF"
}
```

#### POST `/api/v1/licenses/deactivate-by-jwt`

Deactivate hardware via JWT (dashboard).

#### PUT `/api/v1/licenses/activations/toggle`

Enable/disable activation (JWT only).

**Body:**
```json
{
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000",
  "hardwareId": "HW-12345-ABCDEF"
}
```

**Success (200):**
```json
{
  "isActive": true
}
```

**Error (400) -- Cannot Reactivate Inactive License:**
```json
{
  "message": "Cannot reactivate activation for an inactive license."
}
```

---

### Health Check

#### GET `/health`

Health check (excluded from OpenAPI documentation).

**Success (200):**
```json
{
  "status": "healthy"
}
```


## Rate Limiting

See [Error Handling — Rate Limiting](03-api/error-handling.md#rate-limiting) for configuration details.

## Formats

### API Key Format

```
api_aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0kL1mN2oP3
```

`api_` + 64 alphanumeric chars (A-Z, a-z, 0-9)

### License Key Format

```
A1B2-C3D4-E5F6-G7H8
```

4 groups of 4 alphanumeric chars separated by hyphen (A-Z, 0-9)


## Interactive Documentation

Scalar API reference available at:
- **Dev:** `http://localhost:5049/scalar/v1`
- **Prod:** `https://your-domain/scalar/v1`
