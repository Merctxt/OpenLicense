# Development Guide

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 17+](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional, for containerized development)
- [VS Code](https://code.visualstudio.com/) or Visual Studio 2022

## Project Structure

```
OpenLicense/
├── Backend/              # .NET 9.0 API
├── Frontend/             # React/Vite SPA
├── Tests/                # Integration tests
├── docs/                 # Documentation
├── .vscode/              # VS Code settings
├── docker-compose.yml    # Docker orchestration
├── .env.example          # Environment variables template
├── .gitignore
└── README.md
```

## Quick Start

### 1. Clone and Setup

```bash
git clone <repository-url>
cd OpenLicense
```

### 2. Database

Start a PostgreSQL instance. The simplest option:

```bash
docker run -d --name openlicense-db \
  -e POSTGRES_DB=openlicense \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=secret \
  -p 5432:5432 \
  postgres:17-alpine
```

Or use any external PostgreSQL instance (Azure Database for PostgreSQL, etc.).

### 3. Backend

```bash
cd Backend

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API (default: http://localhost:5000)
dotnet run
```

The backend loads configuration from:
- `appsettings.json` (base config)
- `appsettings.Development.json` (dev overrides)
- Environment variables (override JSON config)

### 4. Frontend

```bash
cd Frontend

# Install dependencies
npm install

# Start dev server (default: http://localhost:3000)
npm run dev
```

The dev server proxies `/api/*` requests to `http://localhost:7224` (or wherever the backend is running).

### 5. Open in Browser

Visit `http://localhost:3000` to access the application.

## VS Code Setup

### Recommended Extensions

- **C# Dev Kit** — IntelliSense, debugging, project management
- **C# Extensions** — NuGet support, formatting
- **ESLint** — JavaScript/TypeScript linting
- **Prettier** — Code formatting
- **Error Lens** — Inline error highlighting
- **REST Client** — HTTP request testing

### Debugging

The `.vscode/launch.json` includes a configuration to launch Edge with source maps:

```json
{
  "type": "msedge",
  "request": "launch",
  "name": "Launch Edge against localhost",
  "url": "http://localhost:3000",
  "webRoot": "${workspaceFolder}/Frontend",
  "sourceMaps": true
}
```

Press `F5` to start debugging the frontend in Edge.

### Backend Debugging

In VS Code:
1. Open `Backend/` folder (or the workspace root)
2. Set breakpoints in C# files
3. Press `F5` (choose ".NET Core Launch (web)")

Or in Visual Studio:
1. Open the solution file
2. Set to start `Backend` project
3. Press `F5`

## Development Workflow

### Local Development (All Services)

```bash
# Terminal 1: Database (if not using external)
docker run -d --name openlicense-db \
  -e POSTGRES_DB=openlicense \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=secret \
  -p 5432:5432 postgres:17-alpine

# Terminal 2: Backend
cd Backend
dotnet run

# Terminal 3: Frontend
cd Frontend
npm run dev
```

### Port Configuration

| Service | Default Port |
|---------|-------------|
| Backend API | 5000 |
| Frontend Dev Server | 3000 |
| PostgreSQL | 5432 |
| Scalar API Docs | 5000/scalar/v1 |

### Creating a New Entity

1. Create the model class in `Backend/Models/`
2. Add `DbSet<T>` to `AppDbContext`
3. Create migration: `dotnet ef migrations add <Name>`
4. Apply migration: `dotnet ef database update`
5. Add controller in `Backend/Controllers/`
6. Add service in `Backend/Services/`
7. Write integration tests in `Tests/Features/`

### Adding a New Controller

1. Create `Backend/Controllers/<Name>Controller.cs`
2. Inherit from `ApiController` or use minimal API patterns
3. Add endpoint routes with `[HttpGet]`, `[HttpPost]`, etc.
4. Add SmartAuth policy attributes where needed
5. Register in `Program.cs` if using route groups
6. Add corresponding frontend API functions in `Frontend/src/api/endpoints.js`

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply pending migrations
dotnet ef database update

# Remove last migration (only if not applied)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list
```

## API Development

### Adding a New Endpoint

1. **Backend Controller:**
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class MyController : ControllerBase
   {
       [HttpGet]
       public IActionResult Get() { ... }
   }
   ```

2. **Route Auth Registration:**
   Update `Backend/Models/RouteAuth.cs` if the endpoint needs specific auth.

3. **Frontend API Function:**
   Add to `Frontend/src/api/endpoints.js`:
   ```javascript
   export async function getMyResource() {
     return client.get('/api/my');
   }
   ```

4. **Integration Test:**
   Add to `Tests/Features/` with appropriate test class.

### API Key Auth Endpoints

For client-side validation endpoints:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ValidateController : ControllerBase
{
    [HttpPost("validate")]
    [ServiceFilter(typeof(ApiKeyAuthMiddleware))]
    public IActionResult Validate([FromBody] ValidateRequest request) { ... }
}
```

## Frontend Development

### Adding a New Page

1. Create `Frontend/src/pages/MyPage/MyPage.jsx`
2. Create `Frontend/src/pages/MyPage/useMyPage.js` (hook)
3. Add route in `Frontend/src/App.jsx`
4. Add navigation link in `Frontend/src/components/Layout.jsx`
5. Add API functions in `Frontend/src/api/endpoints.js`

### Adding a New Component

1. Create `Frontend/src/components/MyComponent.jsx`
2. Export as default
3. Import and use in pages

### State Management

The app uses React Context for global state:
- `AuthContext` — Authentication state
- `ThemeContext` — Theme preferences

Page-specific state is managed via custom hooks (`use*.js`).

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true
```

### Filter Tests

```bash
dotnet test --filter "Category=Integration"
dotnet test --filter "FullyQualifiedName~RegisterTests"
```

## Debugging Tips

### Backend

- Check logs: `dotnet run` outputs to console
- Use `ILogger<T>` for structured logging
- Enable detailed errors in development: `appsettings.Development.json`

### Frontend

- React DevTools browser extension
- Network tab in browser DevTools to inspect API calls
- Check Axios interceptors in `Frontend/src/api/client.js`

### Database

- Connect with any PostgreSQL client (DBeaver, pgAdmin, VS Code pg extension)
- Use `dotnet ef dbcontextinfo` to visualize schema
- Check EF Core logging: set `logging:LogLevel:Microsoft.EntityFrameworkCore` to `Debug`

## Common Issues

### CORS Errors

Ensure the frontend origin is in `Cors:AllowedOrigins` in `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": "http://localhost:3000"
  }
}
```

### JWT Authentication Failures

- Check that `Jwt:SecretKey` matches in both backend and test config
- Verify token is not expired (30-minute default)
- Check cookie settings for browser-based auth

### Database Connection Issues

- Verify connection string format: `Host=...;Port=5432;Database=...;Username=...;Password=...`
- Ensure PostgreSQL is accessible from the host
- Check firewall rules for external databases

### Port Conflicts

- Backend default: 5000 (configure via `Properties/launchSettings.json`)
- Frontend default: 3000 (configure in `Frontend/vite.config.js`)
- PostgreSQL default: 5432
