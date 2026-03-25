# Concepts Explained — Deep Dive for Interview

---

## 1. CI/CD — Continuous Integration / Continuous Delivery

### What is CI (Continuous Integration)?
CI is the practice of **automatically building and testing** your code every time someone pushes changes or opens a pull request.

**Why it matters:**
- Catches bugs early — before they reach production
- Ensures the codebase is always in a buildable state
- Reduces "works on my machine" problems
- Gives confidence to merge code frequently

**In practice (GitHub Actions):**
```
Developer pushes code → GitHub detects push → Runs workflow →
  1. Checkout code
  2. Restore dependencies (dotnet restore)
  3. Build the solution (dotnet build)
  4. Run all tests (dotnet test)
  5. Report pass/fail status on the PR
```

### What is CD (Continuous Delivery vs Continuous Deployment)?

**Continuous Delivery** = Code is always in a *deployable* state. Deployment to production requires a manual approval step.

**Continuous Deployment** = Every change that passes CI is *automatically deployed* to production. No manual step.

Most companies use **Continuous Delivery** (with manual approval for production).

```
CI Pipeline:     Push → Build → Test → ✅ Pass
CD Pipeline:     ✅ Pass → Package → Deploy to Staging → [Manual Approval] → Deploy to Prod
```

### CI/CD Pipeline Components
1. **Source Control Trigger** — Push, PR, tag, schedule (cron)
2. **Build Stage** — Compile code, restore dependencies
3. **Test Stage** — Unit tests, integration tests, code coverage
4. **Package Stage** — Create Docker image, NuGet package, zip artifact
5. **Deploy Stage** — Push to server, Kubernetes, Azure App Service, AWS ECS
6. **Post-Deploy** — Smoke tests, health checks, rollback if failed

---

## 2. GitHub Actions — How It Works

GitHub Actions is GitHub's built-in CI/CD platform. Free for public repos, generous free tier for private.

### Core Concepts

**Workflow** — A YAML file in `.github/workflows/`. Defines the entire pipeline.
```yaml
# .github/workflows/ci.yml — this IS the pipeline definition
```

**Event/Trigger** — What starts the workflow.
```yaml
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 0 * * *'    # nightly build
  workflow_dispatch:         # manual trigger button in GitHub UI
```

**Job** — A group of steps that run on the same runner. Jobs run in parallel by default.
```yaml
jobs:
  build:
    runs-on: ubuntu-latest    # GitHub-hosted Linux runner
  test:
    runs-on: ubuntu-latest
    needs: build              # Makes test wait for build to finish
```

**Step** — A single task within a job. Either runs a shell command (`run:`) or uses a pre-built action (`uses:`).
```yaml
steps:
  - uses: actions/checkout@v4          # Pre-built action
  - run: dotnet build                  # Shell command
```

**Runner** — The machine executing the workflow.
- **GitHub-hosted**: `ubuntu-latest`, `windows-latest`, `macos-latest` — managed by GitHub
- **Self-hosted**: Your own machine — useful for private networks, GPU, special hardware

**Action** — A reusable unit of CI/CD logic. Think of it like a NuGet package but for pipelines.
- `actions/checkout@v4` — clones your repo
- `actions/setup-dotnet@v4` — installs .NET SDK
- `docker/build-push-action@v5` — builds and pushes Docker images

### Secrets and Variables
```yaml
env:
  CONNECTION_STRING: ${{ secrets.DB_CONNECTION }}   # Encrypted, never logged
  BUILD_CONFIG: Release                              # Plain variable
```
Set secrets in: GitHub repo → Settings → Secrets and variables → Actions

### Caching for Speed
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget-${{ hashFiles('**/*.csproj') }}
```
Caches NuGet packages so `dotnet restore` doesn't download everything every time.

### Artifacts
```yaml
- uses: actions/upload-artifact@v4
  with:
    name: published-app
    path: ./publish/
```
Saves build output so later jobs (like deploy) can download and use it.

---

## 3. Microservices Architecture — Explained

### What are Microservices?
An architectural style where an application is composed of **small, independent services** that:
- Run in their own process
- Communicate over lightweight protocols (HTTP, gRPC, message queues)
- Are organized around **business capabilities** (not technical layers)
- Can be deployed, scaled, and developed independently

### Real-World Analogy
**Monolith** = A restaurant where one person takes orders, cooks, serves, and cleans.
**Microservices** = A restaurant with a hostess (API Gateway), multiple chefs (services), a cashier, and cleaning staff — each focused on one job.

### When to Use Microservices
**Use when:**
- Team is large enough (multiple teams)
- Different parts of the app need different scaling
- You need independent deployment cycles
- Different parts have different technology needs

**Don't use when:**
- Small team (1-5 developers)
- Simple CRUD application
- You're just starting out (start monolith, extract later)

### Service Communication Deep Dive

**Synchronous (HTTP REST / gRPC):**
```
OrderService ──HTTP GET──► ProductService
              ◄── JSON ───
```
- Simple, familiar
- Tight coupling — caller waits for response
- If ProductService is down, OrderService fails too
- Solution: Circuit Breaker pattern (Polly)

**Asynchronous (Message Queue — RabbitMQ, Azure Service Bus, Kafka):**
```
OrderService ──publish──► [Message Queue] ──subscribe──► NotificationService
                                           ──subscribe──► InventoryService
```
- Loose coupling — publisher doesn't wait
- Resilient — messages are queued if consumer is down
- Eventual consistency — not immediate
- Best for: events, notifications, long-running processes

### API Gateway Pattern
The gateway sits between clients and microservices:
```
Client → API Gateway → ProductService
                     → OrderService
                     → UserService
```
**Responsibilities:**
- Request routing
- Authentication/Authorization
- Rate limiting
- Response aggregation (combine data from multiple services)
- Load balancing
- SSL termination

**Tools:** Ocelot, YARP (Microsoft), Azure API Management, Kong, Nginx

### Circuit Breaker Pattern (Polly)
Prevents cascading failures when a downstream service is unhealthy.

```
States: CLOSED (normal) → OPEN (failing, return error immediately) → HALF-OPEN (try again)
```

```csharp
// Using Polly
builder.Services.AddHttpClient("ProductService")
    .AddTransientHttpErrorPolicy(p =>
        p.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,   // After 3 failures
            durationOfBreak: TimeSpan.FromSeconds(30) // Open for 30s
        ));
```

### Database per Service
Each microservice owns its data. No shared database.

```
ProductService → ProductDB
OrderService   → OrderDB
UserService    → UserDB
```

**Challenge:** How do you query across services?
- **API Composition**: Gateway calls multiple services, combines in memory
- **CQRS**: Separate read model that aggregates data from events
- **Saga Pattern**: Coordinate distributed transactions

---

## 4. ASP.NET Core Web API — Explained

### What is Web API?
A framework for building HTTP-based services that can be consumed by any client (browser, mobile, other services). Returns data (usually JSON), not HTML.

### Controller-Based vs Minimal APIs

**Controller-Based (traditional):**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
        => await _db.Products.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _db.Products.FindAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```

**Minimal APIs (.NET 6+):**
```csharp
app.MapGet("/api/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product p ? Results.Ok(p) : Results.NotFound());
```

### `[ApiController]` Attribute — What It Does
1. **Automatic model validation** — returns 400 if `ModelState` is invalid
2. **Binding source inference** — `[FromBody]`, `[FromRoute]`, `[FromQuery]` inferred automatically
3. **Problem Details responses** — standardized error format (RFC 7807)

### Content Negotiation
The API returns data in the format the client requests:
```
Client sends: Accept: application/json → Gets JSON
Client sends: Accept: application/xml  → Gets XML (if configured)
```

### Middleware Pipeline (Order Matters!)
```csharp
var app = builder.Build();

app.UseExceptionHandler("/error");    // 1. Catch exceptions
app.UseHttpsRedirection();            // 2. Redirect HTTP → HTTPS
app.UseCors("AllowAngular");          // 3. CORS headers
app.UseAuthentication();              // 4. Who are you?
app.UseAuthorization();               // 5. Are you allowed?
app.UseRateLimiter();                 // 6. Too many requests?
app.MapControllers();                 // 7. Route to controllers
```

---

## 5. ASP.NET Core MVC — Explained

### How MVC Works
```
1. Browser sends request: GET /products/details/5
2. Routing matches: ProductsController.Details(id: 5)
3. Controller gets data from service/database
4. Controller passes Model to View
5. Razor View renders HTML with the data
6. HTML response sent to browser
```

### MVC vs Web API
| MVC | Web API |
|-----|---------|
| Returns HTML (Views) | Returns data (JSON/XML) |
| For server-rendered web apps | For SPAs, mobile, service-to-service |
| Inherits `Controller` | Inherits `ControllerBase` |
| Has `View()`, `RedirectToAction()` | Has `Ok()`, `NotFound()`, `CreatedAtAction()` |
| Uses Razor views (.cshtml) | No views |

### In Microservices Context
The **API Gateway/BFF** can use MVC to:
- Serve a server-rendered admin dashboard
- Provide views for health monitoring
- Act as a Backend-for-Frontend (BFF) that also serves APIs

---

## 6. Dependency Injection in .NET Core

### What is DI?
Instead of a class creating its own dependencies, they are **injected** from outside (usually via constructor).

### Service Lifetimes
```csharp
builder.Services.AddTransient<IEmailService, EmailService>();   // New instance every time
builder.Services.AddScoped<IOrderService, OrderService>();      // One per HTTP request
builder.Services.AddSingleton<ICacheService, CacheService>();   // One for entire app lifetime
```

| Lifetime | When | Example |
|----------|------|---------|
| Transient | Lightweight, stateless | Email formatter, validators |
| Scoped | Per-request state | DbContext, unit of work |
| Singleton | Shared state, expensive to create | Cache, configuration, HttpClient |

### Why DI Matters for Microservices
- Easy to swap implementations (e.g., swap real DB for in-memory in tests)
- Clean separation of concerns
- Makes unit testing possible (inject mocks)

---

## 7. Docker in Microservices

### Why Docker?
- **Consistency**: Same environment everywhere (dev, CI, prod)
- **Isolation**: Each service in its own container
- **Portability**: Run anywhere Docker runs
- **Scaling**: Spin up multiple instances easily

### Multi-Stage Build (Important for interviews)
```dockerfile
# Stage 1: Build (large image with SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore                  # Restore first = better layer caching
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2: Runtime (small image, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductService.dll"]
```

**Why multi-stage?**
- Build image: ~700MB (includes SDK, compilers)
- Runtime image: ~100MB (only ASP.NET runtime)
- Smaller = faster deploys, less attack surface

### Docker Compose for Local Development
Runs all services together with one command: `docker-compose up`

---

## 8. Testing in Microservices

### Test Pyramid
```
        /  E2E Tests  \        ← Few, slow, expensive
       / Integration    \      ← Some, moderate
      /   Unit Tests     \     ← Many, fast, cheap
```

### Unit Test Example (xUnit + Moq)
```csharp
public class ProductsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        using var context = new ProductDbContext(options);
        context.Products.Add(new Product { Name = "Test", Price = 9.99m });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<ActionResult<List<Product>>>(result);
        Assert.Single(okResult.Value);
    }
}
```

### Integration Test (WebApplicationFactory)
```csharp
public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## 9. Angular with .NET Microservices

### How Angular Fits
```
Angular SPA (browser) ──HTTP──► API Gateway ──► Microservices
```

- Angular runs in the browser, makes HTTP calls to the API Gateway
- CORS must be configured on the gateway to allow Angular's origin
- Angular uses `HttpClient` module to make REST calls
- Services in Angular handle API communication
- Components handle UI rendering

### Typical Angular Service
```typescript
@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'http://localhost:5000/api/products';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  create(product: Product): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }
}
```

---

## 10. Putting It All Together — The Full Picture

```
Developer pushes code to GitHub
        │
        ▼
GitHub Actions CI triggered
        │
        ├── Restore NuGet packages
        ├── Build all services
        ├── Run unit tests
        ├── Run integration tests
        ├── Build Docker images
        └── Push to Container Registry
                │
                ▼
CD Pipeline deploys to:
        │
        ├── Staging (automatic)
        │       └── Run smoke tests
        │
        └── Production (manual approval)
                ├── Deploy ProductService container
                ├── Deploy OrderService container
                ├── Deploy API Gateway container
                └── Deploy Angular SPA to CDN/static hosting
```

This is the story you tell in the interview. You understand the full lifecycle from code to production.
