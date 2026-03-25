# Interview Cheatsheet — CI/CD, Microservices, .NET Core, MVC, Web API, Angular

Quick-reference for tomorrow. Read this 30 minutes before the interview.

---

## 1. C# Quick Hits

| Concept | One-Liner |
|---------|-----------|
| `async/await` | Non-blocking I/O — frees thread while waiting for DB/HTTP calls |
| `IEnumerable` vs `IQueryable` | `IEnumerable` = in-memory LINQ; `IQueryable` = translates to SQL |
| Dependency Injection | Built into .NET Core — register in `Program.cs`, inject via constructor |
| `record` | Immutable reference type with value-based equality (C# 9+) |
| `var` vs `dynamic` | `var` = compile-time type inference; `dynamic` = runtime resolution |
| Nullable reference types | `string?` — compiler warns on possible null dereference |
| `sealed` class | Cannot be inherited — slight perf benefit, design intent |
| `readonly` vs `const` | `const` = compile-time; `readonly` = runtime, set in constructor |
| Extension methods | `static` methods on `static` class, first param uses `this` keyword |
| LINQ | `Where`, `Select`, `GroupBy`, `OrderBy`, `Join`, `Aggregate` |

---

## 2. .NET Core / .NET 8

| Concept | Key Point |
|---------|-----------|
| Minimal APIs | `app.MapGet("/", () => "Hello")` — no controllers needed |
| Top-level statements | No `Main()` method needed in `Program.cs` |
| Configuration | `appsettings.json` + `IConfiguration` + Options pattern |
| Middleware pipeline | `app.UseRouting()` → `app.UseAuthentication()` → `app.UseAuthorization()` → `app.MapControllers()` |
| Hosting | Kestrel (built-in), behind reverse proxy (Nginx/IIS) in prod |
| Logging | Built-in `ILogger<T>`, pluggable (Serilog, NLog) |
| Health checks | `builder.Services.AddHealthChecks()` → `app.MapHealthChecks("/health")` |

---

## 3. MVC (Model-View-Controller)

```
Request → Routing → Controller → Model (data/logic) → View (Razor) → Response
```

| Concept | Key Point |
|---------|-----------|
| Controller | Inherits `Controller`, returns `IActionResult` |
| View | `.cshtml` files using Razor syntax (`@Model.Name`) |
| Model | POCO classes, can use Data Annotations for validation |
| ViewBag/ViewData/TempData | Pass data to views (prefer ViewModel instead) |
| Tag Helpers | `<a asp-controller="Home" asp-action="Index">` |
| Partial Views | `<partial name="_ProductCard" model="product" />` |
| Areas | Organize large apps into modules |
| Filters | Action filters, Authorization filters, Exception filters |
| Model Binding | Automatically maps request data to action parameters |
| Validation | `[Required]`, `[StringLength]`, `[Range]` + `ModelState.IsValid` |

---

## 4. Web API

| Concept | Key Point |
|---------|-----------|
| `[ApiController]` | Auto model validation, auto 400 responses, binding source inference |
| Return types | `ActionResult<T>`, `IActionResult`, or direct type |
| Routing | `[Route("api/[controller]")]` + `[HttpGet("{id}")]` |
| Content negotiation | Returns JSON by default, supports XML if configured |
| Versioning | URL (`/api/v1/products`), Header, Query string |
| Swagger | `builder.Services.AddSwaggerGen()` → auto API docs |
| CORS | `builder.Services.AddCors()` — required for Angular SPA |
| Authentication | JWT Bearer tokens — `[Authorize]` attribute |
| Rate limiting | Built-in in .NET 7+ with `AddRateLimiter()` |
| Problem Details | RFC 7807 standard error responses |

**HTTP Verbs:**
- `GET` = Read (idempotent)
- `POST` = Create
- `PUT` = Full update (idempotent)
- `PATCH` = Partial update
- `DELETE` = Remove (idempotent)

**Status Codes to Know:**
- `200 OK`, `201 Created`, `204 No Content`
- `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`
- `500 Internal Server Error`

---

## 5. Microservices

### Key Principles
- **Single Responsibility** — each service owns one bounded context
- **Independently Deployable** — own repo/pipeline, own database
- **Loosely Coupled** — communicate via APIs or message queues
- **Technology Agnostic** — each service can use different tech stack
- **Resilient** — failure in one service shouldn't cascade

### Communication Patterns
| Pattern | When | Example |
|---------|------|---------|
| Synchronous (HTTP/gRPC) | Need immediate response | GET product details |
| Asynchronous (Message Queue) | Fire-and-forget, eventual consistency | Order placed → send email |
| API Gateway | Single entry point, routing, auth | Ocelot, YARP, Azure API Management |
| Service Discovery | Dynamic service locations | Consul, Kubernetes DNS |

### Key Patterns
| Pattern | Purpose |
|---------|---------|
| API Gateway | Single entry point, cross-cutting concerns |
| Circuit Breaker | Prevent cascading failures (Polly library) |
| Saga | Distributed transactions across services |
| CQRS | Separate read/write models |
| Event Sourcing | Store state as sequence of events |
| Sidecar | Attach helper processes (logging, proxy) |
| Strangler Fig | Incrementally migrate monolith to microservices |
| Database per Service | Each service owns its data store |

### Monolith vs Microservices
| Monolith | Microservices |
|----------|---------------|
| Simple to develop/deploy initially | Complex infra, but scales independently |
| Single database | Database per service |
| One deployment unit | Many deployment units |
| Tight coupling | Loose coupling |
| Vertical scaling | Horizontal scaling |
| Easier debugging | Distributed tracing needed (Jaeger, Zipkin) |

---

## 6. Entity Framework Core

```csharp
// DbContext
public class AppDbContext : DbContext {
    public DbSet<Product> Products { get; set; }
}

// Registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MyDb"));  // or UseSqlServer()

// Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

| Concept | Key Point |
|---------|-----------|
| Code First | Define models → generate DB |
| Database First | Scaffold from existing DB |
| Migrations | Version control for your schema |
| Lazy vs Eager Loading | `.Include()` = eager; navigation props = lazy |
| AsNoTracking | Read-only queries — better performance |
| Transactions | `using var tx = await db.Database.BeginTransactionAsync()` |

---

## 7. Angular (Good to Have)

```
Angular CLI Commands:
  ng new my-app          # Create new project
  ng generate component  # Create component
  ng serve               # Dev server on :4200
  ng build --prod        # Production build
  ng test                # Run unit tests
```

| Concept | Key Point |
|---------|-----------|
| Components | `@Component({ selector, template, styles })` |
| Services | `@Injectable({ providedIn: 'root' })` — singleton |
| HttpClient | `this.http.get<Product[]>('/api/products')` returns Observable |
| Routing | `RouterModule.forRoot(routes)` in `app.module.ts` |
| Observables | RxJS — `subscribe()`, `pipe()`, `map()`, `switchMap()` |
| Two-way binding | `[(ngModel)]="name"` |
| Lifecycle hooks | `ngOnInit`, `ngOnDestroy`, `ngOnChanges` |
| Standalone components | No module needed (Angular 15+) |

---

## 8. CI/CD with GitHub Actions

### Key Vocabulary
| Term | Meaning |
|------|---------|
| CI (Continuous Integration) | Auto build + test on every push/PR |
| CD (Continuous Delivery) | Auto deploy to staging; manual approval for prod |
| CD (Continuous Deployment) | Auto deploy all the way to production |
| Pipeline | Automated sequence of build → test → deploy steps |
| Artifact | Output of a build (DLL, Docker image, npm package) |
| Runner | Machine that executes the workflow (GitHub-hosted or self-hosted) |

### GitHub Actions Structure
```yaml
name: CI Pipeline

on:                          # TRIGGER
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:                        # JOBS (run in parallel by default)
  build:
    runs-on: ubuntu-latest   # RUNNER
    steps:                   # STEPS (run sequentially)
      - uses: actions/checkout@v4          # Checkout code
      - uses: actions/setup-dotnet@v4      # Setup .NET SDK
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore                # Restore packages
      - run: dotnet build --no-restore     # Build
      - run: dotnet test --no-build        # Test
```

### Common CI/CD Steps for .NET
```bash
dotnet restore          # Download NuGet packages
dotnet build            # Compile the solution
dotnet test             # Run unit tests
dotnet publish -c Release -o ./publish   # Create deployment package
docker build -t myapp . # Build Docker image
docker push myapp       # Push to container registry
```

### GitHub Actions Key Features
- **Secrets**: `${{ secrets.MY_SECRET }}` — store API keys, connection strings
- **Environments**: `production`, `staging` — with approval gates
- **Matrix builds**: Test across multiple .NET versions/OS
- **Caching**: `actions/cache` — speed up NuGet restore
- **Artifacts**: `actions/upload-artifact` — save build outputs
- **Status badges**: `![CI](https://github.com/USER/REPO/actions/workflows/ci.yml/badge.svg)`

---

## 9. Docker Essentials

```dockerfile
# Multi-stage Dockerfile for .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MyService.dll"]
```

```yaml
# docker-compose.yml
services:
  product-service:
    build: ./src/ProductService
    ports: ["5001:8080"]
  order-service:
    build: ./src/OrderService
    ports: ["5002:8080"]
  gateway:
    build: ./src/ApiGateway
    ports: ["5000:8080"]
    depends_on:
      - product-service
      - order-service
```

---

## 10. Agile Quick Reference

| Concept | Key Point |
|---------|-----------|
| Sprint | 2-week iteration |
| Standup | Daily 15-min: yesterday, today, blockers |
| Sprint Planning | Pick stories from backlog for the sprint |
| Retrospective | What went well, what to improve |
| User Story | "As a [user], I want [feature], so that [benefit]" |
| Story Points | Relative effort estimation (Fibonacci: 1,2,3,5,8,13) |
| Velocity | Average story points completed per sprint |
| Kanban | Visual board: To Do → In Progress → Done |
| Definition of Done | Coded + tested + reviewed + deployed |

---

## 11. SQL & ORM Quick Reference

```sql
-- Joins
SELECT * FROM Orders o
INNER JOIN Products p ON o.ProductId = p.Id    -- matching rows only
LEFT JOIN Customers c ON o.CustomerId = c.Id   -- all orders, null if no customer

-- Aggregation
SELECT CategoryId, COUNT(*), AVG(Price)
FROM Products GROUP BY CategoryId HAVING COUNT(*) > 5

-- Window Functions
SELECT *, ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY Price DESC) as Rank
FROM Products

-- CTE
WITH TopProducts AS (
    SELECT * FROM Products WHERE Price > 100
)
SELECT * FROM TopProducts
```

---

## 12. SOLID Principles (Bonus — often asked)

| Principle | One-liner |
|-----------|-----------|
| **S** — Single Responsibility | One class, one reason to change |
| **O** — Open/Closed | Open for extension, closed for modification |
| **L** — Liskov Substitution | Subtypes must be substitutable for base types |
| **I** — Interface Segregation | Many small interfaces > one fat interface |
| **D** — Dependency Inversion | Depend on abstractions, not concretions |

---

## Quick Architecture Pitch (Practice saying this)

> "I built a microservices demo with three .NET Core services — ProductService and OrderService as Web APIs, and an API Gateway using MVC that aggregates them. Each service has its own in-memory database using EF Core, follows REST conventions, and is independently deployable. The Angular SPA talks only to the gateway. I set up CI/CD with GitHub Actions — every push triggers build, test, and Docker image creation. The pipeline uses multi-stage Docker builds for optimized images."
