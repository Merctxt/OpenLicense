# Database Design

OpenLicense uses **PostgreSQL** with **Entity Framework Core** 9.0 and **Npgsql** 9.0.4.

## Entity Framework Core Setup

The `AppDbContext` in `Infrastructure/Persistence/AppDbContext.cs` defines DbSet properties for all five entities (Users, Products, Licenses, ApiKeys, Activations) and configures unique indexes on the LicenseKey and KeyHash fields in the OnModelCreating method.

## Entity Relationship Diagram

```mermaid
erDiagram
    USER ||--o{ PRODUCT : "owns"
    USER ||--o{ APIKEY : "creates"
    PRODUCT ||--o{ LICENSE : "has"
    LICENSE ||--o{ ACTIVATION : "tracks"

    USER {
        Guid id PK
        string name
        string email
        string passwordHash
        bool reportsOptIn
        bool isSuspended
        int productLimit
        int licenseLimit
        DateTime createdAt
    }

    PRODUCT {
        Guid id PK
        Guid userId FK
        string name
        string description
        DateTime createdAt
    }

    LICENSE {
        Guid id PK
        Guid productId FK
        string name
        string licenseKey
        bool status
        DateTime expiresAt
        int maxActivations
        DateTime createdAt
    }

    APIKEY {
        Guid id PK
        Guid userId FK
        string keyHash
        bool isActive
        DateTime createdAt
        DateTime lastUsedAt
    }

    ACTIVATION {
        Guid id PK
        Guid licenseId FK
        string hardwareId
        DateTime activatedAt
        DateTime lastSeenAt
        bool isActive
    }
```

## Connection String

The connection string is configured via the `DATABASE_CONNECTION` or `database_connection` environment variable, following the Npgsql format: `Host=<your-db-host>:<port>;Database=postgres;Username=postgres;Password=...`
