# C# & .NET Core — Interview Q&A

> Your strongest area. Focus on clear, concise answers. Expect depth questions here.

---

## OOP Fundamentals

**Q: What are the four pillars of OOP?**

A:
- **Encapsulation** — Bundling data and methods; hiding internal state (private fields, public properties).
- **Abstraction** — Exposing only what's needed; hiding implementation details (interfaces, abstract classes).
- **Inheritance** — Deriving a class from another to reuse and extend behavior.
- **Polymorphism** — Same method name, different behavior. Compile-time (overloading) and runtime (overriding via `virtual`/`override`).

---

**Q: What is the difference between an abstract class and an interface?**

| | Abstract Class | Interface |
|---|---|---|
| Instance | Cannot instantiate | Cannot instantiate |
| Constructor | Can have | Cannot have |
| Fields | Can have fields | Only properties, methods, events (C# 8+ allows default implementations) |
| Multiple inheritance | Only one base class | A class can implement many |
| Access modifiers | Yes | Public by default |
| When to use | Shared base behavior | Define a contract/capability |

> **Example from your work:** `IImportExportService` interface at Energy Exemplar — defined the contract for the NuGet library, allowing multiple teams to code against the interface, not the implementation.

---

**Q: What is the difference between `==` and `.Equals()` in C#?**

A:
- `==` — For value types: compares values. For reference types: compares references (memory address) by default, unless overloaded.
- `.Equals()` — Can be overridden to define logical equality. `string.Equals()` compares content, not reference.

```csharp
string a = new string("hello");
string b = new string("hello");
Console.WriteLine(a == b);       // true (string overloads ==)
Console.WriteLine(object.ReferenceEquals(a, b)); // false
```

---

**Q: What is the difference between `struct` and `class` in C#?**

| | Class | Struct |
|---|---|---|
| Type | Reference type | Value type |
| Memory | Heap | Stack |
| Null | Can be null | Cannot (unless `Nullable<T>`) |
| Inheritance | Supports | Does not support |
| Default constructor | Implicit | Always has a parameterless one |
| Use case | Complex objects, shared state | Small, immutable data (Point, Color, Money) |

---

## .NET Core Specifics

**Q: What is the difference between .NET Framework and .NET Core?**

A:
- **.NET Framework** — Windows-only, mature, large BCL, tied to IIS.
- **.NET Core** — Cross-platform (Windows/Linux/Mac), open-source, modular, high performance, runs on containers.
- **.NET 5+** — Unified platform, merges both. "Core" branding was dropped.

---

**Q: What is Dependency Injection (DI) and how does it work in .NET Core?**

A: DI is a design pattern where objects receive their dependencies from outside rather than creating them internally. .NET Core has a built-in IoC container.

```csharp
// Register in Program.cs / Startup.cs
builder.Services.AddScoped<IMyService, MyService>();
builder.Services.AddSingleton<IConfig, AppConfig>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Inject via constructor
public class MyController : ControllerBase
{
    private readonly IMyService _service;
    public MyController(IMyService service) => _service = service;
}
```

**Service lifetimes:**
- `Singleton` — One instance for the entire app lifetime.
- `Scoped` — One instance per HTTP request (most common for DbContext).
- `Transient` — New instance every time it's requested.

---

**Q: What is middleware in ASP.NET Core?**

A: Middleware is software assembled into the request pipeline to handle requests and responses. Each middleware calls `next()` to pass control to the next component.

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();
app.MapControllers();
```

Common built-in: logging, authentication, exception handling, CORS, static files.

---

**Q: What is async/await in C#? When would you use it?**

A: `async`/`await` is for asynchronous programming without blocking threads. Used for I/O-bound work (database calls, HTTP requests, file reads).

```csharp
public async Task<List<Order>> GetOrdersAsync()
{
    return await _dbContext.Orders.ToListAsync(); // non-blocking
}
```

- `async` — Marks method as asynchronous.
- `await` — Suspends the method until the awaited task completes, freeing the thread.
- Returns `Task` (void), `Task<T>` (result), or `ValueTask<T>` (performance optimization).

> **Common mistake to mention:** Using `.Result` or `.Wait()` on async methods causes deadlock in ASP.NET contexts.

---

**Q: What is LINQ and give an example?**

A: Language Integrated Query — allows querying collections (IEnumerable) and databases (IQueryable) with C# syntax.

```csharp
// Query syntax
var result = from o in orders
             where o.Amount > 1000
             orderby o.Date descending
             select o;

// Method syntax (more common)
var result = orders
    .Where(o => o.Amount > 1000)
    .OrderByDescending(o => o.Date)
    .Select(o => new { o.Id, o.Amount });
```

---

**Q: What is the difference between IEnumerable, ICollection, and IList?**

| Interface | Enumerate | Count | Add/Remove | Index access |
|---|---|---|---|---|
| `IEnumerable<T>` | ✅ | ❌ | ❌ | ❌ |
| `ICollection<T>` | ✅ | ✅ | ✅ | ❌ |
| `IList<T>` | ✅ | ✅ | ✅ | ✅ |

Use the most restrictive type that satisfies your needs.

---

**Q: What is the difference between `ref` and `out` parameters?**

A:
- `ref` — Variable must be initialized before passing. Two-way.
- `out` — Variable doesn't need initialization. Method must assign it before returning. One-way (output only).

```csharp
int.TryParse("123", out int result); // out: method sets result
```

---

**Q: What are generics in C#? Why use them?**

A: Generics allow type-safe data structures and methods without knowing the type at compile time.

```csharp
public class Repository<T> where T : class
{
    public T GetById(int id) { ... }
    public void Add(T entity) { ... }
}
```

Benefits: type safety, code reuse, better performance (avoids boxing/unboxing).

---

**Q: What is the difference between `throw` and `throw ex` in a catch block?**

A:
- `throw` — Re-throws the original exception, preserving the full stack trace.
- `throw ex` — Throws a new exception from the catch point, losing the original stack trace.

Always use `throw` (bare) when re-throwing.

---

**Q: What is a sealed class?**

A: A class marked `sealed` cannot be inherited. Used for security/performance when you don't want subclassing. `string` is a sealed class in .NET.

---

## Entity Framework Core

**Q: What is Entity Framework Core?**

A: ORM (Object-Relational Mapper) for .NET. Maps C# classes to database tables. Supports Code First, Database First approaches.

```csharp
// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
}

// Query
var orders = await _context.Orders
    .Where(o => o.Status == "Active")
    .Include(o => o.Customer)
    .ToListAsync();
```

**Q: What is the difference between eager loading, lazy loading, and explicit loading in EF Core?**

- **Eager loading** — Load related data upfront using `.Include()`. Best for most cases.
- **Lazy loading** — Related data loaded automatically when navigation property is accessed (requires proxies, can cause N+1 problem).
- **Explicit loading** — Manually load related data with `.Load()` when needed.

---

## Exception Handling

**Q: How do you handle exceptions in .NET Core APIs?**

A: Use a global exception handler middleware rather than try/catch in every controller:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        var error = context.Features.Get<IExceptionHandlerFeature>();
        await context.Response.WriteAsJsonAsync(new { error = error?.Error.Message });
    });
});
```

Or use `IExceptionFilter` / `ProblemDetails` pattern.

---

## Quick-Fire Answers

- **Value type vs Reference type** — Value types (int, struct, enum) stored on stack; reference types (class, string, array) on heap.
- **Boxing/Unboxing** — Boxing = wrapping value type in object (heap). Unboxing = extracting value type back. Avoid in hot paths (performance cost).
- **Nullable types** — `int?` = `Nullable<int>`. Use `??` (null coalescing) and `?.` (null conditional).
- **`const` vs `readonly`** — `const` is compile-time, must be primitive/string. `readonly` is runtime, can be set in constructor.
- **Delegates vs Events** — Delegate is a type-safe function pointer. Event wraps a delegate for publisher/subscriber pattern.
- **`IDisposable`** — Implement for releasing unmanaged resources. Use `using` statement to ensure `Dispose()` is called.
