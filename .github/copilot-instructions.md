# ShortiFy - Copilot Instructions

## Project Overview

URL shortener built with .NET 10 Minimal APIs following **Vertical Slice Architecture** and **REPR pattern** (Request-Endpoint-Response). This is a learning project—keep implementations simple and focused.

## Architecture

### Vertical Slice Structure (planned)
```
src/ShortiFy/
├── Features/
│   ├── Shortify/          # POST /api/shortify
│   │   ├── ShortifyRequest.cs
│   │   ├── ShortifyEndpoint.cs
│   │   └── ShortifyResponse.cs
│   └── Unshortify/        # GET /api/un-shortify/{shortCode}
│       ├── GetShortRequest.cs
│       ├── GetShortEndpoint.cs
│       └── GetShortResponse.cs
├── Infrastructure/Persistence/
└── Shared/Models/
```

Each feature is self-contained with its own Request, Endpoint, and Response classes—no shared controllers.

### Key Patterns
- **REPR Pattern**: Each endpoint has dedicated Request/Response DTOs in the same feature folder
- **Cache-Aside**: Check Redis first → miss queries SQL Server → repopulate cache
- **Direct EF Core**: No repository abstraction—use `DbContext` directly in endpoints
- **DataAnnotations**: Use built-in `[Required]`, `[StringLength]` for validation

## Tech Stack Specifics

| Aspect | Choice | Notes |
|--------|--------|-------|
| Framework | .NET 10 | Minimal APIs only |
| Database | SQL Server + EF Core 10 | LocalDB for dev |
| Cache | Redis 7.x | For URL resolution |
| Logging | Serilog | Structured logging |
| Tracing | OpenTelemetry + Grafana | Distributed tracing |
| Testing | xUnit + Moq | Unit & integration |
| Tool | Bruno | For calling endpoints |

## Code Conventions

- **Namespace**: `SimoneCappelletti.ShortiFy` (configured in `Directory.Build.props`)
- **Nullable**: Enabled globally—handle nulls explicitly
- **Warnings as Errors**: All warnings fail the build
- **Async-First**: Use `async/await` for DB, cache, and I/O operations

## Commands

```bash
# Run the API (http://localhost:5080)
dotnet run --project src/ShortiFy

# Build solution
dotnet build ShortiFy.slnx

# Watch mode
dotnet watch --project src/ShortiFy
```

## Domain Model

```csharp
public class ShortUrl
{
    public int Id { get; set; }
    [Required] [StringLength(10)] public string ShortCode { get; set; }
    [Required] public string OriginalUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## When Adding Features

1. Create feature folder under `Features/` with Request, Endpoint, Response files
2. Register endpoint in `Program.cs` using `app.MapGet/Post`
3. Keep endpoint logic minimal—extract complex logic to services only when necessary
4. Add structured logging with Serilog context
