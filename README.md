# OpenLicense

OpenLicense is a self-hosted software licensing platform that provides a complete solution for managing product licenses, API keys, and client activations. It consists of a RESTful API built with .NET 9 and a modern web dashboard built with React.


## Architecture

The project is composed of two main components:

### OpenLicense API

A .NET 9 Web API that handles all licensing operations, including user authentication, product registration, license key generation, and client-side license validation. It uses PostgreSQL as the database and provides interactive API documentation via Scalar.

**Key Technologies:**
- .NET 9
- ASP.NET Core
- Entity Framework Core (Npgsql/PostgreSQL)
- JWT Bearer Authentication
- API Key Authentication (dual-auth via policy scheme)
- Scalar / OpenAPI

### OpenLicense Web

A single-page application built with React and Vite that serves as the management dashboard for users to create and manage their products, licenses, and API keys.

**Key Technologies:**
- React 19
- React Router DOM v7
- Vite 8
- Axios


## Features

- **User Management**: Registration, login, profile updates, and account deletion.
- **Product Management**: Create, update, list, and delete software products.
- **License Management**: Generate license keys, set activation limits, define expiration dates, and control license status (active/suspended).
- **License Validation**: Client-facing endpoint to validate licenses against a product's API key, enforcing activation limits per hardware ID.
- **API Key Authentication**: Generate and manage scoped API keys for client-side integration.
- **Activation Tracking**: Track hardware-bound activations with last-seen timestamps.
- **Hybrid Authentication**: JWT for dashboard operations, API Key for client-side validation endpoints.
- **Interactive API Reference**: Scalar UI for exploring and testing endpoints.
- **Docker Support**: Pre-configured Docker Compose for local development and production deployment.

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)

### Environment Configuration

1. Clone the repository:

   ```bash
   git clone https://github.com/your-org/OpenLicense.git
   cd OpenLicense
   ```

2. Configure environment variables by creating a `.env` file at the project root follow the `.env.example`:



### Running with Docker Compose

```bash
docker-compose up -d
```

This starts both the API (port 5000) and frontend (port 3000) services, along with any configured dependencies.

### Running Locally

**API:**

```bash
cd OpenLicenseApi
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`. The Scalar API reference is available at `/scalar/v1`.

**Frontend:**

```bash
cd OpenLicenseWeb
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000` with hot-reload enabled.

---

## API Overview

The API is organized into three main areas:

### Authentication (`/api/auth`)

| Method | Endpoint           | Auth   | Description                                  |
|--------|--------------------|--------|----------------------------------------------|
| POST   | `/api/auth/register` | None | Register a new user account                  |
| POST   | `/api/auth/login`    | None | Authenticate and receive a JWT token         |
| GET    | `/api/auth/me`       | JWT   | Retrieve the authenticated user's profile    |
| PUT    | `/api/auth`          | JWT   | Update the authenticated user's profile      |
| DELETE | `/api/auth`          | JWT   | Delete the authenticated user's account      |
| POST   | `/api/auth/apikey`   | JWT   | Create a new API key for client integration  |
| DELETE | `/api/auth/apikey`   | JWT   | Revoke an existing API key                   |

### Products (`/api/products`)

| Method | Endpoint                | Auth | Description                     |
|--------|-------------------------|------|---------------------------------|
| GET    | `/api/products/all`     | JWT  | List all products               |
| POST   | `/api/products/create`  | JWT  | Create a new product            |
| PUT    | `/api/products/update`  | JWT  | Update an existing product      |
| DELETE | `/api/products`         | JWT  | Delete a product                |

### Licenses (`/api/licenses`)

| Method | Endpoint                        | Auth            | Description                               |
|--------|---------------------------------|-----------------|-------------------------------------------|
| GET    | `/api/licenses`                 | JWT  | List licenses for a product               |
| POST   | `/api/licenses`                 | JWT  | Create a new license                      |
| PUT    | `/api/licenses`                 | JWT  | Update a license (name, status, limits)   |
| DELETE | `/api/licenses`                 | JWT  | Delete a license                          |
| POST   | `/api/licenses/validate`        | API Key         | Validate and active a license key against hardware   |
| POST   | `/api/licenses/deactivate`      | JWT / API Key   | Deactivate a hardware activation          |


## License Validation Flow

The license validation endpoint (`POST /api/licenses/validate`) implements the following logic:

1. Validate that the API key exists and is active.
2. Verify that the license key exists and belongs to the product associated with the API key.
3. Check that the license is active (not suspended).
4. If an activation exists for the provided hardware ID, update its `LastSeen` timestamp and return a success response.
5. If no activation exists and the current activation count is below the license's maximum, create a new activation and return a success response.
6. If the activation limit has been reached, return an error indicating the limit has been exceeded.

### Request

```json
{
  "licenseKey": "45AH-4HJY-97MR-2O80",
  "hardwareId": "unique-device-identifier"
}
```

### Response (success)

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


## License Key Format

License keys follow the format of four groups of four alphanumeric characters separated by hyphens:

```
45AH-4HJY-97MR-2O80
```


## Hardware ID

The hardware ID is a unique identifier used to bind a license to a specific client environment. It does not strictly need to be a hardware hash; any stable, unique identifier (such as an installation UUID) can be used. The client application is responsible for generating and consistently reporting the same identifier across executions.

Common approaches per platform:

- **Windows**: `wmic csproduct get uuid`
- **macOS**: `ioreg -rd1 -c IOPlatformExpertDevice | grep IOPlatformUUID`
- **Linux**: `cat /etc/machine-id`


## Database

The project uses PostgreSQL with Entity Framework Core. The schema includes the following entities:

- **Users**: Account information with product and license limits.
- **Products**: Software products owned by users, each containing multiple licenses.
- **Licenses**: License keys bound to a product, with configurable activation limits and expiration.
- **ApiKeys**: API keys for client-side authentication, hashed using SHA-256.
- **Activations**: Hardware-bound activation records tracking each unique client installation.

### Migrations

To apply database migrations:

```bash
cd OpenLicenseApi
dotnet ef database update
```

To create a new migration:

```bash
dotnet ef migrations add MigrationName
```


## Development

### Project Structure

```
OpenLicense/
├── docker-compose.yml                # Docker Compose configuration
├── OpenLicenseApi/                   # .NET 9 API
│   ├── Controllers/                  # API endpoints
│   ├── Data/                         # EF Core DbContext
│   ├── DTOs/                         # Request/response models
│   ├── Extensions/                   # Service configuration extensions
│   ├── Migrations/                   # Database migrations
│   ├── Models/                       # Entity models
│   ├── Services/                     # Business logic
│   │   └── Interfaces/               # Service contracts
│   ├── validation/                   # Custom authentication handlers
│   └── Program.cs                    # Application entry point
├── OpenLicenseWeb/                   # React frontend
│   ├── src/
│   │   ├── api/                      # API client and endpoints
│   │   ├── components/               # Shared UI components
│   │   ├── context/                  # React contexts (Auth, Theme)
│   │   └── pages/                    # Page components
│   └── vite.config.js                # Vite configuration
└── README.md
```


## License

This project is licensed under the terms of the Custom Non-Commercial Software License. See the [LICENSE.md](LICENSE.md) file for details. Commercial use is prohibited without prior written permission.
