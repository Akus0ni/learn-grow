# Object-Oriented Programming - Interview Q&A

> Sr. Software Engineer (6+ YoE) | C#/.NET Focus

---

### Q1: What are the four pillars of OOP and why do they matter?

**Answer:** The four pillars are **Encapsulation**, **Abstraction**, **Inheritance**, and **Polymorphism**. Together they promote code that is modular, reusable, maintainable, and extensible.

- **Encapsulation** -- Bundling data and the methods that operate on it into a single unit (class), while restricting direct access to internal state. In C# we achieve this with access modifiers and properties.
- **Abstraction** -- Hiding complex implementation details and exposing only what is necessary. Abstract classes and interfaces are the primary mechanisms.
- **Inheritance** -- Allowing a class to derive from another, inheriting its members and behavior, enabling code reuse and hierarchical modeling.
- **Polymorphism** -- The ability of different types to be treated through a common interface, with behavior determined at compile time (overloading) or runtime (overriding).

In a real system these pillars work together: you encapsulate state, abstract away complexity, inherit shared behavior, and use polymorphism so calling code doesn't need to know the concrete type.

---

### Q2: Explain encapsulation in C# with a practical example. Why is it more than just making fields private?

**Answer:** Encapsulation is about controlling access to an object's state and enforcing invariants -- not merely hiding fields. A well-encapsulated class guarantees it can never be put into an invalid state from the outside.

```csharp
public class BankAccount
{
    private decimal _balance;
    private readonly List<string> _transactions = new();

    public decimal Balance => _balance; // read-only exposure

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit must be positive.");

        _balance += amount;
        _transactions.Add($"+{amount:C} at {DateTime.UtcNow}");
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal must be positive.");
        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds.");

        _balance -= amount;
        _transactions.Add($"-{amount:C} at {DateTime.UtcNow}");
    }

    // Return a copy so callers can't mutate the internal list
    public IReadOnlyList<string> GetTransactionHistory() => _transactions.AsReadOnly();
}
```

Key points beyond "private fields":

- **Validation logic** lives inside the class; callers cannot set `_balance` to -1000.
- **Derived state** (transaction history) is exposed as a read-only copy so the internal collection is protected.
- If the storage mechanism changes (e.g., from `List<string>` to a database), consuming code is unaffected.

---

### Q3: How does abstraction differ from encapsulation?

**Answer:** They are related but distinct:

| Aspect | Encapsulation | Abstraction |
|---|---|---|
| Focus | **How** data is protected | **What** is exposed to the consumer |
| Mechanism | Access modifiers, properties | Abstract classes, interfaces, method signatures |
| Goal | Prevent invalid state, hide implementation | Reduce complexity, define contracts |

A practical way to remember: encapsulation is about **hiding the internals** of a single class; abstraction is about **hiding entire implementation strategies** behind a contract.

```csharp
// Abstraction: the caller only knows about INotificationService
public interface INotificationService
{
    Task SendAsync(string recipient, string message);
}

// Encapsulation: internal details of how SMTP is configured are hidden
public class EmailNotificationService : INotificationService
{
    private readonly SmtpClient _client;
    private readonly string _fromAddress;

    public EmailNotificationService(SmtpClient client, string fromAddress)
    {
        _client = client;
        _fromAddress = fromAddress;
    }

    public async Task SendAsync(string recipient, string message)
    {
        var mail = new MailMessage(_fromAddress, recipient, "Notification", message);
        await _client.SendMailAsync(mail);
    }
}
```

---

### Q4: What is the difference between an abstract class and an interface in C#? When would you choose one over the other?

**Answer:**

| Feature | Abstract Class | Interface |
|---|---|---|
| Inheritance | Single only | Multiple allowed |
| Constructors | Yes | No |
| Fields / state | Yes | No (properties only, no backing field) |
| Access modifiers on members | Any | `public` by default; `private` requires default impl (C# 8+) |
| Default implementations | Always supported | C# 8+ only |
| Versioning | Add non-abstract members freely | Adding members without defaults breaks implementors (pre-C# 8) |

**Choose an abstract class when:**
- You need to share **state or constructor logic** among related types.
- There is a clear "is-a" relationship (e.g., `Shape` -> `Circle`, `Rectangle`).
- You want to provide a **template method** pattern with partial implementation.

**Choose an interface when:**
- You want to define a **capability contract** that unrelated types can implement (e.g., `IDisposable`, `ISerializable`).
- You need **multiple inheritance** of contracts.
- You are designing for **dependency injection** and testability.

```csharp
public abstract class RepositoryBase<T> where T : class
{
    protected readonly DbContext Context;

    protected RepositoryBase(DbContext context) => Context = context;

    // Shared logic
    public virtual async Task<T?> GetByIdAsync(int id) => await Context.Set<T>().FindAsync(id);

    // Force subclasses to implement
    public abstract Task<IEnumerable<T>> GetAllAsync();
}

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
```

---

### Q5: Explain `virtual`, `override`, and `new` keywords in C#. What happens if you forget `override`?

**Answer:**

- **`virtual`** -- Declares that a base class method *can* be overridden by derived classes. Without it, the method is non-virtual and cannot be overridden.
- **`override`** -- Replaces the base implementation with a new one in the derived class. The runtime dispatches to the most-derived override (runtime polymorphism).
- **`new`** -- *Hides* the base member rather than overriding it. The method called depends on the **compile-time type** of the reference, not the runtime type.

```csharp
public class Animal
{
    public virtual string Speak() => "...";
}

public class Dog : Animal
{
    public override string Speak() => "Woof!";  // true override
}

public class Cat : Animal
{
    public new string Speak() => "Meow!";  // hides, does NOT override
}

// Usage
Animal myDog = new Dog();
Animal myCat = new Cat();

Console.WriteLine(myDog.Speak()); // "Woof!" -- override kicks in
Console.WriteLine(myCat.Speak()); // "..."   -- base method called because 'new' hides, doesn't override

Cat actualCat = new Cat();
Console.WriteLine(actualCat.Speak()); // "Meow!" -- called through Cat reference
```

If you forget `override` on a method that shares the signature of a virtual base method, the compiler issues a warning and implicitly treats it as `new`. This almost always indicates a bug. Use `override` deliberately to get polymorphic dispatch.

---

### Q6: What is composition vs inheritance? Why is "favor composition over inheritance" common advice?

**Answer:** **Inheritance** models an "is-a" relationship and is defined at compile time. **Composition** models a "has-a" relationship by holding references to other objects that provide behavior.

Problems with inheritance:
- Tight coupling to the base class (fragile base class problem).
- Single inheritance in C# limits flexibility.
- Changes in the base can break derived classes unexpectedly.
- Deep hierarchies become hard to reason about.

Composition lets you assemble behavior from small, focused components and swap them at runtime.

```csharp
// Inheritance approach -- rigid
public class RobotDog : Dog
{
    public void Recharge() { /* ... */ }
}
// Problem: what if we need a RobotBird? We can't inherit from both Robot and Bird.

// Composition approach -- flexible
public interface IMovementStrategy
{
    void Move();
}

public class WalkMovement : IMovementStrategy
{
    public void Move() => Console.WriteLine("Walking...");
}

public class FlyMovement : IMovementStrategy
{
    public void Move() => Console.WriteLine("Flying...");
}

public class Robot
{
    private IMovementStrategy _movement;

    public Robot(IMovementStrategy movement) => _movement = movement;

    public void ChangeMovement(IMovementStrategy movement) => _movement = movement;
    public void Move() => _movement.Move();
}

// Now we can compose any behavior at runtime
var robot = new Robot(new WalkMovement());
robot.Move(); // Walking...
robot.ChangeMovement(new FlyMovement());
robot.Move(); // Flying...
```

**Rule of thumb:** Use inheritance when there is a genuine, stable "is-a" relationship with shared state/behavior. Use composition for everything else.

---

### Q7: What is the difference between method overloading and method overriding?

**Answer:**

| Aspect | Overloading | Overriding |
|---|---|---|
| Binding | Compile-time (static polymorphism) | Runtime (dynamic polymorphism) |
| Scope | Same class (or base + derived) | Base and derived class |
| Signature | Same name, **different parameters** | Same name **and** same parameters |
| Keywords | None required | `virtual` in base, `override` in derived |
| Return type | Can differ | Must match (or be covariant for some cases) |

```csharp
public class Logger
{
    // Overloading -- same name, different parameter lists
    public void Log(string message) => Console.WriteLine(message);
    public void Log(string message, LogLevel level) => Console.WriteLine($"[{level}] {message}");
    public void Log(Exception ex) => Console.WriteLine(ex.ToString());
}

public class BaseProcessor
{
    // Overriding -- virtual + override
    public virtual void Process(Order order)
    {
        // default processing
    }
}

public class PriorityProcessor : BaseProcessor
{
    public override void Process(Order order)
    {
        // priority-specific processing
        base.Process(order); // optionally call base
    }
}
```

---

### Q8: List all access modifiers in C# and explain when you would use each.

**Answer:**

| Modifier | Accessibility | Typical Use |
|---|---|---|
| `public` | Everywhere | API surface, interface members |
| `private` | Containing class only | Internal state, helper methods |
| `protected` | Containing class + derived classes | Members meant for subclass customization |
| `internal` | Same assembly | Implementation classes not meant for external consumers |
| `protected internal` | Same assembly **OR** derived classes (even outside assembly) | Framework extension points |
| `private protected` | Same assembly **AND** derived classes | Tightly controlled inheritance within the same assembly |
| `file` (C# 11) | Same source file only | Source-generated types, avoiding naming collisions |

**Senior-level nuance:** Default accessibility varies by context. Class members default to `private`. Top-level types default to `internal`. Interface members default to `public`. Structs cannot have `protected` members because structs cannot be inherited.

```csharp
public class OrderService
{
    private readonly IOrderRepository _repo;           // private -- internal detail
    protected virtual decimal TaxRate => 0.08m;        // protected -- derived classes may override
    internal void RecalculateTotals() { /* ... */ }    // internal -- only within this assembly
    public Order GetOrder(int id) => _repo.GetById(id); // public -- API surface
}
```

---

### Q9: What is the difference between static and instance members? When are static members appropriate?

**Answer:** **Instance members** belong to a specific object; each instance has its own copy of instance fields. **Static members** belong to the type itself and are shared across all instances.

```csharp
public class ConnectionPool
{
    private static readonly List<DbConnection> _pool = new(); // shared across all
    private static int _maxSize = 10;                         // shared config

    public static ConnectionPool Instance { get; } = new();   // singleton

    private readonly string _name;  // per-instance

    private ConnectionPool() => _name = "Default";

    public static void Configure(int maxSize) => _maxSize = maxSize;
    public DbConnection Acquire() { /* ... */ return null!; }
}
```

**When to use static:**
- Utility/helper methods with no state dependency (e.g., `Math.Max`, `Path.Combine`).
- Factory methods, singleton accessors.
- Extension methods (must be in a static class).
- Constants and truly global configuration.

**When to avoid static:**
- When the member needs to be mocked/injected for testing.
- When state varies per instance or per request (e.g., in web apps, static state is shared across all HTTP requests, causing concurrency bugs).
- When you find yourself building a "God class" of static utilities -- that's a design smell.

---

### Q10: What is a sealed class in C#? Why and when would you seal a class?

**Answer:** A `sealed` class cannot be inherited from. Applying `sealed` to a method (that overrides a virtual base method) prevents further overriding in deeper derived classes.

```csharp
public sealed class StringHelper
{
    public static string Truncate(string input, int maxLength)
        => input.Length <= maxLength ? input : input[..maxLength] + "...";
}

// This would cause a compile error:
// public class BetterStringHelper : StringHelper { }

// Sealing a single method
public class A
{
    public virtual void Execute() { }
}

public class B : A
{
    public sealed override void Execute() { /* final implementation */ }
}

public class C : B
{
    // Compile error: cannot override sealed member
    // public override void Execute() { }
}
```

**Reasons to seal a class:**
1. **Security / correctness** -- prevent subclasses from breaking invariants (e.g., immutable types).
2. **Performance** -- the JIT can devirtualize calls on sealed types, enabling inlining. The .NET runtime team seals classes aggressively for this reason.
3. **Design clarity** -- signals "this type is not designed for extension."

Since .NET 6+, the framework and analyzers encourage sealing classes by default (`CA1852`). The guideline is: if you did not design a class for inheritance, seal it.

---

### Q11: What are partial classes in C#? What are real-world use cases?

**Answer:** The `partial` keyword allows a single class (or struct, interface, or method) to be split across multiple files. The compiler merges them into one type at compile time.

```csharp
// File: User.cs (hand-written)
public partial class User
{
    public string FullName => $"{FirstName} {LastName}";

    public bool IsActive()
    {
        // business logic
        return true;
    }
}

// File: User.Generated.cs (auto-generated by EF Core tooling, source generator, etc.)
public partial class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

**Real-world use cases:**
1. **Code generation** -- Entity Framework scaffolding, gRPC/protobuf, WinForms/WPF designers generate partial classes so your custom code lives in a separate file and isn't overwritten on regeneration.
2. **Source generators** (C# 9+) -- Generators emit partial class files that augment your hand-written partial class (e.g., `System.Text.Json`, `LoggerMessage`).
3. **Large classes** -- Splitting a big class for organizational purposes (though this often signals the class should be refactored).

**Rules:**
- All parts must use `partial` and be in the same namespace and assembly.
- Access modifiers, base class, and generic constraints must be consistent.
- If any part is `sealed` or `abstract`, the whole type is.

---

### Q12: Explain coupling and cohesion. What does "high cohesion, low coupling" mean in practice?

**Answer:**

- **Coupling** -- The degree of interdependence between modules/classes. High coupling means changing one class forces changes in many others.
- **Cohesion** -- The degree to which members of a module/class are related and focused on a single responsibility. High cohesion means a class does one thing well.

**"High cohesion, low coupling"** is the goal because it produces systems that are easier to understand, test, and modify independently.

```csharp
// BAD: Low cohesion (does too much), high coupling (depends on concrete types)
public class OrderManager
{
    public void CreateOrder(Order order)
    {
        var db = new SqlConnection("...");       // coupled to SQL Server
        db.Open();
        // save order...

        var smtp = new SmtpClient("smtp.example.com"); // coupled to SMTP
        smtp.Send(new MailMessage(/* ... */));           // sending email here too

        var logger = new FileLogger("C:\\logs");         // coupled to file system
        logger.Log("Order created");
    }
}

// GOOD: High cohesion (single responsibility), low coupling (depends on abstractions)
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly INotificationService _notifications;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        INotificationService notifications,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task CreateOrderAsync(Order order)
    {
        await _repository.SaveAsync(order);
        await _notifications.SendAsync(order.CustomerEmail, "Order confirmed");
        _logger.LogInformation("Order {OrderId} created", order.Id);
    }
}
```

The refactored version depends on **interfaces** (low coupling) and does only **order orchestration** (high cohesion). Each collaborator can be tested, replaced, or modified independently.

---

### Q13: Explain association, aggregation, and composition in OOP. How do they differ?

**Answer:** These describe the strength of "has-a" relationships between objects.

| Relationship | Ownership | Lifetime dependency | Example |
|---|---|---|---|
| **Association** | None | Independent | A `Teacher` teaches many `Students`; both exist independently |
| **Aggregation** | Weak "has-a" | Child can outlive parent | A `Team` has `Players`; players exist if the team is disbanded |
| **Composition** | Strong "has-a" | Child cannot outlive parent | A `House` has `Rooms`; rooms are destroyed when the house is demolished |

```csharp
// Association -- no ownership, just a reference
public class Teacher
{
    public List<Student> Students { get; set; } = new(); // Teacher knows about students
}

// Aggregation -- weak ownership, injected from outside
public class Team
{
    private readonly List<Player> _players;

    public Team(List<Player> players)
    {
        _players = players; // players created externally, can exist without Team
    }
}

// Composition -- strong ownership, created and destroyed internally
public class House
{
    private readonly List<Room> _rooms;

    public House(int numberOfRooms)
    {
        // House creates and owns the rooms
        _rooms = Enumerable.Range(1, numberOfRooms)
            .Select(i => new Room($"Room-{i}"))
            .ToList();
    }
    // When House is GC'd, Rooms go too (no external references)
}
```

**In C#**, the GC handles memory, so composition is enforced by **design** rather than destructors. The key signal is: does the parent **create** the child internally (composition) or **receive** it from outside (aggregation)?

---

### Q14: What is constructor chaining in C#? How does it work with `this` and `base`?

**Answer:** Constructor chaining is the practice of having one constructor call another to avoid duplicating initialization logic.

- **`this(...)`** -- Calls another constructor in the **same** class.
- **`base(...)`** -- Calls a constructor in the **base** class.

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;

    // Primary constructor -- all logic lives here
    public ApiClient(HttpClient httpClient, string baseUrl, TimeSpan timeout, ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _timeout = timeout;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Chain to primary with default timeout
    public ApiClient(HttpClient httpClient, string baseUrl, ILogger logger)
        : this(httpClient, baseUrl, TimeSpan.FromSeconds(30), logger)
    { }

    // Chain to above with default HttpClient
    public ApiClient(string baseUrl, ILogger logger)
        : this(new HttpClient(), baseUrl, logger)
    { }
}

// base() example
public class AuthenticatedApiClient : ApiClient
{
    private readonly string _apiKey;

    public AuthenticatedApiClient(string baseUrl, string apiKey, ILogger logger)
        : base(baseUrl, logger)  // calls ApiClient(string, ILogger)
    {
        _apiKey = apiKey;
    }
}
```

**Execution order:** When you write `: this(...)` or `: base(...)`, the chained constructor runs **first**, then the body of the current constructor executes. Field initializers run before any constructor body.

---

### Q15: Explain deep copy vs shallow copy in C#. How do you implement each?

**Answer:**

- **Shallow copy** -- Copies the object's value-type fields and copies references for reference-type fields. Both original and copy share the same referenced objects.
- **Deep copy** -- Copies everything recursively so the clone is fully independent.

```csharp
public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;

    public Address DeepCopy() => new Address { City = City, Street = Street };
}

public class Employee : ICloneable
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();

    // Shallow copy -- Address is shared
    public Employee ShallowCopy() => (Employee)MemberwiseClone();

    // Deep copy -- Address is also cloned
    public Employee DeepCopy()
    {
        var clone = (Employee)MemberwiseClone();
        clone.Address = Address.DeepCopy();
        return clone;
    }

    // ICloneable (returns object -- generally discouraged due to lack of type safety)
    public object Clone() => DeepCopy();
}

// Demonstration
var original = new Employee { Name = "Alice", Address = new Address { City = "Berlin" } };

var shallow = original.ShallowCopy();
shallow.Address.City = "Munich";
Console.WriteLine(original.Address.City); // "Munich" -- shared reference!

var deep = original.DeepCopy();
deep.Address.City = "Hamburg";
Console.WriteLine(original.Address.City); // "Munich" -- independent copy
```

**Practical deep copy strategies:**
1. **Manual cloning** (as above) -- most control, most work.
2. **Serialization round-trip** -- `JsonSerializer.Serialize` then `Deserialize`. Easy but slower and requires serializable types.
3. **Record types with `with` expressions** -- shallow by default, but for simple types with no mutable reference fields, behaves like deep copy.

---

### Q16: How does polymorphism manifest in C# beyond simple method overriding?

**Answer:** Polymorphism in C# appears in several forms:

1. **Subtype (runtime) polymorphism** -- `virtual`/`override`, interface dispatch.
2. **Ad-hoc polymorphism** -- Method overloading, operator overloading.
3. **Parametric polymorphism** -- Generics.
4. **Pattern-matching polymorphism** (C# 7+) -- `switch` expressions with type patterns.

```csharp
// 1. Subtype polymorphism via interfaces
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

// 2. Parametric polymorphism (generics)
public class Repository<T> where T : class, IEntity
{
    public T? FindById(int id) { /* works for any entity type */ return default; }
}

// 3. Operator overloading
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Currency mismatch.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}

// 4. Pattern-matching polymorphism
public static string Describe(IShape shape) => shape switch
{
    Circle { Radius: > 100 }  => "Large circle",
    Circle c                   => $"Circle with radius {c.Radius}",
    Rectangle { Width: var w, Height: var h } when w == h => "Square",
    Rectangle r                => $"Rectangle {r.Width}x{r.Height}",
    _                          => "Unknown shape"
};
```

At the senior level, knowing **when** to use each form matters. Generics reduce code duplication across types. Pattern matching can sometimes replace visitor patterns or long `if-else` chains more cleanly.

---

### Q17: Can you have multiple inheritance in C#? How do you work around the limitation?

**Answer:** C# does **not** support multiple class inheritance (to avoid the diamond problem). However, you can achieve the effect through:

1. **Multiple interface implementation** -- A class can implement any number of interfaces.
2. **Default interface methods** (C# 8+) -- Interfaces can provide default implementations, resembling traits/mixins.
3. **Composition** -- Hold references to objects that provide needed behavior.

```csharp
// Multiple interfaces with default implementations (C# 8+)
public interface ILoggable
{
    // Default implementation -- acts like a mixin
    void Log(string message) => Console.WriteLine($"[{GetType().Name}] {message}");
}

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? ModifiedAt { get; set; }

    void Stamp()
    {
        if (CreatedAt == default)
            CreatedAt = DateTime.UtcNow;
        else
            ModifiedAt = DateTime.UtcNow;
    }
}

// "Inherits" behavior from both interfaces
public class Invoice : ILoggable, IAuditable
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public decimal Total { get; set; }
}

// Usage -- must cast to interface to call default methods
ILoggable invoice = new Invoice();
invoice.Log("Created"); // [Invoice] Created
```

**Caveat:** Default interface methods require casting to the interface type to call them, which is less ergonomic than true inheritance. For complex shared behavior, composition with DI is usually the cleaner choice.

---

### Q18: What is the difference between `abstract` methods and `virtual` methods?

**Answer:**

| Aspect | `abstract` | `virtual` |
|---|---|---|
| Body in base class | **None** -- must be overridden | **Has a default** implementation |
| Containing type | Must be in an `abstract` class | Can be in any non-sealed class |
| Override required | Yes (compile error if not overridden) | No (optional to override) |
| Use case | Force subclasses to provide implementation | Provide sensible default that can be customized |

```csharp
public abstract class PaymentProcessor
{
    // Abstract -- every processor MUST define how to charge
    public abstract PaymentResult Charge(decimal amount);

    // Virtual -- default logging that can be customized
    public virtual void LogTransaction(PaymentResult result)
    {
        Console.WriteLine($"Transaction {result.TransactionId}: {result.Status}");
    }

    // Template method pattern: concrete method using abstract + virtual
    public PaymentResult ProcessPayment(decimal amount)
    {
        var result = Charge(amount);    // abstract -- subclass defines
        LogTransaction(result);          // virtual -- subclass may customize
        return result;
    }
}

public class StripeProcessor : PaymentProcessor
{
    public override PaymentResult Charge(decimal amount)
    {
        // Stripe-specific charging logic
        return new PaymentResult { TransactionId = Guid.NewGuid(), Status = "Success" };
    }

    // LogTransaction is NOT overridden -- default behavior is fine
}
```

**Design guideline:** Use `abstract` when there is no reasonable default. Use `virtual` when there is a sensible default but you want to allow customization. Avoid making everything virtual "just in case" -- it's a maintenance burden and a potential source of bugs.

---

### Q19: What is the significance of the `IDisposable` pattern and how does it relate to encapsulation?

**Answer:** `IDisposable` is a core interface for deterministic cleanup of unmanaged resources (file handles, DB connections, sockets). It relates to encapsulation because the class **hides** resource management details and provides a clean contract for release.

```csharp
public class ManagedFileWriter : IDisposable
{
    private StreamWriter? _writer;
    private bool _disposed;

    public ManagedFileWriter(string path)
    {
        _writer = new StreamWriter(path);
    }

    public void WriteLine(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer!.WriteLine(text);
    }

    // The Dispose pattern
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _writer?.Dispose();
            _writer = null;
        }

        _disposed = true;
    }

    // Destructor -- safety net if Dispose wasn't called
    ~ManagedFileWriter() => Dispose(disposing: false);
}

// Usage -- encapsulated cleanup via 'using'
using var writer = new ManagedFileWriter("output.txt");
writer.WriteLine("Hello");
// Dispose called automatically at end of scope
```

**Senior-level points:**
- Always use `using` statements/declarations to guarantee `Dispose` is called.
- The `protected virtual Dispose(bool)` pattern allows derived classes to add their own cleanup.
- In modern C#, if your class only wraps managed `IDisposable` resources and has no finalizer, the simplified pattern (just implementing `Dispose()`) is sufficient. The full pattern with a finalizer is only needed for direct unmanaged resource ownership.

---

### Q20: How do you decide between using records, classes, and structs from an OOP design perspective in C#?

**Answer:**

| Type | Semantics | Equality | Mutability | Heap/Stack | Best for |
|---|---|---|---|---|---|
| `class` | Reference | Reference equality by default | Mutable by default | Heap | Entities with identity, services, complex objects |
| `struct` | Value | Value equality by default | Should be immutable | Stack (usually) | Small, lightweight data (< ~16 bytes), no identity |
| `record class` | Reference | Value equality (generated) | Immutable by convention (`init`) | Heap | DTOs, events, immutable domain models |
| `record struct` | Value | Value equality (generated) | Immutable with `readonly` | Stack | Small value objects with equality semantics |

```csharp
// Entity -- use class (identity matters, mutable state)
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Order> Orders { get; } = new();
}

// Value object -- use record (equality by value, immutable)
public record Address(string Street, string City, string PostalCode);

// Lightweight value -- use readonly record struct
public readonly record struct Coordinate(double Latitude, double Longitude);

// Small perf-critical value -- use struct
public readonly struct Color
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public Color(byte r, byte g, byte b) => (R, G, B) = (r, g, b);
}

// Records support non-destructive mutation via 'with'
var addr = new Address("Main St", "Berlin", "10115");
var newAddr = addr with { City = "Munich" }; // creates a new Address
```

**Decision framework:**
1. Does the object have a unique identity (e.g., database ID)? Use `class`.
2. Is it defined entirely by its values with no identity? Use `record`.
3. Is it small (2-3 fields), frequently allocated, and perf-critical? Consider `struct` or `record struct`.
4. Do you need immutability with value equality and readable `ToString()`? `record` gives you all of that for free.

---

*Prepared for Sr. Software Engineer interview -- Lenze Mechatronics*
