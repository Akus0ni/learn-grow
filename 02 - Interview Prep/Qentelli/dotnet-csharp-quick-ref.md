# .NET / C# / ASP.NET Core — Quick Reference

## C# Core Concepts

### async / await
```csharp
// Always return Task or Task<T> from async methods
public async Task<string> GetDataAsync()
{
    var result = await _httpClient.GetStringAsync(url);
    return result;
}
```
- `await` does NOT block the thread — releases it back to the pool
- Use `ConfigureAwait(false)` in library code to avoid deadlocks
- `Task.WhenAll(t1, t2)` — run multiple tasks in parallel
- Avoid `async void` — use `async Task` instead

### Dependency Injection (DI)
```csharp
// Program.cs / Startup.cs
builder.Services.AddScoped<IOrderService, OrderService>();   // per HTTP request
builder.Services.AddTransient<IEmailSender, EmailSender>();  // new instance every time
builder.Services.AddSingleton<ICache, RedisCache>();         // one instance for app lifetime
```
- **Scoped** — same instance within one request (most common for services)
- **Transient** — lightweight, stateless operations
- **Singleton** — shared state (thread-safety is your responsibility)

### LINQ
```csharp
var seniors = employees
    .Where(e => e.Age > 40)
    .OrderBy(e => e.Name)
    .Select(e => new { e.Name, e.Department })
    .ToList();

// Aggregate
var total = orders.Sum(o => o.Amount);
var grouped = orders.GroupBy(o => o.CustomerId);
```

### Generics & Interfaces
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
}
```

### Records (C# 9+)
```csharp
public record Product(int Id, string Name, decimal Price);
// Immutable by default, value equality, with-expression supported
var updated = product with { Price = 29.99m };
```

---

## ASP.NET Core

### Web API Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var product = await _service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateProductRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }
}
```

### Middleware Pipeline
```csharp
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```
- Middleware runs in order — sequence matters
- Short-circuit with `context.Response.WriteAsync()` + return

### Custom Middleware
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Before
        await _next(context);
        // After — response is done here
    }
}
```

### Configuration
```csharp
// appsettings.json → strongly typed
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

// Inject
public class MyService(IOptions<DatabaseOptions> opts) { }
```

### JWT Authentication
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => {
        opts.TokenValidationParameters = new() {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
```

---

## MVC Pattern

| Component | Responsibility |
|-----------|---------------|
| Model | Data + business logic |
| View | Presentation (Razor `.cshtml`) |
| Controller | Handles HTTP request, calls Model, returns View |

### Filter Pipeline (execution order)
1. Authorization filters
2. Resource filters
3. Action filters (before + after action)
4. Exception filters
5. Result filters

---

## Entity Framework Core

```csharp
// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Order>().HasIndex(o => o.CustomerId);
    }
}

// Query
var orders = await _context.Orders
    .Include(o => o.Items)
    .Where(o => o.Status == OrderStatus.Pending)
    .AsNoTracking()   // read-only — faster
    .ToListAsync();
```

- `AsNoTracking()` — use for read-only queries (better performance)
- Migrations: `dotnet ef migrations add <Name>` → `dotnet ef database update`
- N+1 problem: always use `Include()` or split queries

---

## Key Design Patterns

### Repository + Unit of Work
- Repository abstracts data access per entity
- Unit of Work groups multiple repository operations into one transaction

### CQRS (Command Query Responsibility Segregation)
- **Commands** — change state (Create, Update, Delete)
- **Queries** — read state (no side effects)
- Used with MediatR in .NET

```csharp
// MediatR command
public record CreateOrderCommand(Guid CustomerId, List<OrderItem> Items) : IRequest<Guid>;
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid> { ... }
```

### Options Pattern
Strongly-typed config injection via `IOptions<T>` — avoids magic strings.

---

## Performance Tips to Mention
- Use `async/await` throughout (no `.Result` or `.Wait()`)
- `AsNoTracking()` for read queries in EF Core
- Response caching / output caching for repeated reads
- Connection pooling (default in EF Core)
- Use `IEnumerable` for streaming; `List` only when needed
