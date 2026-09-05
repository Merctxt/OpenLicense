# Testing

## Overview

OpenLicense uses xUnit for integration testing with FluentAssertions for expressive assertions. The test suite contains **74 tests** that validate all API endpoints against a real PostgreSQL database.


## Architecture

### Sequential Execution

Tests run sequentially (`maxParallelThreads: 1`) because all tests share the same remote PostgreSQL database. Parallel execution would cause race conditions.


### Test Base Class

All tests inherit from `TestBase` which provides:

- **WebApplicationFactory**: In-memory test host with custom configuration
- **HttpClient**: Configured with auto-redirect disabled
- **Database Cleanup**: Truncates all tables (CASCADE) before each test
- **Test Helpers**:
  - `RegisterUser()` — Registers a new user with unique email
  - `LoginAndGetToken()` — Authenticates and returns JWT token
  - `GetAuthenticatedUser()` — Combines registration + login
  - `GenerateJwtToken()` — Creates JWT tokens for testing (symmetric key from appsettings)

### Test Utilities

- **TestExtensions**: Custom `DeleteAsJsonAsync()` extension for HttpClient (not natively supported by MVC.Testing)
- **GlobalUsings**: Common imports shared across all test files

### Test Data Strategy

Users are created with unique identifiers to prevent conflicts:

```csharp
var uniqueId = Guid.NewGuid().ToString("N")[..8];
var email = $"test.{uniqueId}@test.com";
```

Default credentials:
- Password: `TestPass1!` (meets all complexity requirements)
- Name: `User {uniqueId}` (auto-generated)

## Running Tests

### All Tests

```bash
dotnet test Tests/OpenLicense.Tests.csproj
```

### Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~RegisterTests"
dotnet test --filter "FullyQualifiedName~LoginTests"
dotnet test --filter "FullyQualifiedName~ProductsTests"
dotnet test --filter "FullyQualifiedName~LicensesTests"
```

### Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~ShouldRegisterWithValidData"
```

### Without Recompiling

```bash
dotnet test --no-build
```

## Configuration

Tests read from `Tests/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<host>;Port=<port>;Database=<dbname>;Username=<user>;Password=<password>"
  },
  "Jwt": {
    "SecretKey": "<jwt-secret>",
    "Issuer": "OpenLicenseApi",
    "Audience": "OpenLicenseApiUsers"
  }
}
```

Requirements:
- PostgreSQL is accessible at `DefaultConnection`
- `Jwt:SecretKey` matches the one used by the Backend

## Scenarios Covered

### Authentication (Auth)

| Scenario | Status |
|----------|--------|
| Register with valid data | Covered |
| Register returns 400 when email already exists | Covered |
| Register returns 400 when required fields are missing | Covered |
| Register returns 400 with invalid passwords (short, numeric, no uppercase, no lowercase, no special) | Covered |
| Register normalizes email to lowercase | Covered |
| Register returns 400 when name exceeds 40 characters | Covered |
| Login with valid credentials | Covered |
| Login returns 401 with non-existent email | Covered |
| Login returns 401 with incorrect password | Covered |
| Login normalizes email to lowercase | Covered |
| /me endpoint returns authenticated user data | Covered |
| /me endpoint returns 401 without authentication | Covered |
| Update user name | Covered |
| Update user email | Covered |
| Update user password | Covered |
| Update email returns 400 when email belongs to another user | Covered |
| Update without authentication returns 401 | Covered |
| Delete account | Covered |
| Delete without authentication returns 401 | Covered |
| Logout and re-authentication | Covered |
| Create API Key (limit of 3) | Covered |
| Delete API Key | Covered |
| Create 4th API Key returns 400 | Covered |
| Delete non-existent API Key returns 404 | Covered |

### Products

| Scenario | Status |
|----------|--------|
| Create product with valid data | Covered |
| Create product without authentication returns 401 | Covered |
| Product limit of 3 per user (create 3, 4th returns 400) | Covered |
| List user products | Covered |
| List products returns empty list when no products | Covered |
| List products without authentication returns 401 | Covered |
| Update owned product | Covered |
| Update non-existent product returns 404 | Covered |
| Update product belonging to another user returns 404 | Covered |
| Update product without authentication returns 401 | Covered |
| Delete owned product | Covered |
| Delete non-existent product returns 404 | Covered |
| Delete product belonging to another user returns 404 | Covered |
| Delete product without authentication returns 401 | Covered |

### Licenses and Activations

| Scenario | Status |
|----------|--------|
| List empty licenses | Covered |
| List licenses with data | Covered |
| List licenses without productId returns 400 | Covered |
| List licenses for non-existent product returns 404 | Covered |
| List licenses for product belonging to another user returns 404 | Covered |
| List licenses without authentication returns 401 | Covered |
| Create license with valid data | Covered |
| Generate unique license keys | Covered |
| Create license with empty productId returns 400 | Covered |
| Create license with MaxActivations=0 returns 400 | Covered |
| Create license for non-existent product returns 404 | Covered |
| Create license without authentication returns 401 | Covered |
| Update license name | Covered |
| Update license MaxActivations | Covered |
| Update license status | Covered |
| Update non-existent license returns 404 | Covered |
| Update license belonging to another user returns 404 | Covered |
| Update license already in same status returns 400 | Covered |
| Update license without authentication returns 401 | Covered |
| Delete owned license | Covered |
| Delete non-existent license returns 404 | Covered |
| Delete license belonging to another user returns 404 | Covered |
| Delete license without authentication returns 401 | Covered |
| List empty activations | Covered |
| List activations with data | Covered |
| List activations for non-existent license returns 404 | Covered |
| List activations without authentication returns 401 | Covered |

### Shared

| Scenario | Status |
|----------|--------|
| Health endpoint returns 200 | Covered |
| Non-existent endpoint returns 404 | Covered |
| Method not allowed returns 405 | Covered |
| Invalid JSON returns 400 | Covered |


## Framework Packages

| Package | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.9.3 | Test framework |
| FluentAssertions | 9.0.0 | Assertion library |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.18 | In-memory test host |
| Microsoft.NET.Test.Sdk | 17.13.0 | Test runner |
| coverlet.collector | 6.0.4 | Code coverage |
