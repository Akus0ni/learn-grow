# .NET Core 8 — Mock Interview Q&A

> Current LTS release. Cross-platform, container-native, high performance. The active standard for all new .NET development.

---

## Architecture & Runtime

---

**Q: What's new and notable in .NET 8 specifically?**

| Feature | Details |
|---|---|
| **Native AOT (Ahead-of-Time)** | Compile to native binary — no JIT, smaller size, faster startup |
| **Primary constructors** (C# 12) | Concise constructor syntax on classes (not just records) |
| **Frozen collections** | `FrozenDictionary`, `FrozenSet` — immutable, read-optimized |
| **`TimeProvider` abstraction** | Testable time — inject `TimeProvider` instead of `DateTime.Now` |
| **`IHostedLifecycleService`** | Finer lifecycle hooks for hosted services |
| **Keyed DI services** | Register multiple implementations of same interface with a key |
| **`System.Text.Json` improvements** | `[JsonIgnore(Condition)]`, interface serialization, custom converters |
| **Blazor improvements** | Unified Blazor (server + wasm), streaming rendering |

---

**Q: What is the difference between `dotnet run`, `dotnet build`, and `dotnet publish`?**

| Command | Does |
|---|---|
| `dotnet build` | Compiles → produces DLLs in `bin/` (not self-contained) |
| `dotnet run` | Builds + runs in-process (dev only) |
| `dotnet publish` | Produces deployment artifact — includes runtime files if self-contained |

```bash
# Framework-dependent (host needs .NET 8 installed)
dotnet publish -c Release -r linux-x64

# Self-contained (no runtime needed on host)
dotnet publish -c Release -r linux-x64 --self-contained true

# Single-file publish
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true
```

---

**Q: What is Native AOT and when would you use it?**

Native AOT compiles the entire app to a native binary at publish time. No JIT, no CLR startup cost.

**Benefits:**
- Near-instant startup (microseconds vs. hundreds of milliseconds)
- Small binary size
- No JIT memory overhead

**Limitations:**
- No dynamic code (no `Assembly.Load`, limited reflection)
- Longer build times
- Not all NuGet packages compatible

**Best for:** Cloud functions, CLI tools, microservices with extreme cold-start requirements.

---

## Dependency Injection

---

**Q: What are the three DI lifetimes in ASP.NET Core and when do you use each?**

| Lifetime | Created | Use For |
|---|---|---|
| **Singleton** | Once per app lifetime | Shared state, configuration, expensive setup (e.g., `HttpClient` factory, caches) |
| **Scoped** | Once per HTTP request | DbContext, unit-of-work patterns |
| **Transient** | Every time requested | Lightweight, stateless services |

```csharp
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IEmailFormatter, EmailFormatter>();
```

**Captive dependency bug:** A Singleton that takes a Scoped dependency — the scoped instance is captured forever. ASP.NET Core throws `InvalidOperationException` in development mode if detected.

---

**Q: What are keyed services in .NET 8 DI?**

Keyed services let you register multiple implementations of the same interface, distinguished by a key:

```csharp
// Registration
builder.Services.AddKeyedScoped<IPaymentGateway, StripeGateway>("stripe");
builder.Services.AddKeyedScoped<IPaymentGateway, PayPalGateway>("paypal");

// Resolution in constructor
public class CheckoutService
{
    private readonly IPaymentGateway _gateway;

    public CheckoutService([FromKeyedServices("stripe")] IPaymentGateway gateway)
    {
        _gateway = gateway;
    }
}
```

---

**Q: How do you register an open generic type in DI?**

```csharp
// Register open generic
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Usage — DI resolves IRepository<Order> → Repository<Order>
public class OrderService
{
    public OrderService(IRepository<Order> repo) { ... }
}
```

---

## ASP.NET Core Pipeline

---

**Q: Explain the ASP.NET Core middleware pipeline.**

Middleware is a chain of components. Each component can:
1. Do work before passing to the next (`next()`)
2. Short-circuit (return without calling `next()`)
3. Do work after the next returns

```csharp
// Custom middleware
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);           // call next middleware
        sw.Stop();
        // log sw.ElapsedMilliseconds
    }
}

// Registration order matters
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();    // must be before Authorization
app.UseAuthorization();
app.MapControllers();
```

---

**Q: What is the difference between `app.Use`, `app.Run`, and `app.Map`?**

```csharp
// app.Use — passes to next middleware
app.Use(async (context, next) =>
{
    // before
    await next(context);
    // after
});

// app.Run — terminal, never calls next
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello");
});

// app.Map — branches pipeline based on path
app.Map("/health", healthApp =>
{
    healthApp.Run(async ctx => await ctx.Response.WriteAsync("OK"));
});
```

---

**Q: What is minimal API and how does it differ from controller-based API?**

Minimal APIs (introduced .NET 6) define endpoints directly in `Program.cs` without controllers:

```csharp
// Minimal API
app.MapGet("/products/{id}", async (int id, IProductService svc) =>
{
    var product = await svc.GetByIdAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/products", async (CreateProductRequest req, IProductService svc) =>
{
    var created = await svc.CreateAsync(req);
    return Results.Created($"/products/{created.Id}", created);
});
```

**When to use what:**
- **Minimal API:** Microservices, simple CRUD, high-performance scenarios (less overhead)
- **Controller-based:** Complex apps needing filters, model binding, action conventions, versioning

---

## Entity Framework Core

---

**Q: What's the difference between EF Core and EF6?**

| | EF6 | EF Core |
|---|---|---|
| **Platform** | .NET Framework only | Cross-platform |
| **LINQ translation** | Limited | Improved, more operations translated to SQL |
| **Performance** | Slower | Faster (bulk operations, compiled queries) |
| **Migrations** | Package Manager Console | CLI or PMC |
| **Lazy loading** | Enabled by default | Opt-in |
| **Split queries** | No | Yes — avoids cartesian explosion |
| **Bulk operations** | No native support | `ExecuteUpdate`/`ExecuteDelete` in EF Core 7+ |

---

**Q: What is the N+1 query problem and how do you fix it in EF Core?**

```csharp
// N+1 PROBLEM — 1 query for orders + N queries for customers
var orders = await context.Orders.ToListAsync();
foreach (var order in orders)
    Console.WriteLine(order.Customer.Name); // lazy load per order

// FIX 1: Eager loading with Include
var orders = await context.Orders
    .Include(o => o.Customer)
    .ToListAsync();

// FIX 2: Split query (avoids cartesian explosion with multiple Includes)
var orders = await context.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
    .AsSplitQuery()
    .ToListAsync();

// FIX 3: Projection — only select what you need
var summaries = await context.Orders
    .Select(o => new { o.Id, CustomerName = o.Customer.Name })
    .ToListAsync();
```

---

**Q: What are compiled queries in EF Core and when should you use them?**

Compiled queries cache the LINQ-to-SQL translation, eliminating the overhead on every execution:

```csharp
// Define once — translation compiled and cached
private static readonly Func<AppDbContext, int, Task<Order?>> GetOrderById =
    EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
        ctx.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id));

// Use in hot paths
var order = await GetOrderById(context, orderId);
```

Use compiled queries in hot paths called thousands of times per second.

---

**Q: What is `ExecuteUpdate` and `ExecuteDelete` in EF Core 7+?**

Bulk operations that execute directly in the DB without loading entities into memory:

```csharp
// Delete all expired sessions — no load into memory
await context.Sessions
    .Where(s => s.ExpiresAt < DateTime.UtcNow)
    .ExecuteDeleteAsync();

// Update all prices in a category
await context.Products
    .Where(p => p.CategoryId == categoryId)
    .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.Price, p => p.Price * 1.1m)
        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
```

---

## Performance

---

**Q: What is `Span<T>` and when should you use it?**

`Span<T>` is a stack-allocated, contiguous memory view. It allows slicing strings/arrays without heap allocation:

```csharp
// Without Span — allocates new string
string csv = "Alice,Bob,Charlie";
string first = csv.Split(',')[0]; // allocates array + new string

// With Span — zero allocation
ReadOnlySpan<char> span = csv.AsSpan();
int comma = span.IndexOf(',');
ReadOnlySpan<char> firstName = span.Slice(0, comma); // no allocation

// Parsing without allocation
bool parsed = int.TryParse(span.Slice(0, 3), out int value);
```

**Rule:** Use `Span<T>` in parsing, serialization, and data processing hot paths to reduce GC pressure.

---

**Q: What is `IAsyncEnumerable<T>` and when would you use it?**

`IAsyncEnumerable<T>` streams data asynchronously, item by item, instead of loading everything into memory:

```csharp
// Producer — yields items as they arrive
public async IAsyncEnumerable<Order> GetLargeOrderSetAsync()
{
    await foreach (var order in dbContext.Orders.AsAsyncEnumerable())
    {
        yield return order;
    }
}

// Consumer — processes without buffering all records
await foreach (var order in GetLargeOrderSetAsync())
{
    await ProcessOrderAsync(order);
}
```

**Use cases:** Streaming DB results, large file processing, real-time event feeds.

---

**Q: What are `ValueTask<T>` and when should you use it over `Task<T>`?**

`ValueTask<T>` avoids heap allocation when a result is often available synchronously (cache hits, fast paths):

```csharp
// Use Task<T> — default for most async methods
public async Task<string> GetFromApiAsync() { ... }

// Use ValueTask<T> — when result is frequently cached/synchronous
private readonly Dictionary<int, Product> _cache = new();

public ValueTask<Product?> GetProductAsync(int id)
{
    if (_cache.TryGetValue(id, out var cached))
        return ValueTask.FromResult<Product?>(cached); // no allocation

    return new ValueTask<Product?>(FetchFromDbAsync(id)); // async path
}
```

**Warning:** Don't `await` a `ValueTask` more than once, and don't store them — they're single-use.

---

## Configuration & Options

---

**Q: How does configuration work in ASP.NET Core 8?**

Configuration is layered — later sources override earlier ones:

```csharp
// Default order (WebApplication.CreateBuilder):
// 1. appsettings.json
// 2. appsettings.{Environment}.json
// 3. User secrets (Development only)
// 4. Environment variables
// 5. Command-line arguments

var builder = WebApplication.CreateBuilder(args);

// Bind to strongly-typed class
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));
```

```json
// appsettings.json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587
  }
}
```

```csharp
// Consuming options
public class EmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }
}
```

---

**Q: What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?**

| | Lifetime | Reloads on change |
|---|---|---|
| `IOptions<T>` | Singleton | No |
| `IOptionsSnapshot<T>` | Scoped (per request) | Yes (per request) |
| `IOptionsMonitor<T>` | Singleton | Yes (real-time, with `OnChange` callback) |

Use `IOptionsMonitor<T>` for services that need live config updates (feature flags, rate limits).

---

## Security

---

**Q: How do you implement JWT authentication in ASP.NET Core 8?**

```csharp
// Registration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Pipeline
app.UseAuthentication();
app.UseAuthorization();

// Token generation
public string GenerateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

**Q: What is the difference between Authentication and Authorization in ASP.NET Core?**

- **Authentication** — Who are you? (`[Authorize]` without policy triggers this)
- **Authorization** — What can you do? (roles, claims, policies)

```csharp
// Role-based
[Authorize(Roles = "Admin,Manager")]
public IActionResult AdminPanel() { ... }

// Policy-based (more flexible)
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("SeniorEmployee", policy =>
        policy.RequireClaim("YearsOfService")
              .RequireRole("Employee"));
});

[Authorize(Policy = "SeniorEmployee")]
public IActionResult SeniorPortal() { ... }
```

---

## Testing

---

**Q: How do you write unit tests for an ASP.NET Core controller?**

```csharp
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _controller = new ProductsController(_mockService.Object);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Widget" };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(product, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductMissing()
    {
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Product?)null);
        var result = await _controller.GetById(99);
        Assert.IsType<NotFoundResult>(result);
    }
}
```

---

**Q: What is `WebApplicationFactory` and when do you use it?**

`WebApplicationFactory<T>` spins up the full ASP.NET Core pipeline in-memory for integration tests — no ports, no network:

```csharp
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with test DB
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(opts =>
                    opts.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetOrders_Returns200()
    {
        var response = await _client.GetAsync("/api/orders");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Common Scenarios

---

**Q: How do you implement background processing in .NET 8?**

```csharp
// IHostedService — basic background task
public class OrderProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingOrdersAsync();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

// Registration
builder.Services.AddHostedService<OrderProcessingService>();
```

For queue-based work, combine with `Channel<T>`:

```csharp
// Producer
await _channel.Writer.WriteAsync(new OrderTask(orderId));

// Consumer in BackgroundService
await foreach (var task in _channel.Reader.ReadAllAsync(stoppingToken))
    await ProcessAsync(task);
```

---

**Q: How do you handle global exceptions in ASP.NET Core 8?**

```csharp
// Option 1: Exception handler middleware (simple)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred",
            traceId = Activity.Current?.Id
        });
    });
});

// Option 2: IExceptionHandler (NET 8, cleaner)
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, message) = exception switch
        {
            NotFoundException => (404, exception.Message),
            ValidationException => (400, exception.Message),
            _ => (500, "An unexpected error occurred")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = message }, ct);
        return true;
    }
}

// Register
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

---

**Q: What is the `TimeProvider` abstraction in .NET 8 and why does it matter?**

`TimeProvider` makes time-dependent code testable by injecting a controllable clock:

```csharp
// Instead of DateTime.UtcNow (untestable)
public bool IsExpired(DateTime expiresAt) => DateTime.UtcNow > expiresAt;

// With TimeProvider (testable)
public class TokenValidator
{
    private readonly TimeProvider _time;

    public TokenValidator(TimeProvider time) => _time = time;

    public bool IsExpired(DateTime expiresAt) =>
        _time.GetUtcNow() > expiresAt;
}

// Test
var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow.AddDays(2));
var validator = new TokenValidator(fakeTime);
Assert.True(validator.IsExpired(DateTime.UtcNow.AddDays(1)));
```

---

**Q: How does `HttpClientFactory` work and why should you use it?**

`HttpClientFactory` manages `HttpClient` lifetimes to avoid socket exhaustion and DNS issues:

```csharp
// Registration
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.weather.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// With Polly resilience (retry, circuit breaker)
builder.Services.AddHttpClient<IWeatherService, WeatherService>()
    .AddStandardResilienceHandler(); // .NET 8 built-in

// Usage — inject typed client
public class WeatherService : IWeatherService
{
    private readonly HttpClient _client;
    public WeatherService(HttpClient client) => _client = client;

    public async Task<Weather> GetAsync(string city) =>
        await _client.GetFromJsonAsync<Weather>($"/v1/current?city={city}");
}
```

**Why not `new HttpClient()`?**
- Creates a new socket per instance — exhausts ephemeral ports under load
- Doesn't respect DNS TTL — stale connections after DNS changes
- `HttpClientFactory` pools handlers, rotates them, and handles all of this
