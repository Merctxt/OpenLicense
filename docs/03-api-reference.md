# API Reference

Complete reference for all Backend API endpoints.

## Base URL

Development: `http://localhost:5000`
Production: Depends on deployment configuration

## Authentication

The API uses two authentication methods:
- **JWT Bearer Token**: For dashboard/management operations
- **API Key**: For client-side license validation

Authentication is handled via a hybrid "SmartAuth" policy that auto-selects based on headers.

## Response Format

### Success Response
```json
{
  "message": "Success message.",
  "data": { /* response body */ }
}
```

### Error Response
```json
{
  "message": "Error message from the server."
}
```

## Authentication Endpoints

### POST `/api/auth/register`

Register a new user account.

**Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePass1!"
}
```

**Validation Rules:**
- Name: Required, max 40 characters
- Email: Required, valid email format, will be normalized to lowercase
- Password: Required, 8-128 characters, must contain uppercase, lowercase, digit, and special character

**Success Response (200):**
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

**Error Response (400):**
```json
{
  "message": "Password must contain at least one uppercase letter, lowercase letter, digit, special character."
}
```

**Error Response (400) — Duplicate Email:**
```json
{
  "message": "User with the same email already exists."
}
```

---

### POST `/api/auth/login`

Authenticate user and receive JWT token.

**Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass1!"
}
```

**Success Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response Cookies:**
| Name | Value | Attributes |
|------|-------|-----------|
| `auth_token` | JWT token | `HttpOnly`, `Secure`, `SameSite=Strict`, `Expires=30min` |

**Error Response (401):**
```json
{
  "message": "Invalid credentials."
}
```

**Error Response (401) — Suspended Account:**
```json
{
  "message": "Account is suspended."
}
```

---

### POST `/api/auth/logout`

Clear the authentication cookie.

**Success Response (200):**
```json
{
  "ok": true
}
```

---

### GET `/api/auth/me`

Get current user profile with API keys.

**Headers:**
```
Authorization: Bearer <token>
```

**Success Response (200):**
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
      "apiKey": "api_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
      "createdAt": "2026-09-05T13:00:00Z",
      "lastUsedAt": null,
      "isActive": true
    }
  ]
}
```

**Error Response (401):**
```json
{
  "message": "Invalid credentials."
}
```

---

### PUT `/api/auth`

Update user profile.

**Headers:**
```
Authorization: Bearer <token>
```

**Body (all fields optional, at least one required):**
```json
{
  "name": "John Updated",
  "email": "john.updated@example.com",
  "password": "NewSecurePass1!"
}
```

**Success Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Updated",
  "email": "john.updated@example.com",
  "createdAt": "2026-09-05T12:00:00Z"
}
```

**Error Response (400) — Duplicate Email:**
```json
{
  "message": "Another user with the same email already exists."
}
```

---

### DELETE `/api/auth`

Delete user account and all associated data.

**Headers:**
```
Authorization: Bearer <token>
```

**Success Response (204):** No content

---

### POST `/api/auth/apikey`

Create a new API key (max 3 per user).

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "name": "Production API Key"
}
```

**Success Response (200):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "Production API Key",
  "apiKey": "api_aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0kL1mN2oP3",
  "createdAt": "2026-09-05T13:00:00Z",
  "isActive": true
}
```

**Important:** The `apiKey` value is only returned once at creation time. Store it securely.

**Error Response (400):**
```json
{
  "message": "API key limit reached for this account."
}
```

---

### DELETE `/api/auth/apikey`

Delete an API key.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "apiKeyId": "660e8400-e29b-41d4-a716-446655440001"
}
```

**Success Response (204):** No content

**Error Response (404):**
```json
{
  "message": "API key not found."
}
```

---

### POST `/api/auth/forgot-password`

Send password recovery email.

**Body:**
```json
{
  "email": "john@example.com"
}
```

**Success Response (200):**
```json
{
  "message": "If the email exists, a recovery token has been sent."
}
```

**Note:** Response is always successful even if email doesn't exist (security measure).

---

### POST `/api/auth/reset-password/verify`

Verify password reset token.

**Body:**
```json
{
  "email": "john@example.com",
  "token": "ABC123DEF456..."
}
```

**Success Response (200):**
```json
{
  "valid": true
}
```

**Error Response (400):**
```json
{
  "message": "Invalid or expired token."
}
```

---

### POST `/api/auth/reset-password`

Reset password with verified token.

**Body:**
```json
{
  "email": "john@example.com",
  "token": "ABC123DEF456...",
  "password": "NewSecurePass1!"
}
```

**Success Response (200):**
```json
{
  "message": "Password reset successfully."
}
```

**Error Response (401):**
```json
{
  "message": "Invalid or expired token."
}
```

## Products Endpoints

### GET `/api/products/all`

List all products owned by the authenticated user.

**Headers:**
```
Authorization: Bearer <token>
```

**Success Response (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My SaaS Product",
    "description": "A cloud-based SaaS application",
    "createdAt": "2026-09-05T12:00:00Z",
    "licenses": [
      {
        "id": "aa0e8400-e29b-41d4-a716-446655440000",
        "productId": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Premium License",
        "licenseKey": "A1B2-C3D4-E5F6-G7H8",
        "status": true,
        "createdAt": "2026-09-05T12:00:00Z",
        "expiresAt": "2027-01-01T00:00:00Z",
        "maxActivations": 5,
        "activations": []
      }
    ]
  }
]
```

---

### POST `/api/products/create`

Create a new product.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "name": "My New Product",
  "description": "Product description (optional, max 200 chars)"
}
```

**Validation:**
- Name: Required, max 40 characters
- Description: Optional, max 200 characters
- Product limit: 3 per user

**Success Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My New Product",
  "description": "Product description",
  "createdAt": "2026-09-05T12:00:00Z"
}
```

**Error Response (400):**
```json
{
  "message": "Product limit reached. You can only create up to 3 products."
}
```

---

### PUT `/api/products/update`

Update an existing product.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Product Name",
  "description": "Updated description"
}
```

**Success Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Product Name",
  "description": "Updated description"
}
```

**Error Response (404):**
```json
{
  "message": "Product not found."
}
```

---

### DELETE `/api/products`

Delete a product and all its licenses.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Success Response (204):** No content

**Error Response (404):**
```json
{
  "message": "Product not found."
}
```

## Licenses Endpoints

### GET `/api/licenses`

List licenses for a specific product.

**Headers:**
```
Authorization: Bearer <token>
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `productId` | Guid | Yes | Product ID to filter licenses |

**Success Response (200):**
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

**Error Response (400):**
```json
{
  "message": "ProductId is required."
}
```

**Error Response (404) — Product not found:**
```json
{
  "message": "Product not found."
}
```

---

### POST `/api/licenses`

Create a new license.

**Headers:**
```
Authorization: Bearer <token>
```

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
- ProductId: Required, must not be empty, must belong to user
- Name: Optional, max 40 characters
- ExpiresAt: Optional, ISO 8601 datetime
- MaxActivations: Required, must be ≥ 1

**Success Response (200):**
```json
{
  "id": "aa0e8400-e29b-41d4-a716-446655440000",
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Premium License",
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "status": true,
  "createdAt": "2026-09-05T12:00:00Z",
  "expiresAt": "2027-01-01T00:00:00Z",
  "maxActivations": 5
}
```

---

### PUT `/api/licenses`

Update a license.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Name",
  "expiresAt": "2027-06-01T00:00:00Z",
  "maxActivations": 10,
  "status": false
}
```

**Validation:**
- LicenseId: Required, must not be empty
- Name: Optional, max 40 characters
- ExpiresAt: Optional
- MaxActivations: Optional, must be ≥ 1
- Status: Optional, cannot set to same current value

**Success Response (200):**
```json
{
  "id": "aa0e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Name",
  "status": false
}
```

**Error Response (400) — Same Status:**
```json
{
  "message": "License is already in status suspended."
}
```

---

### DELETE `/api/licenses`

Delete a license.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000"
}
```

**Success Response (200):**
```json
{
  "message": "License deleted successfully."
}
```

---

### GET `/api/licenses/activations`

List activations for a specific license.

**Headers:**
```
Authorization: Bearer <token>
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `licenseId` | Guid | Yes | License ID to get activations for |

**Success Response (200):**
```json
[
  {
    "id": "bb0e8400-e29b-41d4-a716-446655440000",
    "licenseId": "aa0e8400-e29b-41d4-a716-446655440000",
    "hardwareId": "HW-12345-ABCDEF",
    "activatedAt": "2026-09-05T12:00:00Z",
    "lastSeenAt": "2026-09-05T14:30:00Z",
    "isActive": true
  }
]
```

**Error Response (404) — License not found:**
```json
{
  "message": "License not found."
}
```

---

### POST `/api/licenses/validate`

Validate a license key and activate hardware. This endpoint uses API Key authentication.

**Headers:**
```
X-Api-Key: api_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**Body:**
```json
{
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "hardwareId": "HW-12345-ABCDEF"
}
```

**Validation Flow:**
1. Check API key exists and is active
2. Verify license key exists and belongs to the product associated with the API key
3. Check license is active (not suspended)
4. Check license has not expired
5. Check activation limit
6. Create activation if under limit, or update LastSeen if already activated

**Success Response (200) — New Activation:**
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

**Success Response (200) — Reused Activation:**
```json
{
  "isValid": true,
  "message": "License is valid.",
  "reusedActivation": true,
  "currentActivations": 1,
  "maxActivations": 5,
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

**Error Response (404) — Invalid License:**
```json
{
  "message": "Invalid license."
}
```

**Error Response (400) — Inactive License:**
```json
{
  "message": "Inactive license."
}
```

**Error Response (400) — Activation Limit Reached:**
```json
{
  "message": "Activation limit reached."
}
```

---

### POST `/api/licenses/deactivate`

Deactivate hardware activation. Uses API Key authentication.

**Headers:**
```
X-Api-Key: api_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**Body:**
```json
{
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "hardwareId": "HW-12345-ABCDEF"
}
```

**Success Response (200):**
```json
{
  "message": "License deactivated successfully."
}
```

---

### POST `/api/licenses/deactivate-by-jwt`

Deactivate hardware activation. Uses JWT authentication.

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**
```json
{
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "hardwareId": "HW-12345-ABCDEF"
}
```

**Success Response (200):**
```json
{
  "message": "License deactivated successfully."
}
```

## Health Check

### GET `/health`

Check if the API is healthy. Excluded from OpenAPI documentation.

**Success Response (200):**
```json
{
  "status": "healthy"
}
```

## Error Codes

| HTTP Code | Description |
|-----------|-------------|
| 400 | Bad Request — Validation error or business rule violation |
| 401 | Unauthorized — Invalid or missing authentication |
| 404 | Not Found — Resource not found or access denied |
| 429 | Too Many Requests — Rate limit exceeded |

## Rate Limiting

Applied to `/api/auth` POST endpoints (IP-based sliding window):

| Endpoint | Limit | Window |
|----------|-------|--------|
| `/api/auth/login` | 10 | 1 minute |
| `/api/auth/register` | 5 | 5 minutes |
| `/api/auth/forgot-password` | 3 | 5 minutes |
| `/api/auth/reset-password/verify` | 6 | 5 minutes |
| `/api/auth/reset-password` | 3 | 5 minutes |

**Rate Limit Exceeded Response (429):**
```json
{
  "message": "Too many requests. Please try again later."
}
```

Headers:
- `Retry-After`: Seconds until retry allowed

## API Key Format

API keys follow the format: `api_` + 64 random alphanumeric characters (A-Z, a-z, 0-9).

```
api_aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0kL1mN2oP3
```

## License Key Format

License keys follow the format: 4 groups of 4 alphanumeric characters separated by hyphens.

```
A1B2-C3D4-E5F6-G7H8
```

Characters: A-Z, 0-9 only (uppercase).

## Data Models

### User
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2026-09-05T12:00:00Z",
  "isSuspended": false,
  "productLimit": 3,
  "licenseLimit": 450,
  "apiKeys": []
}
```

### Product
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My SaaS Product",
  "description": "Product description",
  "createdAt": "2026-09-05T12:00:00Z",
  "licenses": []
}
```

### License
```json
{
  "id": "aa0e8400-e29b-41d4-a716-446655440000",
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Premium License",
  "licenseKey": "A1B2-C3D4-E5F6-G7H8",
  "status": true,
  "createdAt": "2026-09-05T12:00:00Z",
  "expiresAt": "2027-01-01T00:00:00Z",
  "maxActivations": 5
}
```

### API Key
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "My API Key",
  "apiKey": "api_xxxxxxxx...",
  "createdAt": "2026-09-05T13:00:00Z",
  "lastUsedAt": null,
  "isActive": true
}
```

### Activation
```json
{
  "id": "bb0e8400-e29b-41d4-a716-446655440000",
  "licenseId": "aa0e8400-e29b-41d4-a716-446655440000",
  "hardwareId": "HW-12345-ABCDEF",
  "activatedAt": "2026-09-05T12:00:00Z",
  "lastSeenAt": "2026-09-05T14:30:00Z",
  "isActive": true
}
```

## Interactive API Documentation

The Scalar API reference is available at:
- Development: `http://localhost:5000/scalar/v1`
- Production: `https://your-domain/scalar/v1`
