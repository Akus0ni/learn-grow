# OOP & Design Patterns — Interview Q&A

---

## SOLID Principles

**Q: Explain SOLID principles with examples.**

### S — Single Responsibility Principle
> A class should have only one reason to change.

```csharp
// BAD: one class does too much
public class OrderProcessor
{
    public void ProcessOrder(Order order) { ... }
    public void SendEmail(Order order) { ... }   // different responsibility
    public void SaveToDb(Order order) { ... }    // different responsibility
}

// GOOD: separate responsibilities
public class OrderProcessor { public void Process(Order order) { ... } }
public class EmailService { public void Send(Order order) { ... } }
public class OrderRepository { public void Save(Order order) { ... } }
```

### O — Open/Closed Principle
> Open for extension, closed for modification.

```csharp
// Add new behavior via new classes, not changing existing
public abstract class Discount { public abstract decimal Apply(decimal price); }
public class SeasonalDiscount : Discount { ... }
public class LoyaltyDiscount : Discount { ... }
```

### L — Liskov Substitution Principle
> Subclasses must be substitutable for their base class without breaking behavior.

```csharp
// BAD: Square breaking Rectangle's contract
class Rectangle { public virtual int Width { get; set; } public virtual int Height { get; set; } }
class Square : Rectangle { /* width/height must always be equal — violates contract */ }
```

### I — Interface Segregation Principle
> Don't force clients to implement interfaces they don't use.

```csharp
// BAD: fat interface
interface IWorker { void Work(); void Eat(); void Sleep(); }

// GOOD: segregated
interface IWorkable { void Work(); }
interface IFeedable { void Eat(); }
```

### D — Dependency Inversion Principle
> Depend on abstractions, not concrete implementations.

```csharp
// BAD
public class OrderService { private SqlOrderRepo _repo = new SqlOrderRepo(); }

// GOOD
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo; // DI
}
```

---

## Common Design Patterns

### Creational Patterns

**Q: What is the Singleton pattern?**

A: Ensures only one instance of a class exists and provides global access.

```csharp
public class ConfigManager
{
    private static ConfigManager _instance;
    private static readonly object _lock = new();

    private ConfigManager() { }

    public static ConfigManager Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new ConfigManager();
                return _instance;
            }
        }
    }
}
```

> In .NET Core, use `services.AddSingleton<T>()` instead of implementing yourself.

---

**Q: What is the Factory pattern?**

A: Creates objects without exposing instantiation logic.

```csharp
public interface INotification { void Send(string message); }
public class EmailNotification : INotification { public void Send(string msg) { ... } }
public class SmsNotification : INotification { public void Send(string msg) { ... } }

public class NotificationFactory
{
    public static INotification Create(string type) => type switch
    {
        "email" => new EmailNotification(),
        "sms"   => new SmsNotification(),
        _       => throw new ArgumentException("Unknown type")
    };
}
```

---

**Q: What is the Builder pattern?**

A: Constructs complex objects step by step.

```csharp
var order = new OrderBuilder()
    .ForClient("Client123")
    .WithAmount(5000)
    .WithCurrency("USD")
    .Build();
```

Common in query builders, HTTP clients (`HttpClient`), and `StringBuilder`.

---

### Structural Patterns

**Q: What is the Repository pattern?**

A: Abstracts data access logic behind an interface, decoupling the domain from the database.

```csharp
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task AddAsync(Order order);
    Task SaveChangesAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    // implementation using EF Core
}
```

> **Why it matters:** Enables unit testing (mock the repo, not EF Core), and centralizes query logic.

---

**Q: What is the Decorator pattern?**

A: Adds behavior to an object dynamically without modifying its class.

```csharp
// Base
public interface IOrderService { Task<Order> GetOrderAsync(int id); }

// Decorator: adds caching
public class CachedOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ICache _cache;

    public CachedOrderService(IOrderService inner, ICache cache)
    { _inner = inner; _cache = cache; }

    public async Task<Order> GetOrderAsync(int id)
    {
        return await _cache.GetOrSetAsync($"order:{id}",
            () => _inner.GetOrderAsync(id));
    }
}
```

---

**Q: What is the Adapter pattern?**

A: Wraps an incompatible interface to make it compatible.

```csharp
// Third-party logging library with different interface
class ThirdPartyLogger { public void LogMessage(string msg) { ... } }

// Adapter to our ILogger interface
class LoggerAdapter : ILogger
{
    private readonly ThirdPartyLogger _logger;
    public LoggerAdapter(ThirdPartyLogger logger) => _logger = logger;
    public void Log(string msg) => _logger.LogMessage(msg);
}
```

---

### Behavioral Patterns

**Q: What is the Observer pattern?**

A: Defines a one-to-many dependency. When one object changes state, all dependents are notified.

In .NET: `IObservable<T>` / `IObserver<T>`, or C# events/delegates.

```csharp
public class OrderService
{
    public event Action<Order> OrderCreated;

    public void CreateOrder(Order order)
    {
        // create order...
        OrderCreated?.Invoke(order); // notify subscribers
    }
}
```

---

**Q: What is the Strategy pattern?**

A: Defines a family of algorithms, encapsulates them, and makes them interchangeable.

```csharp
public interface IExportStrategy { void Export(IEnumerable<Order> orders); }
public class CsvExport : IExportStrategy { ... }
public class ExcelExport : IExportStrategy { ... }
public class PdfExport : IExportStrategy { ... }

public class ReportGenerator
{
    private IExportStrategy _strategy;
    public ReportGenerator(IExportStrategy strategy) => _strategy = strategy;
    public void Generate(IEnumerable<Order> orders) => _strategy.Export(orders);
}
```

---

**Q: What is the Command pattern?**

A: Encapsulates a request as an object, enabling undo/redo, queuing, and logging.

Used heavily with **MediatR** in .NET (CQRS pattern):

```csharp
public record CreateOrderCommand(string ClientId, decimal Amount) : IRequest<int>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // create order, return id
    }
}
```

---

## UML (Quick Reference)

**Q: What UML diagrams are most common in design?**

| Diagram | Purpose |
|---|---|
| **Class Diagram** | Shows classes, relationships, attributes, methods |
| **Sequence Diagram** | Shows interaction between objects over time |
| **Use Case Diagram** | Shows system functionality from user perspective |
| **Activity Diagram** | Shows workflow / business logic flow |
| **Component Diagram** | Shows high-level system components |

**Key relationships in class diagrams:**
- **Association** — `A uses B` (solid line)
- **Aggregation** — `A has B` (hollow diamond) — B can exist without A
- **Composition** — `A owns B` (filled diamond) — B cannot exist without A
- **Inheritance** — `A is-a B` (solid line + hollow triangle)
- **Realization** — `A implements interface B` (dashed line + hollow triangle)
- **Dependency** — `A depends on B` (dashed line + arrow)

---

## Quick-Fire

- **DRY** — Don't Repeat Yourself. Avoid duplication.
- **KISS** — Keep It Simple, Stupid. Simplest solution that works.
- **YAGNI** — You Aren't Gonna Need It. Don't build for hypothetical futures.
- **Cohesion** — How related are things within a module (high = good).
- **Coupling** — How dependent modules are on each other (low = good).
