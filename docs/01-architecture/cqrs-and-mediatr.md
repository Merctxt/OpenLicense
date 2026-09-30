# CQRS and MediatR

OpenLicense implements **CQRS** (Command Query Responsibility Segregation) using **MediatR** to separate reads from writes.

## CQRS Flow

```mermaid
flowchart TD
    subgraph HTTP
        Controller["Controller\n(Maps request)"]
    end

    subgraph MediatR
        Command["Command\nIRequest<TResponse>"]
        Pipeline["Pipeline Behavior\n(FluentValidation)"]
        Handler["Handler\nIRequestHandler<TCmd, TRes>"]
    end

    subgraph Application
        Service["Service Interface\n(IService)"]
    end

    subgraph Infrastructure
        Impl["Service Implementation\n(Database, External APIs)"]
    end

    Controller --> Command
    Command --> Pipeline
    Pipeline --> Handler
    Handler --> Service
    Service --> Impl
    Impl --> Database[("Database")]

    classDef http fill:#e1f5fe,stroke:#01579b
    classDef mediatr fill:#fff3e0,stroke:#e65100
    classDef app fill:#e8f5e9,stroke:#1b5e20
    classDef infra fill:#f3e5f5,stroke:#4a148c
    classDef nodetext color:#333;

    class Controller http,nodetext
    class Command,Pipeline,Handler mediatr,nodetext
    class Service app,nodetext
    class Impl infra,nodetext
```


## Pipeline Behavior (Automatic Validation)

OpenLicense uses **FluentValidation** with pipeline behavior to validate commands/queries automatically.

The `ValidationBehavior<TRequest, TResponse>` in `Application/Common/ValidationBehavior.cs` implements `IPipelineBehavior<TRequest, TResponse>`. Before each handler executes, it looks up any FluentValidation validators registered for the request type, validates all rules, and throws a `ValidationException` if any failures are found. If validation passes, it calls the next delegate in the pipeline.

The pipeline behavior is registered in `Program.cs` using MediatR's configuration: `cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))`. This ensures every command and query is validated before reaching its handler.


## Complete Operation Flow

```mermaid
flowchart TD
    A["[POST /api/v1/auth/register]"] --> B["AuthController.Register\n(RegisterRequest)"]
    B --> C["new RegisterUserCommand\n(name, email, password)"]
    C --> D["_mediator.Send(command)"]
    D --> E["Pipeline Behavior"]
    E --> F["FluentValidation\n(if validators exist)"]
    F --> G["RegisterUserCommandHandler.Handle"]
    G --> H["_authService.RegisterAsync"]
    H --> I["AuthService\n(Infrastructure)"]
    I --> J["BCrypt.Verify/Hash"]
    I --> K["Check email uniqueness"]
    I --> L["Save to database"]
    L --> M["Return User"]
    M --> N["{ id, name, email,\ncreatedAt, ... }"]

    classDef endpoint fill:#e1f5fe,stroke:#01579b
    classDef controller fill:#fff3e0,stroke:#e65100
    classDef mediatr fill:#e8f5e9,stroke:#1b5e20
    classDef infra fill:#f3e5f5,stroke:#4a148c
    classDef nodetext color:#333;

    class A endpoint,nodetext
    class B controller,nodetext
    class C,D,E,F,G mediatr,nodetext
    class H,I,J,K,L infra,nodetext
```

## Creating a New Command/Query

### Checklist

1. **DTO** (if needed) -> `Application/DTOs/MyRequest.cs`
2. **Command/Query** -> `Application/Commands/.../MyCommand.cs` or `Application/Queries/.../MyQuery.cs`
3. **Handler** -> `Application/Handlers/.../MyHandler.cs`
4. **Service interface** (if new service) -> `Application/Services/Interfaces/IMyService.cs`
5. **Implementation** -> `Infrastructure/Services/MyService.cs`
6. **DI Registration** -> `Infrastructure/DependencyInjection.cs`
7. **Controller** -> `Api/Controllers/MyController.cs`

### Complete Example

To create a new command or query, follow these steps in order:

1. **Define the Command or Query record** — A simple record type inheriting from `IRequest<TResponse>`, e.g. `CreateWidgetCommand(Guid UserId, string Name) : IRequest<Widget>`. This file goes in the appropriate `Commands/` or `Queries/` subdirectory under `Application/`.

2. **Create the Handler** — A class implementing `IRequestHandler<CreateWidgetCommand, Widget>` that injects the relevant service interface via constructor and calls the service method in its `Handle()` method. This file goes in the corresponding `Handlers/` subdirectory.

3. **Define the Service Interface** (if new) — An interface like `IWidgetService` with a method `Task<Widget> CreateAsync(Guid userId, string name)`. This lives in `Application/Services/Interfaces/`.

4. **Implement the Service** — The concrete class implementing the interface, e.g. `WidgetService`, which accesses the database via `AppDbContext`. This lives in `Infrastructure/Services/`.

5. **Register in Dependency Injection** — Add `services.AddScoped<IWidgetService, WidgetService>()` in `Infrastructure/DependencyInjection.cs`.

6. **Add the Controller Endpoint** — Create or extend a controller with an HTTP method attribute like `[HttpPost]`, extract the userId from claims, create the command instance, and call `_mediator.Send()`. This lives in `Api/Controllers/`.
