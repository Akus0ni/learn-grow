# SOLID Principles & Design Patterns — Interview Q&A

> Sr. Software Engineer (6+ YoE) | C# / .NET Focus

---

## SOLID Principles

---

### Q1: What is the Single Responsibility Principle (SRP) and why does it matter?

**Answer:** SRP states that a class should have only one reason to change — meaning it should encapsulate exactly one responsibility. This makes classes easier to test, maintain, and reason about. When a class handles multiple concerns, a change in one concern can break another.

**Violation:**

```csharp
public class Employee
{
    public string Name { get; set; }
    public decimal Salary { get; set; }

    public decimal CalculatePay() => Salary / 12;

    // Violates SRP — persistence is a separate concern
    public void SaveToDatabase()
    {
        using var conn = new SqlConnection("...");
        // SQL insert logic
    }

    // Violates SRP — reporting is yet another concern
    public string GenerateReport() => $"Employee: {Name}, Pay: {CalculatePay()}";
}
```

**Refactored:**

```csharp
public class Employee
{
    public string Name { get; set; }
    public decimal Salary { get; set; }
}

public class PayCalculator
{
    public decimal CalculateMonthlyPay(Employee emp) => emp.Salary / 12;
}

public class EmployeeRepository
{
    public void Save(Employee emp) { /* persistence logic */ }
}

public class EmployeeReportGenerator
{
    private readonly PayCalculator _calc;
    public EmployeeReportGenerator(PayCalculator calc) => _calc = calc;
    public string Generate(Employee emp) => $"Employee: {emp.Name}, Pay: {_calc.CalculateMonthlyPay(emp)}";
}
```

Each class now changes for exactly one reason: the domain model, pay rules, persistence, or report formatting.

---

### Q2: Explain the Open/Closed Principle (OCP) with a practical C# example.

**Answer:** OCP states that software entities should be **open for extension but closed for modification**. You should be able to add new behavior without changing existing, tested code. In C# this is typically achieved through abstraction (interfaces, abstract classes) and polymorphism.

**Violation — modifying existing code for each new shape:**

```csharp
public class AreaCalculator
{
    public double Calculate(object shape)
    {
        if (shape is Circle c)
            return Math.PI * c.Radius * c.Radius;
        if (shape is Rectangle r)
            return r.Width * r.Height;
        // Every new shape forces a modification here
        throw new NotSupportedException();
    }
}
```

**Refactored — open for extension, closed for modification:**

```csharp
public interface IShape
{
    double Area();
}

public class Circle : IShape
{
    public double Radius { get; init; }
    public double Area() => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; init; }
    public double Height { get; init; }
    public double Area() => Width * Height;
}

// Adding a Triangle requires zero changes to existing code
public class Triangle : IShape
{
    public double Base { get; init; }
    public double Height { get; init; }
    public double Area() => 0.5 * Base * Height;
}

public class AreaCalculator
{
    public double TotalArea(IEnumerable<IShape> shapes) =>
        shapes.Sum(s => s.Area());
}
```

New shapes are added by creating new classes — `AreaCalculator` never changes.

---

### Q3: What is the Liskov Substitution Principle (LSP)? How can you violate it in C#?

**Answer:** LSP states that objects of a derived class must be substitutable for objects of the base class without altering the correctness of the program. If calling code works with a base type, swapping in any subtype should not cause unexpected behavior, exceptions, or violated invariants.

**Classic violation — Square inheriting Rectangle:**

```csharp
public class Rectangle
{
    public virtual double Width { get; set; }
    public virtual double Height { get; set; }
    public double Area() => Width * Height;
}

public class Square : Rectangle
{
    public override double Width
    {
        get => base.Width;
        set { base.Width = value; base.Height = value; } // couples both dimensions
    }
    public override double Height
    {
        get => base.Height;
        set { base.Height = value; base.Width = value; }
    }
}

// Client code that works with Rectangle breaks with Square
void ResizeAndAssert(Rectangle rect)
{
    rect.Width = 5;
    rect.Height = 10;
    Debug.Assert(rect.Area() == 50); // FAILS for Square (area is 100)
}
```

**Fix:** Do not model Square as a subtype of Rectangle. Instead, use a common `IShape` interface or make them siblings.

```csharp
public interface IShape
{
    double Area();
}

public class Rectangle : IShape { /* ... */ }
public class Square : IShape
{
    public double Side { get; set; }
    public double Area() => Side * Side;
}
```

LSP violations are often signaled by overrides that throw `NotSupportedException`, ignore parameters, or tighten preconditions.

---

### Q4: Explain the Interface Segregation Principle (ISP) with a C# example.

**Answer:** ISP states that no client should be forced to depend on methods it does not use. Fat interfaces should be split into smaller, role-specific ones so that implementing classes only bear the contracts they actually fulfill.

**Violation — fat interface:**

```csharp
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

// A robot can work but cannot eat or sleep
public class RobotWorker : IWorker
{
    public void Work() { /* ... */ }
    public void Eat() => throw new NotSupportedException();   // forced to implement
    public void Sleep() => throw new NotSupportedException();  // forced to implement
}
```

**Refactored — segregated interfaces:**

```csharp
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class HumanWorker : IWorkable, IFeedable, ISleepable
{
    public void Work()  { /* ... */ }
    public void Eat()   { /* ... */ }
    public void Sleep() { /* ... */ }
}

public class RobotWorker : IWorkable
{
    public void Work() { /* ... */ }
    // No unwanted baggage
}
```

A real-world .NET example: `IEnumerable<T>` vs `ICollection<T>` vs `IList<T>` — you pick the narrowest interface your code actually needs.

---

### Q5: What is the Dependency Inversion Principle (DIP) and how does it relate to Dependency Injection?

**Answer:** DIP states:

1. High-level modules should not depend on low-level modules. Both should depend on **abstractions**.
2. Abstractions should not depend on details. Details should depend on abstractions.

DIP is the **principle**; Dependency Injection (DI) is a **technique** used to implement it.

**Violation:**

```csharp
public class OrderService
{
    private readonly SqlOrderRepository _repo = new SqlOrderRepository(); // hard dependency on a concrete class

    public void PlaceOrder(Order order)
    {
        // business logic
        _repo.Save(order);
    }
}
```

`OrderService` (high-level) directly depends on `SqlOrderRepository` (low-level detail). Swapping databases means modifying `OrderService`.

**Refactored with DIP + DI:**

```csharp
// Abstraction owned by the high-level layer
public interface IOrderRepository
{
    void Save(Order order);
}

// Low-level detail depends on the abstraction
public class SqlOrderRepository : IOrderRepository
{
    public void Save(Order order) { /* EF Core / ADO.NET */ }
}

// High-level module depends only on the abstraction
public class OrderService
{
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo) // injected via constructor
    {
        _repo = repo;
    }

    public void PlaceOrder(Order order)
    {
        // business logic
        _repo.Save(order);
    }
}

// Registration in DI container (Program.cs / Startup.cs)
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<OrderService>();
```

Now `OrderService` is testable (mock `IOrderRepository`), and switching to CosmosDB only requires a new implementation — no changes to business logic.

---

## Design Patterns

---

### Q6: Implement a thread-safe Singleton in C#. What are the options and trade-offs?

**Answer:** A Singleton ensures only one instance of a class exists. In C# there are several approaches:

**1. Lazy<T> (recommended):**

```csharp
public sealed class AppSettings
{
    private static readonly Lazy<AppSettings> _instance =
        new Lazy<AppSettings>(() => new AppSettings());

    public static AppSettings Instance => _instance.Value;

    private AppSettings()
    {
        // load config
    }

    public string ConnectionString { get; private set; }
}
```

Thread-safe by default, lazy initialization, clean syntax.

**2. Static readonly field (eager, simplest):**

```csharp
public sealed class Logger
{
    private static readonly Logger _instance = new Logger();
    public static Logger Instance => _instance;
    private Logger() { }
}
```

Thread-safe because CLR guarantees static field initialization is thread-safe. Downside: initialized eagerly when the type is first accessed.

**3. Double-check lock (manual — less common today):**

```csharp
public sealed class Cache
{
    private static volatile Cache _instance;
    private static readonly object _lock = new object();

    public static Cache Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new Cache();
                }
            }
            return _instance;
        }
    }

    private Cache() { }
}
```

**Trade-offs:**
- `Lazy<T>` is the modern best practice — thread-safe, lazy, clean.
- In ASP.NET Core, prefer registering as `AddSingleton<T>()` in the DI container over hand-rolling a Singleton. The DI container manages the lifetime.

---

### Q7: Explain the Factory Method pattern. When would you use it?

**Answer:** Factory Method defines an interface for creating objects but lets subclasses decide which class to instantiate. It decouples client code from concrete types and centralizes creation logic.

```csharp
// Product hierarchy
public interface INotification
{
    void Send(string message);
}

public class EmailNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

public class SmsNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"SMS: {message}");
}

public class PushNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"Push: {message}");
}

// Factory Method
public static class NotificationFactory
{
    public static INotification Create(string channel) => channel.ToLower() switch
    {
        "email" => new EmailNotification(),
        "sms"   => new SmsNotification(),
        "push"  => new PushNotification(),
        _ => throw new ArgumentException($"Unknown channel: {channel}")
    };
}

// Client code never touches concrete classes
INotification notifier = NotificationFactory.Create("sms");
notifier.Send("Hello!");
```

**When to use:**
- The exact type to create is determined at runtime.
- You want to isolate construction logic so adding a new type does not require changing every call site.
- Often combined with DI — register a `Func<string, INotification>` factory delegate.

---

### Q8: What is the Abstract Factory pattern and how does it differ from Factory Method?

**Answer:** Abstract Factory provides an interface for creating **families of related objects** without specifying their concrete classes. Factory Method creates one product; Abstract Factory creates a suite of products that are designed to work together.

```csharp
// Abstract products
public interface IButton { void Render(); }
public interface ITextBox { void Render(); }

// Concrete families
public class WinButton : IButton { public void Render() => Console.WriteLine("Windows Button"); }
public class WinTextBox : ITextBox { public void Render() => Console.WriteLine("Windows TextBox"); }

public class MacButton : IButton { public void Render() => Console.WriteLine("Mac Button"); }
public class MacTextBox : ITextBox { public void Render() => Console.WriteLine("Mac TextBox"); }

// Abstract Factory
public interface IUIFactory
{
    IButton CreateButton();
    ITextBox CreateTextBox();
}

public class WindowsUIFactory : IUIFactory
{
    public IButton CreateButton() => new WinButton();
    public ITextBox CreateTextBox() => new WinTextBox();
}

public class MacUIFactory : IUIFactory
{
    public IButton CreateButton() => new MacButton();
    public ITextBox CreateTextBox() => new MacTextBox();
}

// Client — works with any platform without knowing concrete types
public class FormRenderer
{
    private readonly IUIFactory _factory;
    public FormRenderer(IUIFactory factory) => _factory = factory;

    public void Render()
    {
        _factory.CreateButton().Render();
        _factory.CreateTextBox().Render();
    }
}
```

**Key difference:** Factory Method uses one method to create one product. Abstract Factory uses a family of factory methods to create a coherent set of products. Use Abstract Factory when you must ensure that related objects (e.g., Win button + Win textbox) are always used together.

---

### Q9: Explain the Builder pattern with a C# example. When is it better than a constructor?

**Answer:** Builder separates the construction of a complex object from its representation, allowing the same construction process to create different representations. It is preferable when an object has many optional parameters, making telescoping constructors unreadable.

```csharp
public class HttpRequestConfig
{
    public string Url { get; }
    public string Method { get; }
    public Dictionary<string, string> Headers { get; }
    public string Body { get; }
    public TimeSpan Timeout { get; }
    public int RetryCount { get; }

    // Private constructor — only Builder can create this
    private HttpRequestConfig(Builder builder)
    {
        Url = builder.Url;
        Method = builder.Method;
        Headers = builder.Headers;
        Body = builder.Body;
        Timeout = builder.Timeout;
        RetryCount = builder.RetryCount;
    }

    public class Builder
    {
        public string Url { get; }
        public string Method { get; private set; } = "GET";
        public Dictionary<string, string> Headers { get; } = new();
        public string Body { get; private set; }
        public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);
        public int RetryCount { get; private set; } = 0;

        public Builder(string url) => Url = url;

        public Builder WithMethod(string method) { Method = method; return this; }
        public Builder WithHeader(string key, string value) { Headers[key] = value; return this; }
        public Builder WithBody(string body) { Body = body; return this; }
        public Builder WithTimeout(TimeSpan timeout) { Timeout = timeout; return this; }
        public Builder WithRetries(int count) { RetryCount = count; return this; }

        public HttpRequestConfig Build() => new HttpRequestConfig(this);
    }
}

// Usage — fluent and readable
var config = new HttpRequestConfig.Builder("https://api.example.com/orders")
    .WithMethod("POST")
    .WithHeader("Authorization", "Bearer token123")
    .WithBody("{\"item\":\"widget\"}")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetries(3)
    .Build();
```

**When to use:**
- Objects with many optional parameters (avoids telescoping constructors).
- When construction involves multiple steps or validation.
- Immutable objects that need complex setup.
- .NET examples: `IHostBuilder`, `ConnectionStringBuilder`, `IConfigurationBuilder`.

---

### Q10: Explain the Observer pattern. How does C# support it natively?

**Answer:** Observer defines a one-to-many dependency so that when one object (subject) changes state, all dependents (observers) are notified. C# has first-class support through **events and delegates**.

**Manual implementation:**

```csharp
public interface IObserver<T>
{
    void Update(T data);
}

public class StockTicker // Subject
{
    private readonly List<IObserver<decimal>> _observers = new();
    private decimal _price;

    public decimal Price
    {
        get => _price;
        set { _price = value; Notify(value); }
    }

    public void Subscribe(IObserver<decimal> observer) => _observers.Add(observer);
    public void Unsubscribe(IObserver<decimal> observer) => _observers.Remove(observer);
    private void Notify(decimal price) => _observers.ForEach(o => o.Update(price));
}
```

**Idiomatic C# with events:**

```csharp
public class StockTicker
{
    public event EventHandler<decimal> PriceChanged;

    private decimal _price;
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            PriceChanged?.Invoke(this, value);
        }
    }
}

public class PriceDisplay
{
    public PriceDisplay(StockTicker ticker)
    {
        ticker.PriceChanged += (sender, price) =>
            Console.WriteLine($"New price: {price:C}");
    }
}
```

**Also available:** `IObservable<T>` / `IObserver<T>` (Rx pattern) for reactive streams. In modern .NET, `System.Reactive` (Rx.NET) provides powerful composition over observable sequences. MediatR's `INotification` is another Observer variant used in CQRS architectures.

---

### Q11: Explain the Strategy pattern with a C# example.

**Answer:** Strategy defines a family of algorithms, encapsulates each one, and makes them interchangeable at runtime. It is an alternative to large `if/else` or `switch` blocks that select behavior.

```csharp
// Strategy interface
public interface IDiscountStrategy
{
    decimal ApplyDiscount(decimal price);
}

// Concrete strategies
public class NoDiscount : IDiscountStrategy
{
    public decimal ApplyDiscount(decimal price) => price;
}

public class SeasonalDiscount : IDiscountStrategy
{
    public decimal ApplyDiscount(decimal price) => price * 0.9m; // 10% off
}

public class LoyaltyDiscount : IDiscountStrategy
{
    private readonly int _yearsAsMember;
    public LoyaltyDiscount(int years) => _yearsAsMember = years;
    public decimal ApplyDiscount(decimal price) =>
        price * (1 - Math.Min(_yearsAsMember * 0.05m, 0.25m)); // up to 25% off
}

// Context
public class ShoppingCart
{
    private readonly IDiscountStrategy _discount;

    public ShoppingCart(IDiscountStrategy discount) => _discount = discount;

    public decimal Checkout(decimal subtotal) => _discount.ApplyDiscount(subtotal);
}

// Usage — strategy selected at runtime
IDiscountStrategy strategy = customer.IsLoyal
    ? new LoyaltyDiscount(customer.MemberYears)
    : new SeasonalDiscount();

var cart = new ShoppingCart(strategy);
decimal total = cart.Checkout(200m);
```

Strategy is also naturally expressed as a `Func<T, TResult>` delegate in C#, especially for simple cases:

```csharp
Func<decimal, decimal> discount = price => price * 0.85m;
decimal total = discount(200m); // 170
```

**When to use:** When you have multiple algorithms for the same task and want to select one at runtime without conditionals littering your code.

---

### Q12: What is the Repository pattern? How do you implement it with Entity Framework Core?

**Answer:** Repository abstracts data access behind a collection-like interface, decoupling business logic from persistence technology. It provides a seam for unit testing and makes it easy to swap data stores.

```csharp
// Generic repository interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// EF Core implementation
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Remove(T entity) => _dbSet.Remove(entity);
}

// Domain-specific repository for richer queries
public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetPendingOrdersAsync();
}

public class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Order>> GetPendingOrdersAsync()
        => await _dbSet.Where(o => o.Status == OrderStatus.Pending)
                       .Include(o => o.Items)
                       .ToListAsync();
}
```

**Unit of Work** is often paired with Repository to coordinate saves across multiple repositories within a single transaction:

```csharp
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IRepository<Customer> Customers { get; }
    Task<int> SaveChangesAsync();
}
```

**Debate:** Some argue that `DbContext` itself is already a Unit of Work + Repository. A separate Repository is justified when you want to: (a) hide EF-specific details from the domain layer, (b) enforce query consistency, or (c) support non-EF data sources.

---

### Q13: Explain Dependency Injection in .NET. What are the three service lifetimes?

**Answer:** DI is a technique where an object's dependencies are provided (injected) rather than created internally. ASP.NET Core has a built-in IoC container that supports three lifetimes:

| Lifetime | Behavior | Use Case |
|-----------|----------|----------|
| **Transient** | New instance every time it is requested | Lightweight, stateless services |
| **Scoped** | One instance per HTTP request (or scope) | DbContext, per-request services |
| **Singleton** | One instance for the application's lifetime | Caches, configuration, logging |

```csharp
// Program.cs (minimal API / .NET 6+)
var builder = WebApplication.CreateBuilder(args);

// Transient — new instance per injection
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Scoped — one instance per HTTP request
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Singleton — one instance for the app lifetime
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
```

**Injection mechanisms:**

```csharp
// 1. Constructor injection (preferred)
public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEmailSender _email;

    public OrderService(IOrderRepository repo, IEmailSender email)
    {
        _repo = repo;
        _email = email;
    }
}

// 2. Method injection (for middleware / action methods)
public IActionResult Get([FromServices] IOrderRepository repo) { ... }

// 3. IServiceProvider (service locator — avoid when possible)
var service = serviceProvider.GetRequiredService<IOrderRepository>();
```

**Captive dependency pitfall:** Never inject a Scoped service into a Singleton — the scoped instance becomes a de facto singleton and can cause concurrency issues (e.g., a shared `DbContext`). .NET validates this at startup when `ValidateScopes` is enabled.

---

### Q14: Explain the Adapter pattern with a C# example.

**Answer:** Adapter converts the interface of an existing class into another interface the client expects. It is a structural pattern that lets incompatible interfaces work together — commonly used when integrating third-party libraries.

```csharp
// Target interface your application expects
public interface IPaymentProcessor
{
    Task<PaymentResult> ChargeAsync(decimal amount, string currency, string token);
}

// Third-party SDK with an incompatible interface (Adaptee)
public class StripeClient
{
    public async Task<StripeCharge> CreateChargeAsync(StripeChargeRequest request)
    {
        // Stripe-specific API call
        return new StripeCharge { Id = "ch_123", Status = "succeeded" };
    }
}

// Adapter
public class StripePaymentAdapter : IPaymentProcessor
{
    private readonly StripeClient _stripe;
    public StripePaymentAdapter(StripeClient stripe) => _stripe = stripe;

    public async Task<PaymentResult> ChargeAsync(decimal amount, string currency, string token)
    {
        var request = new StripeChargeRequest
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = currency,
            Source = token
        };

        var charge = await _stripe.CreateChargeAsync(request);

        return new PaymentResult
        {
            Success = charge.Status == "succeeded",
            TransactionId = charge.Id
        };
    }
}

// Registration — swap payment provider by changing this line
builder.Services.AddScoped<IPaymentProcessor, StripePaymentAdapter>();
```

**When to use:**
- Wrapping third-party APIs behind your own interfaces.
- Migrating from one service to another (old interface to new).
- Making legacy code work with modern interfaces.

---

### Q15: Explain the Decorator pattern. How is it different from inheritance?

**Answer:** Decorator dynamically adds behavior to an object without altering its interface. Unlike inheritance (which is static and creates tight coupling), Decorator uses composition and wraps an object of the same interface, allowing behaviors to be stacked at runtime.

```csharp
public interface IMessageSender
{
    Task SendAsync(string to, string body);
}

// Core implementation
public class EmailSender : IMessageSender
{
    public async Task SendAsync(string to, string body)
    {
        // send the email
        await Task.CompletedTask;
    }
}

// Decorator — adds logging
public class LoggingMessageSender : IMessageSender
{
    private readonly IMessageSender _inner;
    private readonly ILogger<LoggingMessageSender> _logger;

    public LoggingMessageSender(IMessageSender inner, ILogger<LoggingMessageSender> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task SendAsync(string to, string body)
    {
        _logger.LogInformation("Sending message to {To}", to);
        await _inner.SendAsync(to, body);
        _logger.LogInformation("Message sent to {To}", to);
    }
}

// Decorator — adds retry logic
public class RetryMessageSender : IMessageSender
{
    private readonly IMessageSender _inner;
    private readonly int _maxRetries;

    public RetryMessageSender(IMessageSender inner, int maxRetries = 3)
    {
        _inner = inner;
        _maxRetries = maxRetries;
    }

    public async Task SendAsync(string to, string body)
    {
        for (int i = 0; i <= _maxRetries; i++)
        {
            try { await _inner.SendAsync(to, body); return; }
            catch when (i < _maxRetries) { await Task.Delay(1000 * (i + 1)); }
        }
    }
}

// Composing decorators — behaviors stacked at runtime
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<IMessageSender>(sp =>
{
    var core = sp.GetRequiredService<EmailSender>();
    var logger = sp.GetRequiredService<ILogger<LoggingMessageSender>>();
    var withLogging = new LoggingMessageSender(core, logger);
    return new RetryMessageSender(withLogging);
});
```

**Key differences from inheritance:**
- Decorator uses composition, not `class B : A`.
- Decorators can be combined in any order at runtime.
- Adding new cross-cutting behavior does not require modifying or subclassing the core class.
- In .NET, libraries like **Scrutor** (`builder.Services.Decorate<IMessageSender, LoggingMessageSender>()`) make this registration cleaner.

---

### Q16: Explain the Command pattern with a C# example.

**Answer:** Command encapsulates a request as an object, allowing you to parameterize clients with different requests, queue operations, log them, and support undo. It decouples the invoker (who triggers the action) from the receiver (who performs it).

```csharp
// Command interface
public interface ICommand
{
    void Execute();
    void Undo();
}

// Receiver
public class TextEditor
{
    public StringBuilder Content { get; } = new();

    public void InsertText(string text) => Content.Append(text);
    public void DeleteLast(int count) => Content.Remove(Content.Length - count, count);
}

// Concrete command
public class InsertTextCommand : ICommand
{
    private readonly TextEditor _editor;
    private readonly string _text;

    public InsertTextCommand(TextEditor editor, string text)
    {
        _editor = editor;
        _text = text;
    }

    public void Execute() => _editor.InsertText(_text);
    public void Undo() => _editor.DeleteLast(_text.Length);
}

// Invoker — manages command history
public class CommandManager
{
    private readonly Stack<ICommand> _history = new();

    public void Execute(ICommand command)
    {
        command.Execute();
        _history.Push(command);
    }

    public void Undo()
    {
        if (_history.TryPop(out var command))
            command.Undo();
    }
}

// Usage
var editor = new TextEditor();
var manager = new CommandManager();

manager.Execute(new InsertTextCommand(editor, "Hello "));
manager.Execute(new InsertTextCommand(editor, "World!"));
Console.WriteLine(editor.Content); // "Hello World!"

manager.Undo();
Console.WriteLine(editor.Content); // "Hello "
```

**Real-world .NET usage:**
- **MediatR** implements a variant: `IRequest<T>` is a command, `IRequestHandler<TRequest, TResponse>` is the handler.
- Useful in CQRS where commands (writes) and queries (reads) are separated.
- Queuing work items (`BackgroundService`, message queues) — each message is a serialized command.

---

### Q17: When would you choose the Factory pattern over the Builder pattern?

**Answer:** The choice depends on the complexity of object construction:

| Concern | Factory | Builder |
|---------|---------|---------|
| **Object complexity** | Simple objects, few parameters | Complex objects, many optional parameters |
| **Variants** | Different types sharing an interface | Same type, different configurations |
| **Creation logic** | Selection logic (which type?) | Assembly logic (how to compose?) |
| **Return type** | Varies — returns different subtypes | Fixed — always returns the same type |

**Use Factory when:**

```csharp
// You need to select which type to create based on runtime data
ILogger logger = LoggerFactory.Create(config.Provider); // returns FileLogger, ConsoleLogger, etc.
```

**Use Builder when:**

```csharp
// You need to assemble a complex object step by step
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { ... })
    .ConfigureLogging(logging => { ... })
    .ConfigureWebHostDefaults(web => { ... })
    .Build();
```

**Combine them** when you need to select a type AND configure it:

```csharp
public class NotificationFactory
{
    public INotification Create(NotificationConfig config) =>
        config.Channel switch
        {
            "email" => new EmailNotification.Builder()
                           .WithFrom(config.From)
                           .WithRetries(config.Retries)
                           .Build(),
            "sms" => new SmsNotification.Builder()
                         .WithProvider(config.SmsProvider)
                         .Build(),
            _ => throw new ArgumentException()
        };
}
```

---

### Q18: How would you choose the right design pattern for a given scenario?

**Answer:** Pattern selection should be driven by the specific problem, not by wanting to use a pattern. Here is a decision framework:

**"I need to control object creation":**
- One instance globally -> **Singleton** (or DI singleton lifetime)
- Select among types at runtime -> **Factory Method**
- Create families of related objects -> **Abstract Factory**
- Complex multi-step construction -> **Builder**

**"I need to add behavior or adapt structure":**
- Wrap an incompatible interface -> **Adapter**
- Add responsibilities without subclassing -> **Decorator**
- Simplify a complex subsystem -> **Facade**

**"I need to manage communication":**
- Notify many objects of state changes -> **Observer** (events)
- Encapsulate a request as an object (undo, queue, log) -> **Command**
- Swap algorithms at runtime -> **Strategy**

**Scenario-based examples:**

| Scenario | Pattern |
|----------|---------|
| Integrate a third-party payment gateway behind your own interface | Adapter |
| Add caching, logging, or retry to an existing service without modifying it | Decorator |
| Calculate shipping cost differently based on carrier and region | Strategy |
| Let users undo/redo actions in an editor | Command |
| Build an HTTP request with many optional headers, body, and timeout | Builder |
| Ensure one connection pool shared across the app | Singleton |
| Send notifications over email, SMS, or push based on user preference | Factory Method |
| Decouple microservices using event-driven communication | Observer |

**Anti-pattern warning:** Do not force patterns where simple code suffices. A `switch` statement is not always a code smell. Patterns add indirection — apply them when the benefit (flexibility, testability, decoupling) outweighs the cost (complexity).

---

### Q19: How do SOLID principles and design patterns complement each other?

**Answer:** SOLID principles are the **why** (the guidelines), and design patterns are the **how** (proven solutions that naturally follow these guidelines).

| SOLID Principle | Patterns That Embody It |
|-----------------|------------------------|
| **SRP** | Command (separates request from execution), Repository (separates persistence from business logic) |
| **OCP** | Strategy (extend algorithms without modifying context), Decorator (extend behavior without modifying core), Factory (add types without changing client) |
| **LSP** | Template Method (subtypes fulfill base contract), Strategy (interchangeable implementations behind a common interface) |
| **ISP** | Adapter (expose only what the client needs), Repository (narrow interfaces per aggregate) |
| **DIP** | All patterns using constructor injection — Factory, Strategy, Repository, etc. |

**Concrete example tying them together:**

```csharp
// DIP + Strategy + OCP + SRP in one design
public interface ITaxCalculator  // ISP — only tax
{
    decimal Calculate(Order order);
}

public class USTaxCalculator : ITaxCalculator  // OCP — new region = new class
{
    public decimal Calculate(Order order) => order.Subtotal * 0.08m;
}

public class EUTaxCalculator : ITaxCalculator
{
    public decimal Calculate(Order order) => order.Subtotal * 0.20m;
}

public class OrderService  // SRP — only orchestration
{
    private readonly ITaxCalculator _tax;  // DIP — depends on abstraction
    private readonly IOrderRepository _repo;

    public OrderService(ITaxCalculator tax, IOrderRepository repo)
    {
        _tax = tax;
        _repo = repo;
    }

    public async Task<decimal> CalculateTotal(int orderId)
    {
        var order = await _repo.GetByIdAsync(orderId);
        return order.Subtotal + _tax.Calculate(order);  // Strategy at runtime
    }
}

// DI wiring — swap strategy per deployment region
if (region == "US")
    builder.Services.AddScoped<ITaxCalculator, USTaxCalculator>();
else
    builder.Services.AddScoped<ITaxCalculator, EUTaxCalculator>();
```

The key takeaway: knowing patterns is insufficient. Understanding **which SOLID principle** a pattern serves helps you choose the right pattern and avoid over-engineering.

---

### Q20: You are designing a notification system that must support email, SMS, push, and future channels. Each channel may need retry logic and logging. Walk through your design.

**Answer:** This is a classic scenario that exercises multiple patterns and SOLID principles together.

**Step 1 — Define the abstraction (DIP + ISP):**

```csharp
public interface INotificationChannel
{
    Task SendAsync(Notification notification);
    bool CanHandle(string channelType);
}

public record Notification(string Recipient, string Subject, string Body, string ChannelType);
```

**Step 2 — Implement channels (OCP — new channel = new class):**

```csharp
public class EmailChannel : INotificationChannel
{
    private readonly ISmtpClient _smtp;
    public EmailChannel(ISmtpClient smtp) => _smtp = smtp;

    public bool CanHandle(string channelType) => channelType == "email";

    public async Task SendAsync(Notification notification)
    {
        await _smtp.SendMailAsync(notification.Recipient, notification.Subject, notification.Body);
    }
}

public class SmsChannel : INotificationChannel
{
    private readonly ISmsGateway _gateway;
    public SmsChannel(ISmsGateway gateway) => _gateway = gateway;

    public bool CanHandle(string channelType) => channelType == "sms";

    public async Task SendAsync(Notification notification)
    {
        await _gateway.SendAsync(notification.Recipient, notification.Body);
    }
}
```

**Step 3 — Add cross-cutting concerns with Decorator:**

```csharp
public class RetryChannelDecorator : INotificationChannel
{
    private readonly INotificationChannel _inner;
    private readonly int _maxRetries;

    public RetryChannelDecorator(INotificationChannel inner, int maxRetries = 3)
    {
        _inner = inner;
        _maxRetries = maxRetries;
    }

    public bool CanHandle(string channelType) => _inner.CanHandle(channelType);

    public async Task SendAsync(Notification notification)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await _inner.SendAsync(notification);
                return;
            }
            catch when (attempt < _maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // exponential backoff
            }
        }
    }
}

public class LoggingChannelDecorator : INotificationChannel
{
    private readonly INotificationChannel _inner;
    private readonly ILogger _logger;

    public LoggingChannelDecorator(INotificationChannel inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public bool CanHandle(string channelType) => _inner.CanHandle(channelType);

    public async Task SendAsync(Notification notification)
    {
        _logger.LogInformation("Sending {Channel} notification to {Recipient}",
            notification.ChannelType, notification.Recipient);
        await _inner.SendAsync(notification);
        _logger.LogInformation("Sent successfully");
    }
}
```

**Step 4 — Dispatch with Strategy (selecting the right channel):**

```csharp
public class NotificationDispatcher
{
    private readonly IEnumerable<INotificationChannel> _channels;

    public NotificationDispatcher(IEnumerable<INotificationChannel> channels)
        => _channels = channels;

    public async Task DispatchAsync(Notification notification)
    {
        var channel = _channels.FirstOrDefault(c => c.CanHandle(notification.ChannelType))
            ?? throw new NotSupportedException($"No channel for: {notification.ChannelType}");

        await channel.SendAsync(notification);
    }
}
```

**Step 5 — Wire it up in DI:**

```csharp
// Register concrete channels wrapped with decorators
builder.Services.AddScoped<EmailChannel>();
builder.Services.AddScoped<SmsChannel>();

builder.Services.AddScoped<IEnumerable<INotificationChannel>>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<LoggingChannelDecorator>>();

    INotificationChannel email = new LoggingChannelDecorator(
        new RetryChannelDecorator(sp.GetRequiredService<EmailChannel>()),
        logger);

    INotificationChannel sms = new LoggingChannelDecorator(
        new RetryChannelDecorator(sp.GetRequiredService<SmsChannel>()),
        logger);

    return new[] { email, sms };
});

builder.Services.AddScoped<NotificationDispatcher>();
```

**Patterns and principles used:**
- **Strategy** — selecting the right channel at runtime via `CanHandle`.
- **Decorator** — stacking retry and logging without modifying channel implementations.
- **DIP** — everything depends on `INotificationChannel`, not concrete classes.
- **OCP** — adding a push channel requires a new class and a DI registration line; zero existing code changes.
- **SRP** — each class handles one concern: sending, retrying, logging, or dispatching.
- **ISP** — `INotificationChannel` is a small, focused interface.

This design is testable (mock any interface), extensible (add channels/decorators freely), and each component has a clear single responsibility.
