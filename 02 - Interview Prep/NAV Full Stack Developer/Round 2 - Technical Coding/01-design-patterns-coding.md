# Design Patterns — Live Coding Guide

> Focus: Implement patterns in C# from scratch. Know the *why*, not just the *what*.

---

## Pattern Complexity Map

| Pattern | Likely Asked? | Difficulty | Your Confidence Target |
|---------|--------------|------------|------------------------|
| Repository | ⭐⭐⭐ HIGH | Medium | Write from scratch in 5 min |
| Factory / Abstract Factory | ⭐⭐⭐ HIGH | Medium | Write from scratch in 5 min |
| Strategy | ⭐⭐⭐ HIGH | Easy | Write from scratch in 3 min |
| Singleton (thread-safe) | ⭐⭐ MED | Easy | Explain + write in 3 min |
| Builder | ⭐⭐ MED | Medium | Write in 7 min |
| Decorator | ⭐⭐ MED | Medium | Explain with example |
| Observer / Events | ⭐⭐ MED | Medium | Explain with C# events |
| Command / MediatR | ⭐ LOW | Hard | Explain conceptually |
| Unit of Work | ⭐⭐ MED | Hard | Explain with Repository |

---

## 1. Repository Pattern

### Q: "Implement a Repository pattern for a Trade entity."

**Why use it?** Abstracts data access from business logic. Swap EF Core for Dapper without changing business code.

```csharp
// Step 1: Define the entity
public class Trade
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public decimal Amount { get; set; }
    public DateTime TradeDate { get; set; }
    public string Status { get; set; }
}

// Step 2: Define the interface (ALWAYS start with the interface!)
public interface ITradeRepository
{
    Task<Trade> GetByIdAsync(int id);
    Task<IEnumerable<Trade>> GetAllAsync();
    Task<IEnumerable<Trade>> GetByStatusAsync(string status);
    Task AddAsync(Trade trade);
    Task UpdateAsync(Trade trade);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

// Step 3: EF Core implementation
public class TradeRepository : ITradeRepository
{
    private readonly AppDbContext _context;

    public TradeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> GetByIdAsync(int id)
        => await _context.Trades.FindAsync(id);

    public async Task<IEnumerable<Trade>> GetAllAsync()
        => await _context.Trades.ToListAsync();

    public async Task<IEnumerable<Trade>> GetByStatusAsync(string status)
        => await _context.Trades
            .Where(t => t.Status == status)
            .ToListAsync();

    public async Task AddAsync(Trade trade)
    {
        await _context.Trades.AddAsync(trade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Trade trade)
    {
        _context.Trades.Update(trade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var trade = await GetByIdAsync(id);
        if (trade != null)
        {
            _context.Trades.Remove(trade);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Trades.AnyAsync(t => t.Id == id);
}

// Step 4: Register in DI (show you know this!)
// builder.Services.AddScoped<ITradeRepository, TradeRepository>();
```

**Follow-up: "What's Unit of Work and how does it relate?"**

```csharp
// Unit of Work wraps multiple repositories under ONE SaveChanges call
public interface IUnitOfWork : IDisposable
{
    ITradeRepository Trades { get; }
    IFundRepository Funds { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITradeRepository Trades { get; }
    public IFundRepository Funds { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Trades = new TradeRepository(context);
        Funds = new FundRepository(context);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
```

> **Say this**: "Repository gives us testability — I can mock `ITradeRepository` in unit tests. Unit of Work ensures atomicity across multiple repositories in a single transaction."

---

## 2. Factory Pattern

### Q: "Implement a Factory to create different report types."

**Why use it?** Centralises object creation. Open/Closed principle — add new types without changing factory consumers.

```csharp
// Step 1: Common interface
public interface IReport
{
    string Generate(DateTime fromDate, DateTime toDate);
    string Format { get; }
}

// Step 2: Concrete implementations
public class PdfReport : IReport
{
    public string Format => "PDF";
    public string Generate(DateTime fromDate, DateTime toDate)
        => $"[PDF] NAV Report from {fromDate:d} to {toDate:d}";
}

public class ExcelReport : IReport
{
    public string Format => "Excel";
    public string Generate(DateTime fromDate, DateTime toDate)
        => $"[Excel] NAV Report from {fromDate:d} to {toDate:d}";
}

public class CsvReport : IReport
{
    public string Format => "CSV";
    public string Generate(DateTime fromDate, DateTime toDate)
        => $"[CSV] NAV Report from {fromDate:d} to {toDate:d}";
}

// Step 3a: Simple Factory (static)
public static class ReportFactory
{
    public static IReport Create(string format) => format.ToUpper() switch
    {
        "PDF"   => new PdfReport(),
        "EXCEL" => new ExcelReport(),
        "CSV"   => new CsvReport(),
        _ => throw new ArgumentException($"Unknown format: {format}")
    };
}

// Usage:
// var report = ReportFactory.Create("PDF");
// Console.WriteLine(report.Generate(DateTime.Today.AddDays(-30), DateTime.Today));
```

**Follow-up: "What's the difference between Factory Method and Abstract Factory?"**

```csharp
// Factory Method — subclasses decide which object to create
public abstract class ReportGenerator
{
    // Factory Method
    protected abstract IReport CreateReport();

    public string RunReport(DateTime from, DateTime to)
    {
        var report = CreateReport(); // subclass decides
        return report.Generate(from, to);
    }
}

public class PdfReportGenerator : ReportGenerator
{
    protected override IReport CreateReport() => new PdfReport();
}

// Abstract Factory — family of related objects
public interface IReportFactory
{
    IReport CreateReport();
    IReportHeader CreateHeader();
    IReportFooter CreateFooter();
}

// Abstract Factory is when you need to create FAMILIES of objects
// e.g., a "NAV Report Suite" that creates Header + Body + Footer consistently
```

> **Say this**: "Factory Method uses inheritance — subclasses override to decide. Abstract Factory uses composition — you inject the factory and it creates a suite of related objects. I'd use Factory Method for single-object creation, Abstract Factory when the objects need to be consistent with each other."

---

## 3. Strategy Pattern

### Q: "Implement Strategy pattern for different fee calculation methods."

**Why use it?** Swap algorithms at runtime without if/else chains. Perfect for business rules that change.

```csharp
// Step 1: Strategy interface
public interface IFeeCalculationStrategy
{
    decimal Calculate(decimal navValue, decimal unitsHeld);
    string StrategyName { get; }
}

// Step 2: Concrete strategies
public class ManagementFeeStrategy : IFeeCalculationStrategy
{
    public string StrategyName => "Management Fee";
    private readonly decimal _annualRate;

    public ManagementFeeStrategy(decimal annualRate = 0.02m) // 2% default
    {
        _annualRate = annualRate;
    }

    public decimal Calculate(decimal navValue, decimal unitsHeld)
        => navValue * unitsHeld * (_annualRate / 365); // daily accrual
}

public class PerformanceFeeStrategy : IFeeCalculationStrategy
{
    public string StrategyName => "Performance Fee";
    private readonly decimal _highWaterMark;
    private readonly decimal _rate;

    public PerformanceFeeStrategy(decimal highWaterMark, decimal rate = 0.20m)
    {
        _highWaterMark = highWaterMark;
        _rate = rate;
    }

    public decimal Calculate(decimal navValue, decimal unitsHeld)
    {
        if (navValue <= _highWaterMark) return 0;
        return (navValue - _highWaterMark) * unitsHeld * _rate;
    }
}

public class FlatFeeStrategy : IFeeCalculationStrategy
{
    public string StrategyName => "Flat Fee";
    private readonly decimal _flatAmount;

    public FlatFeeStrategy(decimal flatAmount) => _flatAmount = flatAmount;

    public decimal Calculate(decimal navValue, decimal unitsHeld)
        => _flatAmount;
}

// Step 3: Context (uses the strategy)
public class FeeCalculator
{
    private IFeeCalculationStrategy _strategy;

    public FeeCalculator(IFeeCalculationStrategy strategy)
    {
        _strategy = strategy;
    }

    // Can swap at runtime!
    public void SetStrategy(IFeeCalculationStrategy strategy)
        => _strategy = strategy;

    public decimal CalculateFee(decimal navValue, decimal unitsHeld)
    {
        var fee = _strategy.Calculate(navValue, unitsHeld);
        Console.WriteLine($"[{_strategy.StrategyName}] Fee: {fee:C}");
        return fee;
    }
}

// Usage:
// var calculator = new FeeCalculator(new ManagementFeeStrategy());
// calculator.CalculateFee(100m, 1000);
//
// calculator.SetStrategy(new PerformanceFeeStrategy(95m));
// calculator.CalculateFee(100m, 1000);
```

> **Say this**: "Strategy eliminates big if/else or switch chains and makes adding new fee types easy — open for extension, closed for modification. I used this pattern at Energy Exemplar for different calculation modes."

---

## 4. Builder Pattern

### Q: "Implement a Builder for a complex FundReport object."

**Why use it?** Step-by-step construction of complex objects. Avoids telescoping constructors.

```csharp
// The complex product
public class FundReport
{
    public string FundName { get; set; }
    public DateTime ReportDate { get; set; }
    public string Currency { get; set; }
    public decimal TotalNAV { get; set; }
    public int TotalInvestors { get; set; }
    public List<string> IncludedSections { get; set; } = new();
    public string ReportFormat { get; set; }
    public bool IncludeCharts { get; set; }

    public override string ToString()
        => $"Fund: {FundName} | NAV: {TotalNAV:C} | Date: {ReportDate:d} | Sections: {string.Join(", ", IncludedSections)}";
}

// Builder interface
public interface IFundReportBuilder
{
    IFundReportBuilder ForFund(string fundName);
    IFundReportBuilder AsOf(DateTime date);
    IFundReportBuilder InCurrency(string currency);
    IFundReportBuilder WithNAV(decimal totalNAV);
    IFundReportBuilder WithInvestorCount(int count);
    IFundReportBuilder IncludeSection(string section);
    IFundReportBuilder InFormat(string format);
    IFundReportBuilder WithCharts(bool include = true);
    FundReport Build();
}

// Concrete Builder
public class FundReportBuilder : IFundReportBuilder
{
    private readonly FundReport _report = new FundReport();

    public IFundReportBuilder ForFund(string fundName)
    {
        _report.FundName = fundName;
        return this;
    }

    public IFundReportBuilder AsOf(DateTime date)
    {
        _report.ReportDate = date;
        return this;
    }

    public IFundReportBuilder InCurrency(string currency)
    {
        _report.Currency = currency;
        return this;
    }

    public IFundReportBuilder WithNAV(decimal totalNAV)
    {
        _report.TotalNAV = totalNAV;
        return this;
    }

    public IFundReportBuilder WithInvestorCount(int count)
    {
        _report.TotalInvestors = count;
        return this;
    }

    public IFundReportBuilder IncludeSection(string section)
    {
        _report.IncludedSections.Add(section);
        return this;
    }

    public IFundReportBuilder InFormat(string format)
    {
        _report.ReportFormat = format;
        return this;
    }

    public IFundReportBuilder WithCharts(bool include = true)
    {
        _report.IncludeCharts = include;
        return this;
    }

    public FundReport Build()
    {
        // Validation here!
        if (string.IsNullOrWhiteSpace(_report.FundName))
            throw new InvalidOperationException("Fund name is required");
        if (_report.ReportDate == default)
            _report.ReportDate = DateTime.Today;

        return _report;
    }
}

// Usage — fluent API!
var report = new FundReportBuilder()
    .ForFund("NAV Global Equity Fund")
    .AsOf(DateTime.Today)
    .InCurrency("USD")
    .WithNAV(1_250_000m)
    .WithInvestorCount(342)
    .IncludeSection("Performance Summary")
    .IncludeSection("Holdings")
    .IncludeSection("Risk Metrics")
    .InFormat("PDF")
    .WithCharts()
    .Build();

Console.WriteLine(report);
```

> **Say this**: "Builder is great when you have more than 3-4 constructor parameters, or when some are optional. The fluent interface makes the call site read like English. The Build() method is also the right place to put validation."

---

## 5. Singleton Pattern (Thread-Safe)

### Q: "Write a thread-safe Singleton for a configuration manager."

```csharp
// Option 1: Lazy<T> — preferred in modern C# (thread-safe by default)
public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance =
        new Lazy<ConfigurationManager>(() => new ConfigurationManager());

    private readonly Dictionary<string, string> _settings;

    private ConfigurationManager()
    {
        // Private constructor prevents direct instantiation
        _settings = new Dictionary<string, string>
        {
            ["ConnectionString"] = "Server=...;Database=NAV;",
            ["MaxRetries"] = "3",
            ["Timeout"] = "30"
        };
    }

    public static ConfigurationManager Instance => _instance.Value;

    public string GetSetting(string key)
        => _settings.TryGetValue(key, out var value) ? value : string.Empty;
}

// Usage:
// var connStr = ConfigurationManager.Instance.GetSetting("ConnectionString");

// Option 2: Double-check locking (know this pattern too)
public sealed class AppCache
{
    private static AppCache _instance;
    private static readonly object _lock = new object();

    private AppCache() { }

    public static AppCache Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null) // double-check!
                        _instance = new AppCache();
                }
            }
            return _instance;
        }
    }
}
```

> **Say this**: "In modern .NET I prefer `Lazy<T>` — it handles thread safety for free and is cleaner. The double-check lock was the pre-.NET 4 approach. Also worth noting: in ASP.NET Core, true Singletons are usually handled by the DI container via `AddSingleton<T>()`, which is preferred over manual Singleton implementations because it plays nicely with the container lifecycle."

---

## 6. Decorator Pattern

### Q: "Add logging and caching to a TradeService without modifying it."

```csharp
public interface ITradeService
{
    Task<Trade> GetTradeAsync(int id);
}

// Base implementation
public class TradeService : ITradeService
{
    private readonly ITradeRepository _repo;
    public TradeService(ITradeRepository repo) => _repo = repo;

    public async Task<Trade> GetTradeAsync(int id)
        => await _repo.GetByIdAsync(id);
}

// Decorator 1: Logging
public class LoggingTradeService : ITradeService
{
    private readonly ITradeService _inner;
    private readonly ILogger<LoggingTradeService> _logger;

    public LoggingTradeService(ITradeService inner, ILogger<LoggingTradeService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Trade> GetTradeAsync(int id)
    {
        _logger.LogInformation("Getting trade {TradeId}", id);
        var stopwatch = Stopwatch.StartNew();
        var result = await _inner.GetTradeAsync(id);
        stopwatch.Stop();
        _logger.LogInformation("Got trade {TradeId} in {Elapsed}ms", id, stopwatch.ElapsedMilliseconds);
        return result;
    }
}

// Decorator 2: Caching
public class CachingTradeService : ITradeService
{
    private readonly ITradeService _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public CachingTradeService(ITradeService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Trade> GetTradeAsync(int id)
    {
        var key = $"trade_{id}";
        if (_cache.TryGetValue(key, out Trade cached))
            return cached;

        var trade = await _inner.GetTradeAsync(id);
        _cache.Set(key, trade, _cacheDuration);
        return trade;
    }
}

// Stacking decorators (DI registration):
// services.AddScoped<ITradeRepository, TradeRepository>();
// services.AddScoped<TradeService>();
// services.AddScoped<ITradeService>(sp =>
//     new CachingTradeService(
//         new LoggingTradeService(
//             sp.GetRequiredService<TradeService>(),
//             sp.GetRequiredService<ILogger<LoggingTradeService>>()),
//         sp.GetRequiredService<IMemoryCache>()));
```

> **Say this**: "Decorator follows Open/Closed — I add behaviour without touching the original. The key is that every decorator implements the same interface as the thing it wraps, so the caller never knows. This is also how ASP.NET Core middleware works conceptually."

---

## 7. Observer Pattern

### Q: "Implement an event system where trade completion notifies multiple handlers."

```csharp
// C# events ARE the Observer pattern built in
public class TradeEventArgs : EventArgs
{
    public Trade Trade { get; }
    public TradeEventArgs(Trade trade) => Trade = trade;
}

public class TradeProcessor
{
    // Publisher
    public event EventHandler<TradeEventArgs> TradeCompleted;
    public event EventHandler<TradeEventArgs> TradeFailed;

    protected virtual void OnTradeCompleted(Trade trade)
        => TradeCompleted?.Invoke(this, new TradeEventArgs(trade));

    public async Task ProcessAsync(Trade trade)
    {
        try
        {
            // ... processing logic
            trade.Status = "Completed";
            OnTradeCompleted(trade);
        }
        catch (Exception ex)
        {
            TradeFailed?.Invoke(this, new TradeEventArgs(trade));
        }
    }
}

// Subscribers
public class EmailNotificationHandler
{
    public void OnTradeCompleted(object sender, TradeEventArgs e)
        => Console.WriteLine($"Email: Trade {e.Trade.Id} completed for {e.Trade.Symbol}");
}

public class AuditLogHandler
{
    public void OnTradeCompleted(object sender, TradeEventArgs e)
        => Console.WriteLine($"Audit: Logging trade {e.Trade.Id}");
}

// Wiring it up:
var processor = new TradeProcessor();
var emailHandler = new EmailNotificationHandler();
var auditHandler = new AuditLogHandler();

processor.TradeCompleted += emailHandler.OnTradeCompleted;
processor.TradeCompleted += auditHandler.OnTradeCompleted;

// await processor.ProcessAsync(trade); // both handlers fire
```

> **Say this**: "C# events are the Observer pattern — the publisher doesn't know who's listening. For more complex scenarios in ASP.NET Core I'd use MediatR notifications which are Observer through the DI container, giving us loose coupling across assemblies."

---

## 8. SOLID — Live Coding Questions

### Q: "Show me a violation of SRP and fix it."

```csharp
// BAD: OrderService does too many things
public class OrderService_BAD
{
    public void ProcessOrder(Order order)
    {
        // Validation
        if (order.Amount <= 0) throw new ArgumentException("Invalid amount");

        // Save to DB
        using var conn = new SqlConnection("...");
        conn.Execute("INSERT INTO Orders...", order);

        // Send email
        var smtp = new SmtpClient();
        smtp.Send(new MailMessage("from@nav.com", order.CustomerEmail));

        // Log
        File.AppendAllText("log.txt", $"Order {order.Id} processed");
    }
}

// GOOD: Each class has ONE reason to change
public class OrderValidator
{
    public void Validate(Order order)
    {
        if (order.Amount <= 0)
            throw new ArgumentException("Invalid amount");
    }
}

public class OrderRepository
{
    public void Save(Order order) { /* DB logic */ }
}

public class OrderNotificationService
{
    public void NotifyCustomer(Order order) { /* Email logic */ }
}

public class OrderService_GOOD
{
    private readonly OrderValidator _validator;
    private readonly OrderRepository _repo;
    private readonly OrderNotificationService _notifications;
    private readonly ILogger<OrderService_GOOD> _logger;

    public OrderService_GOOD(
        OrderValidator validator,
        OrderRepository repo,
        OrderNotificationService notifications,
        ILogger<OrderService_GOOD> logger)
    {
        _validator = validator;
        _repo = repo;
        _notifications = notifications;
        _logger = logger;
    }

    public void ProcessOrder(Order order)
    {
        _validator.Validate(order);
        _repo.Save(order);
        _notifications.NotifyCustomer(order);
        _logger.LogInformation("Order {OrderId} processed", order.Id);
    }
}
```

### Q: "Show me Dependency Inversion in practice."

```csharp
// BAD: High-level depends on low-level (concrete)
public class OrderService_BAD2
{
    private readonly SqlOrderRepository _repo; // concrete dependency!
    public OrderService_BAD2() => _repo = new SqlOrderRepository();
}

// GOOD: Both depend on abstraction
public interface IOrderRepository
{
    void Save(Order order);
}

public class SqlOrderRepository : IOrderRepository
{
    public void Save(Order order) { /* SQL Server */ }
}

public class InMemoryOrderRepository : IOrderRepository // for testing!
{
    private readonly List<Order> _orders = new();
    public void Save(Order order) => _orders.Add(order);
}

public class OrderService_GOOD2
{
    private readonly IOrderRepository _repo;
    // Injected by DI container — doesn't know or care which implementation
    public OrderService_GOOD2(IOrderRepository repo) => _repo = repo;
}
```

---

## Common Interview Bug-Finding Exercises

### Bug 1: Singleton in a multi-threaded scenario
```csharp
// BUGGY — not thread safe
public class Logger
{
    private static Logger _instance;
    public static Logger Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Logger(); // race condition!
            return _instance;
        }
    }
}
// FIX: Use Lazy<T> as shown above
```

### Bug 2: Violating Liskov Substitution
```csharp
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : Rectangle
{
    public override int Width { set { base.Width = value; base.Height = value; } } // BUG!
    public override int Height { set { base.Width = value; base.Height = value; } }
}

// Rectangle r = new Square();
// r.Width = 4; r.Height = 5;
// r.Area() == 25, NOT 20 — LSP violation!
// FIX: Don't inherit Square from Rectangle. Use a common IShape interface.
```

### Bug 3: Abstract Factory wrong usage
```csharp
// If asked "what's wrong with this Factory?"
public class BadFactory
{
    public object Create(string type)
    {
        if (type == "A") return new TypeA();
        if (type == "B") return new TypeB();
        return null; // BUG: returns null for unknown types
    }
}
// FIX: throw ArgumentException, use switch expression, or return a NullObject pattern
```

---

## Pattern Quick-Reference Card

| Pattern | One Line | C# keyword to mention |
|---------|----------|-----------------------|
| Repository | Abstracts data layer | `interface`, `async Task` |
| Factory | Centralises creation | `switch expression`, interface |
| Strategy | Swap algorithms at runtime | `interface`, DI injection |
| Builder | Step-by-step construction | fluent, method chaining |
| Decorator | Add behaviour without changing | wraps same interface |
| Singleton | One instance, globally | `Lazy<T>`, `sealed` |
| Observer | Notify many without coupling | `event`, `EventHandler<T>` |
| Unit of Work | Atomic multi-repo operations | single `SaveChanges()` |
