# .NET Core, .NET Framework & C# — Interview Prep

> Aligned with your eGain & Energy Exemplar experience. Enterprise-depth coverage.

---

## 1. .NET Evolution — Know This Timeline

```
.NET Framework 1.0 (2002)  → Windows-only, monolithic
.NET Framework 4.8 (2019)  → Last Framework release, still maintained
.NET Core 1.0 (2016)       → Cross-platform, open-source rewrite
.NET Core 3.1 (2019)       → LTS, desktop app support added
.NET 5 (2020)              → Unified "just .NET" (Framework + Core merged branding)
.NET 6 (2021)              → LTS, minimal APIs, hot reload
.NET 7 (2022)              → Performance improvements, rate limiting
.NET 8 (2023)              → LTS, current recommended, AOT compilation
```

**Key distinction:**
- **.NET Framework** — Windows-only, GAC, COM interop, legacy WinForms/WebForms
- **.NET Core/.NET 5+** — Cross-platform, container-friendly, high performance, no GAC

**Interview answer:** *"When I joined eGain, the analytics product was on .NET Framework. I've worked through the modernization path — understanding both the legacy constraints (Windows deployment, IIS dependencies) and the benefits of moving to .NET 6+: cross-platform, containerization, and significantly improved performance."*

---

## 2. ASP.NET Core Deep Dive

### Request Pipeline & Middleware

The pipeline is ordered. Order matters — authentication always before authorization.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services (DI container)
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Middleware pipeline — ORDER IS CRITICAL
app.UseExceptionHandler("/error");      // catch unhandled exceptions
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();                // who are you?
app.UseAuthorization();                 // what can you do?
app.MapControllers();

app.Run();
```

### Controller-Based API

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService service, ILogger<AnalyticsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AnalyticsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnalyticsRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAnalyticsRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

### Minimal APIs (.NET 6+)

```csharp
// Alternative to controllers — leaner for simple endpoints
app.MapGet("/api/reports/{id}", async (int id, IReportService service) =>
{
    var report = await service.GetByIdAsync(id);
    return report is null ? Results.NotFound() : Results.Ok(report);
})
.RequireAuthorization()
.WithName("GetReport");

app.MapPost("/api/reports", async (CreateReportRequest req, IReportService service) =>
{
    var created = await service.CreateAsync(req);
    return Results.CreatedAtRoute("GetReport", new { id = created.Id }, created);
});
```

---

## 3. Dependency Injection (DI)

ASP.NET Core has DI built in. This is a core enterprise pattern.

### Service Lifetimes

| Lifetime | Registered As | Created | Destroyed | Use For |
|---|---|---|---|---|
| **Singleton** | `AddSingleton` | Once on startup | App shutdown | Config, caches, logging |
| **Scoped** | `AddScoped` | Once per HTTP request | Request end | DB contexts, business services |
| **Transient** | `AddTransient` | Every injection | After use | Lightweight, stateless services |

```csharp
builder.Services.AddSingleton<IConfiguration>(config);
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Register with factory
builder.Services.AddScoped<IDbContext>(provider =>
    new AppDbContext(provider.GetRequiredService<IConfiguration>()));
```

**Captive dependency trap:** Never inject a Scoped service into a Singleton — the Singleton outlives the Scoped service's intended lifetime. ASP.NET Core detects this at startup.

### Interface-Based DI (Your Pattern at eGain)
```csharp
public interface IAnalyticsService
{
    Task<AnalyticsDto> GetByIdAsync(int id);
    Task<AnalyticsDto> CreateAsync(CreateAnalyticsRequest request);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IDbContext _db;

    public AnalyticsService(IDbContext db) => _db = db;

    public async Task<AnalyticsDto> GetByIdAsync(int id)
    {
        var entity = await _db.Analytics.FindAsync(id);
        return entity?.ToDto();
    }
}
```

---

## 4. C# Essential Features for Enterprise Dev

### async / await (Critical)

```csharp
// CORRECT: async all the way down
public async Task<Report> GetReportAsync(int id)
{
    // await releases the thread while waiting — no blocking
    var data = await _db.Reports.FindAsync(id);
    var enriched = await _enrichmentService.EnrichAsync(data);
    return enriched;
}

// WRONG: .Result or .Wait() causes deadlocks in ASP.NET
public Report GetReport(int id)
{
    return _db.Reports.FindAsync(id).Result; // DEADLOCK risk
}

// Parallel async operations
public async Task<(Report report, User user)> GetDataAsync(int reportId, int userId)
{
    var reportTask = _reportService.GetAsync(reportId);
    var userTask = _userService.GetAsync(userId);
    await Task.WhenAll(reportTask, userTask);
    return (reportTask.Result, userTask.Result);
}

// CancellationToken — always support it in APIs
public async Task<Report> GetReportAsync(int id, CancellationToken ct)
{
    return await _db.Reports
        .Where(r => r.Id == id)
        .FirstOrDefaultAsync(ct);
}
```

### LINQ (Heavily Used at eGain/Analytics Work)

```csharp
var reports = await _db.Reports
    .Where(r => r.Status == ReportStatus.Active && r.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(r => r.CreatedAt)
    .Select(r => new ReportSummary
    {
        Id = r.Id,
        Title = r.Title,
        Owner = r.Owner.FullName  // navigation property
    })
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// GroupBy for analytics aggregations
var stats = await _db.Events
    .GroupBy(e => e.Type)
    .Select(g => new { Type = g.Key, Count = g.Count(), Latest = g.Max(e => e.CreatedAt) })
    .ToListAsync();

// Any / All
bool hasActiveReports = await _db.Reports.AnyAsync(r => r.Status == ReportStatus.Active);
```

### Generics

```csharp
// Generic repository pattern (common enterprise pattern)
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
}
```

### Records & Pattern Matching (Modern C#)

```csharp
// Records — immutable value types, great for DTOs
public record ReportDto(int Id, string Title, string Status, DateTime CreatedAt);

public record CreateReportRequest(string Title, string Description)
{
    // Can add validation attributes
}

// Pattern matching
public string GetReportLabel(Report report) => report.Status switch
{
    ReportStatus.Draft    => "In Progress",
    ReportStatus.Active   => "Live",
    ReportStatus.Archived => "Archived",
    _                     => "Unknown"
};

// is pattern with type check
if (result is ErrorResult { Code: 404 } notFound)
{
    _logger.LogWarning("Resource not found: {Message}", notFound.Message);
}
```

### Nullable Reference Types (.NET 6+ default)

```csharp
// Enabled by default in .NET 6+ — compiler warns on potential null dereferences
string? nullableName = null;           // explicitly nullable
string nonNullableName = "Akash";      // compiler guarantees non-null

public User? FindUser(string email)    // ? = may return null
{
    return _users.FirstOrDefault(u => u.Email == email);
}

// Null-coalescing and conditional member access
var display = user?.FullName ?? "Anonymous";
var city = user?.Address?.City ?? "Unknown";

// Null-coalescing assignment
user.Tags ??= new List<string>();
```

---

## 5. Entity Framework Core

Your DB work at eGain (SQL Server, Redshift) — EF Core is the standard ORM.

### DbContext Setup

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Report> Reports { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AnalyticsEvent> Events { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.Reports)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.Status);    // index for common queries
        });
    }
}
```

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Migrations

```bash
dotnet ef migrations add AddReportStatusIndex
dotnet ef database update
dotnet ef migrations list
```

### N+1 Problem (Common Interview Trap)

```csharp
// BAD: N+1 — queries DB once per report for owner
var reports = await _db.Reports.ToListAsync();
foreach (var r in reports)
{
    Console.WriteLine(r.Owner.Name);  // separate query each time!
}

// GOOD: Include — single JOIN query
var reports = await _db.Reports
    .Include(r => r.Owner)
    .Include(r => r.Tags)
    .ToListAsync();

// GOOD: Projection avoids loading full entity
var summaries = await _db.Reports
    .Select(r => new { r.Id, r.Title, OwnerName = r.Owner.Name })
    .ToListAsync();
```

---

## 6. Authentication & Authorization (JWT)

You implemented JWT at IKS Health — here's the enterprise pattern.

### JWT Setup (ASP.NET Core)

```csharp
// Program.cs
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ReportsAccess", policy => policy.RequireClaim("permission", "reports.read"));
});
```

### Token Generation

```csharp
public string GenerateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("permission", "reports.read")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

## 7. Design Patterns for Enterprise .NET

### Repository + Unit of Work

```csharp
// Unit of Work wraps multiple repositories in one transaction
public interface IUnitOfWork : IDisposable
{
    IRepository<Report> Reports { get; }
    IRepository<User> Users { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IRepository<Report> Reports { get; }
    public IRepository<User> Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Reports = new Repository<Report>(context);
        Users = new Repository<User>(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}
```

### CQRS with MediatR (Enterprise Standard)

Separate read and write operations — scales better, cleaner code.

```csharp
// Command (write)
public record CreateReportCommand(string Title, int OwnerId) : IRequest<int>;

public class CreateReportHandler : IRequestHandler<CreateReportCommand, int>
{
    private readonly IUnitOfWork _uow;
    public CreateReportHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(CreateReportCommand cmd, CancellationToken ct)
    {
        var report = new Report { Title = cmd.Title, OwnerId = cmd.OwnerId };
        await _uow.Reports.AddAsync(report);
        await _uow.SaveChangesAsync();
        return report.Id;
    }
}

// Query (read)
public record GetReportQuery(int Id) : IRequest<ReportDto>;

// Controller using MediatR
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateReportCommand cmd)
    => Ok(await _mediator.Send(cmd));
```

### Options Pattern (Configuration)

```csharp
// appsettings.json
// { "Redshift": { "ConnectionString": "...", "Schema": "analytics" } }

public class RedshiftOptions
{
    public string ConnectionString { get; set; }
    public string Schema { get; set; }
}

// Register
builder.Services.Configure<RedshiftOptions>(
    builder.Configuration.GetSection("Redshift"));

// Use via DI
public class RedshiftService
{
    private readonly RedshiftOptions _options;
    public RedshiftService(IOptions<RedshiftOptions> options)
        => _options = options.Value;
}
```

---

## 8. VB.NET Cheat Sheet (For Legacy Codebase Navigation)

```vb
' Class
Public Class ReportService
    Implements IReportService

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    ' Function with return value
    Public Async Function GetByIdAsync(id As Integer) As Task(Of ReportDto) _
        Implements IReportService.GetByIdAsync
        Dim report = Await _db.Reports.FindAsync(id)
        If report Is Nothing Then Return Nothing
        Return New ReportDto(report.Id, report.Title)
    End Function

    ' Sub = void method
    Public Async Function DeleteAsync(id As Integer) As Task
        Dim report = Await _db.Reports.FindAsync(id)
        If report IsNot Nothing Then
            _db.Reports.Remove(report)
            Await _db.SaveChangesAsync()
        End If
    End Function
End Class

' LINQ in VB.NET
Dim activeReports = From r In _db.Reports
                    Where r.Status = ReportStatus.Active
                    Order By r.CreatedAt Descending
                    Select r

' String interpolation
Dim message = $"Report {report.Id} created by {report.Owner.Name}"

' Null check
Dim name = If(user?.Name, "Anonymous")
```

---

## 9. Performance Patterns

### Caching

```csharp
// In-memory cache
builder.Services.AddMemoryCache();

public class ReportService
{
    private readonly IMemoryCache _cache;

    public async Task<ReportDto> GetByIdAsync(int id)
    {
        return await _cache.GetOrCreateAsync($"report:{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _db.Reports.FindAsync(id);
        });
    }
}

// Distributed cache (Redis) — for multi-instance deployments
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = "localhost:6379");
```

### Response Compression & Output Caching

```csharp
builder.Services.AddResponseCompression();
builder.Services.AddOutputCache();

app.UseResponseCompression();
app.UseOutputCache();

// On endpoint
app.MapGet("/api/reports", GetReports).CacheOutput(p => p.Expire(TimeSpan.FromMinutes(1)));
```

---

## 10. Common Interview Questions

**Q: What's the difference between Task.WhenAll and Task.WhenAny?**
> `WhenAll` waits for ALL tasks to complete (parallel). `WhenAny` returns when the FIRST task completes. Use `WhenAll` for parallel data fetching, `WhenAny` for timeouts or first-response patterns.

**Q: What causes a deadlock in async .NET code?**
> Calling `.Result` or `.Wait()` on an async method in a context that has a synchronization context (classic ASP.NET, WinForms) — the continuation tries to resume on the same thread that's blocked waiting. Solution: `async` all the way down, or `ConfigureAwait(false)` in library code.

**Q: What is the difference between `IEnumerable` and `IQueryable`?**
> `IEnumerable` executes in memory (LINQ to Objects). `IQueryable` defers execution and translates LINQ to SQL (EF Core queries). Always use `IQueryable` for DB queries and project with `Select` before calling `ToListAsync`.

**Q: What is middleware and how does the order matter?**
> Middleware components form a pipeline — each component processes the request and optionally passes to the next. Order matters: exception handling must be first (outermost) to catch errors from inner middleware. Authentication before authorization, routing before endpoint execution.

**Q: How do you handle cross-cutting concerns?**
> Middleware (for HTTP-level concerns), Action Filters (for controller-level), MediatR pipeline behaviors (for CQRS), or Aspect-Oriented approaches. At eGain, I used middleware for request logging and exception handling across all API endpoints.

**Q: Scoped vs Transient — when do you use each?**
> Scoped for services that should share state within a request (e.g., DbContext — same transaction). Transient for lightweight, stateless services where a fresh instance each time is safe. Never Singleton for DbContext — it holds a connection and would cause concurrency issues.

**Q: What is the Repository pattern and why use it?**
> Abstracts data access behind an interface, decoupling business logic from the ORM. Benefits: testable (mock the interface), swappable data source, centralized query logic. At eGain, we used a layered architecture where services depended on repository interfaces, not directly on EF DbContext.
