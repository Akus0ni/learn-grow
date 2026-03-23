# C# and .NET — Interview Questions & Answers

> Targeted at **Senior Software Engineer (6+ years)** level.

---

## .NET Platform & Runtime

### Q1: What are the differences between .NET Framework, .NET Core, and .NET (5+)? When would you choose each?

**Answer:**

| Aspect | .NET Framework | .NET Core / .NET 5+ |
|---|---|---|
| Platform | Windows only | Cross-platform (Windows, Linux, macOS) |
| Deployment | Machine-wide install | Side-by-side, self-contained possible |
| Performance | Baseline | Significantly faster (Kestrel, Span\<T\>, etc.) |
| Open Source | Partially | Fully open source |
| Future Investment | Maintenance mode (4.8.x) | Active development (.NET 6, 7, 8, 9) |

**When to choose:**
- **.NET Framework** — only for maintaining legacy apps or when you depend on Windows-only APIs (WCF server-side, Windows Workflow Foundation).
- **.NET 6/7/8/9** — all new development. It is the unified successor to both .NET Core and .NET Framework.
- Avoid starting new projects on .NET Framework; Microsoft's investment is entirely in the unified .NET platform.

---

### Q2: What is .NET Standard and what role does it play in cross-platform library development?

**Answer:** .NET Standard is a **formal specification of APIs** that must be available on all .NET implementations. It is not a runtime — it is a contract.

- A library targeting **.NET Standard 2.0** can be consumed by .NET Framework 4.6.1+, .NET Core 2.0+, Xamarin, Unity, and .NET 5+.
- It was essential during the transition era when projects mixed .NET Framework and .NET Core.
- With .NET 5+ unifying the platform, new libraries should target `net6.0` or later directly unless you still need .NET Framework consumers — in that case, target `netstandard2.0`.

```xml
<!-- Multi-targeting in a .csproj -->
<TargetFrameworks>netstandard2.0;net8.0</TargetFrameworks>
```

---

### Q3: Explain CLR, CTS, and CLS.

**Answer:**

- **CLR (Common Language Runtime)** — the execution engine for .NET. It handles JIT compilation (IL to native code), garbage collection, type safety, exception handling, and thread management. In .NET Core/.NET 5+ the runtime is called **CoreCLR**.

- **CTS (Common Type System)** — defines all the data types the CLR understands (value types, reference types, interfaces, delegates, etc.). It ensures that a `System.Int32` in C# is the same type as `Integer` in VB.NET.

- **CLS (Common Language Specification)** — a subset of CTS that all .NET languages must support for cross-language interoperability. For example, CLS does not allow unsigned integers as public API parameters because some languages don't support them. You can mark assemblies with `[assembly: CLSCompliant(true)]` to get compiler warnings for non-compliant public APIs.

---

## Type System

### Q4: What is the difference between value types and reference types in C#?

**Answer:**

| Aspect | Value Types | Reference Types |
|---|---|---|
| Storage | Stack (or inline in containing object) | Heap (variable holds a reference) |
| Assignment | Copies the value | Copies the reference |
| Default | `0`, `false`, etc. | `null` |
| Examples | `int`, `double`, `bool`, `struct`, `enum` | `class`, `string`, `array`, `delegate`, `interface` |
| Inheritance | Implicitly sealed; derive from `System.ValueType` | Can inherit; derive from `System.Object` |

```csharp
// Value type — independent copies
int a = 10;
int b = a;
b = 20; // a is still 10

// Reference type — shared object
var list1 = new List<int> { 1, 2, 3 };
var list2 = list1;
list2.Add(4); // list1 also has 4 elements now
```

Key senior-level nuance: large structs copied frequently can hurt performance; consider `ref struct`, `in` parameters, or `readonly struct` to mitigate.

---

### Q5: What is boxing and unboxing? What are the performance implications?

**Answer:** **Boxing** wraps a value type in an `object` (heap allocation). **Unboxing** extracts the value type back from the `object`.

```csharp
int num = 42;
object boxed = num;       // Boxing — allocates on the heap
int unboxed = (int)boxed; // Unboxing — copies value back to the stack
```

**Performance implications:**
- Each boxing operation causes a **heap allocation** and a **copy**.
- Frequent boxing in hot paths leads to GC pressure.
- Common culprits: non-generic collections (`ArrayList`), `string.Format` with value-type args (pre-interpolation optimizations), LINQ over value types without proper generic constraints.

**Mitigation:** Use generics (`List<int>` instead of `ArrayList`), `Span<T>`, and avoid casting value types to `object` or interfaces unnecessarily.

---

### Q6: Explain nullable value types and the null-coalescing / null-conditional operators.

**Answer:** Value types cannot be `null` by default. `Nullable<T>` (shorthand `T?`) adds null semantics to value types.

```csharp
int? age = null;
age = 25;

// Null-coalescing operator ??
int displayAge = age ?? 0; // 25

// Null-coalescing assignment ??=
age ??= 30; // no effect since age is already 25

// Null-conditional operator ?.
string? name = GetPerson()?.Address?.City;

// Combined pattern
string city = GetPerson()?.Address?.City ?? "Unknown";
```

With **Nullable Reference Types** (NRT) enabled in C# 8+, the compiler tracks reference-type nullability at compile time:

```csharp
#nullable enable
string? mightBeNull = null;  // OK
string mustNotBeNull = null;  // Warning CS8600
int len = mightBeNull.Length; // Warning CS8602
```

Enabling NRT project-wide (`<Nullable>enable</Nullable>`) is a best practice for new projects.

---

## Memory Management

### Q7: How does garbage collection work in .NET? Explain generations.

**Answer:** The .NET GC is a **generational, mark-and-sweep, compacting** collector.

**Generations:**
- **Gen 0** — newly allocated short-lived objects. Collected most frequently. Very fast (usually < 1 ms).
- **Gen 1** — objects that survived one Gen 0 collection. Acts as a buffer between short- and long-lived objects.
- **Gen 2** — objects that survived Gen 1 collection. Contains long-lived objects. Full GC is expensive.
- **LOH (Large Object Heap)** — objects >= 85,000 bytes. Collected with Gen 2 but not compacted by default (can opt in via `GCSettings.LargeObjectHeapCompactionMode`).

**Process:**
1. **Mark** — GC walks from roots (statics, locals, CPU registers) and marks reachable objects.
2. **Sweep** — unreachable objects are reclaimed.
3. **Compact** — surviving objects are moved together to reduce fragmentation (except LOH by default).

**Workstation vs Server GC:** Server GC uses one heap per logical processor and collects in parallel — better for high-throughput server apps. Configured via `<ServerGarbageCollection>true</ServerGarbageCollection>`.

---

### Q8: What is IDisposable and how does the `using` statement work?

**Answer:** `IDisposable` provides a deterministic cleanup mechanism for **unmanaged resources** (file handles, DB connections, sockets, etc.) since the GC only handles managed memory.

```csharp
public class DatabaseConnection : IDisposable
{
    private SqlConnection _connection;
    private bool _disposed;

    public DatabaseConnection(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // No need for finalizer if Dispose was called
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose(); // Dispose managed resources
            }
            // Release unmanaged resources here if any
            _disposed = true;
        }
    }
}
```

**`using` statement** guarantees `Dispose()` is called even if an exception occurs:

```csharp
// Classic
using (var conn = new DatabaseConnection(connStr))
{
    // work with conn
} // Dispose called here

// C# 8+ using declaration — scoped to enclosing block
using var conn = new DatabaseConnection(connStr);
// Dispose called at end of enclosing method/block
```

**Senior tip:** Only implement the full Dispose pattern (with finalizer) if your class directly holds unmanaged resources. If it only wraps other `IDisposable` objects, a simple `Dispose()` suffices.

---

## Delegates, Events, and LINQ

### Q9: What are delegates in C# and how do they relate to events?

**Answer:** A **delegate** is a type-safe function pointer — it encapsulates a reference to a method with a specific signature.

```csharp
// Define a delegate type
public delegate bool Predicate<T>(T item);

// Built-in equivalents: Func<T, bool>, Action<T>, Predicate<T>

// Multicast delegate (invocation list)
Action<string> logger = Console.WriteLine;
logger += msg => File.AppendAllText("log.txt", msg);
logger("Hello"); // Both methods are invoked
```

**Events** are a restricted wrapper around delegates that enforce publisher-subscriber encapsulation:

```csharp
public class StockTicker
{
    // Only the declaring class can invoke the event
    public event EventHandler<decimal> PriceChanged;

    public void UpdatePrice(decimal newPrice)
    {
        PriceChanged?.Invoke(this, newPrice);
    }
}

// Subscriber
var ticker = new StockTicker();
ticker.PriceChanged += (sender, price) => Console.WriteLine($"New price: {price}");

// Outside the class, you CANNOT do:
// ticker.PriceChanged.Invoke(...)   // Compile error
// ticker.PriceChanged = null;       // Compile error — only += and -= allowed
```

This restriction prevents subscribers from accidentally clearing the entire invocation list or invoking the event from outside.

---

### Q10: Explain the basics of LINQ. What is deferred execution?

**Answer:** LINQ (Language Integrated Query) provides a declarative query syntax over any `IEnumerable<T>` or `IQueryable<T>` source.

```csharp
var employees = new List<Employee> { /* ... */ };

// Method syntax
var seniors = employees
    .Where(e => e.YearsOfExperience > 5)
    .OrderBy(e => e.Name)
    .Select(e => new { e.Name, e.Department });

// Query syntax (compiled to the same method calls)
var seniors2 = from e in employees
               where e.YearsOfExperience > 5
               orderby e.Name
               select new { e.Name, e.Department };
```

**Deferred (lazy) execution:** The query is not evaluated when defined — it executes when the result is enumerated (e.g., `foreach`, `.ToList()`, `.Count()`).

```csharp
var query = employees.Where(e => e.Active); // No execution yet
employees.Add(new Employee { Active = true });
var results = query.ToList(); // NOW it executes — includes the newly added employee
```

**Immediate execution** operators: `ToList()`, `ToArray()`, `ToDictionary()`, `Count()`, `First()`, `Any()`, `Sum()`, aggregate functions.

**Senior pitfall:** Enumerating a deferred query multiple times re-executes it. With `IQueryable<T>` (EF Core), this means multiple database round-trips. Materialize with `.ToList()` when you need to iterate more than once.

---

## Async Programming

### Q11: How does async/await work in C#? What is the Task-based Asynchronous Pattern (TAP)?

**Answer:** `async/await` is syntactic sugar over the **Task-based Asynchronous Pattern (TAP)**. The compiler transforms an `async` method into a state machine.

```csharp
public async Task<string> GetDataAsync(string url)
{
    using var client = new HttpClient();
    string result = await client.GetStringAsync(url); // Yields thread here
    return result.ToUpper(); // Continuation runs after awaited task completes
}
```

**Key mechanics:**
1. When `await` encounters an incomplete `Task`, it captures the current **SynchronizationContext** (or `TaskScheduler`) and returns control to the caller.
2. When the awaited task completes, the continuation resumes on the captured context (UI thread in WPF/WinForms) or on a thread-pool thread (ASP.NET Core has no `SynchronizationContext`).
3. `ConfigureAwait(false)` skips capturing the context — use it in library code to avoid deadlocks and improve performance.

```csharp
// Library code best practice
var data = await httpClient.GetStringAsync(url).ConfigureAwait(false);
```

**Common mistakes at senior level:**
- **Blocking on async** (`task.Result` or `task.Wait()`) — causes deadlocks when a `SynchronizationContext` exists.
- **async void** — only for event handlers. Exceptions in `async void` crash the process.
- **Not awaiting tasks** — fire-and-forget silently swallows exceptions.

---

### Q12: What is `ValueTask<T>` and when should you use it over `Task<T>`?

**Answer:** `ValueTask<T>` is a struct that avoids heap allocation when the result is already available synchronously.

```csharp
// Cache hit returns synchronously — no Task allocation
public ValueTask<Product> GetProductAsync(int id)
{
    if (_cache.TryGetValue(id, out var product))
        return new ValueTask<Product>(product); // No allocation

    return new ValueTask<Product>(LoadFromDatabaseAsync(id)); // Wraps the Task
}
```

**Use `ValueTask<T>` when:**
- The method frequently completes synchronously (cache hits, buffered I/O).
- You're in a high-throughput hot path where Task allocations matter.

**Rules:**
- A `ValueTask<T>` must be awaited **exactly once** and never concurrently consumed.
- Do not call `.Result` on a `ValueTask<T>` that is not yet completed.
- When in doubt, stick with `Task<T>` — it is safer and more flexible.

---

## Generics & Extension Methods

### Q13: What are generics and what are generic constraints?

**Answer:** Generics enable type-safe, reusable code without boxing or casting.

```csharp
public class Repository<T> where T : class, IEntity, new()
{
    private readonly DbContext _context;

    public T GetById(int id) => _context.Set<T>().Find(id);

    public T CreateDefault() => new T(); // Requires new() constraint
}
```

**Constraint types:**

| Constraint | Meaning |
|---|---|
| `where T : struct` | Must be a value type |
| `where T : class` | Must be a reference type |
| `where T : class?` | Nullable reference type |
| `where T : new()` | Must have parameterless constructor |
| `where T : BaseClass` | Must derive from BaseClass |
| `where T : IInterface` | Must implement interface |
| `where T : notnull` | Must be non-nullable |
| `where T : unmanaged` | Must be unmanaged type (no references) |
| `where T : U` | T must derive from another type parameter U |

**Covariance and contravariance** (for interfaces and delegates):
```csharp
IEnumerable<string> strings = new List<string>();
IEnumerable<object> objects = strings; // Covariant (out T)

Action<object> actObj = obj => Console.WriteLine(obj);
Action<string> actStr = actObj; // Contravariant (in T)
```

---

### Q14: What are extension methods and what are their best practices?

**Answer:** Extension methods let you add methods to existing types without modifying them or inheriting.

```csharp
public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}

// Usage — reads like an instance method
string title = "A very long title that needs truncation";
string short = title.Truncate(20);
```

**Best practices:**
- Place in a descriptive, dedicated namespace (not `System`).
- Keep them pure/stateless — they should not maintain state or cause side effects.
- Don't overuse — if you find yourself adding many extensions to a type, consider creating a wrapper or service class.
- Instance methods take precedence over extension methods with the same signature.
- They're great for: fluent APIs, adding functionality to interfaces, LINQ-like operators, and adapting third-party types.

---

## Strings and Collections

### Q15: What is the difference between `String` and `StringBuilder`?

**Answer:** `string` is **immutable** — every modification creates a new string object on the heap. `StringBuilder` is a **mutable buffer** that modifies characters in place.

```csharp
// BAD — creates ~10,000 string objects
string result = "";
for (int i = 0; i < 10_000; i++)
    result += i.ToString(); // O(n^2) due to repeated allocations + copying

// GOOD — single buffer, amortized O(n)
var sb = new StringBuilder();
for (int i = 0; i < 10_000; i++)
    sb.Append(i);
string result = sb.ToString();
```

**When to use `StringBuilder`:** Any loop-based concatenation, or when you are building a string from many parts. For 2-3 concatenations, `string.Concat` or interpolation is fine — the compiler and JIT optimize small cases.

**String interning:** Literal strings are interned (shared). `string.Intern()` can be used explicitly, but be cautious — interned strings live for the process lifetime.

---

### Q16: Compare List\<T\>, Dictionary\<TKey, TValue\>, HashSet\<T\>, and ConcurrentDictionary\<TKey, TValue\>.

**Answer:**

| Collection | Backing Store | Lookup | Add | Use Case |
|---|---|---|---|---|
| `List<T>` | Dynamic array | O(n) by value, O(1) by index | O(1) amortized | Ordered sequence, indexed access |
| `Dictionary<TKey, TValue>` | Hash table | O(1) average | O(1) average | Key-value lookups |
| `HashSet<T>` | Hash table | O(1) average | O(1) average | Unique items, set operations (Union, Intersect) |
| `ConcurrentDictionary<TKey, TValue>` | Striped hash table | O(1) average | O(1) average | Thread-safe key-value store |

```csharp
// ConcurrentDictionary — atomic operations
var cache = new ConcurrentDictionary<string, int>();

// Atomic get-or-add
int value = cache.GetOrAdd("key", key => ExpensiveComputation(key));

// Atomic update
cache.AddOrUpdate("key",
    addValue: 1,
    updateValueFactory: (key, oldValue) => oldValue + 1);
```

**Senior considerations:**
- `Dictionary<TKey, TValue>` is **not thread-safe**. Concurrent reads are safe only if no writes occur simultaneously.
- `ConcurrentDictionary` uses fine-grained locking (lock striping), not a global lock — much better throughput than wrapping `Dictionary` with `lock`.
- For read-heavy scenarios with rare updates, consider `ImmutableDictionary<TKey, TValue>` or `FrozenDictionary<TKey, TValue>` (.NET 8+).

---

## Exception Handling

### Q17: What are the best practices for exception handling in C#?

**Answer:**

1. **Catch specific exceptions**, not `Exception` broadly:
```csharp
try
{
    await httpClient.GetAsync(url);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    _logger.LogWarning("Resource not found: {Url}", url);
    return null;
}
catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
{
    _logger.LogInformation("Request was cancelled");
    throw; // Re-throw to propagate cancellation
}
```

2. **Use exception filters** (`when`) — they don't unwind the stack, preserving the original call stack for debugging.

3. **Re-throw with `throw;`** not `throw ex;`** — the latter resets the stack trace.

4. **Don't use exceptions for control flow** — they are expensive (stack walk, object allocation). Use return values, `TryParse` patterns, or result types.

5. **Create custom exceptions** when callers need to distinguish your errors:
```csharp
public class OrderValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public OrderValidationException(IReadOnlyList<string> errors)
        : base("Order validation failed")
    {
        Errors = errors;
    }
}
```

6. **Always dispose resources** even when exceptions occur — use `using` or `finally`.

7. **Log and wrap at boundaries** — catch low-level exceptions at service boundaries and wrap them in domain-specific exceptions.

---

## Reflection and Attributes

### Q18: What is Reflection and when should you use it?

**Answer:** Reflection allows inspecting and interacting with type metadata at runtime — discovering types, invoking methods, reading attributes, and creating instances dynamically.

```csharp
// Get type info
Type type = typeof(MyService);
// Or from an instance:  obj.GetType()
// Or by name:           Type.GetType("Namespace.MyService")

// Create instance dynamically
object instance = Activator.CreateInstance(type);

// Invoke method
MethodInfo method = type.GetMethod("ProcessOrder");
object result = method.Invoke(instance, new object[] { orderId });

// Read properties
foreach (PropertyInfo prop in type.GetProperties())
{
    Console.WriteLine($"{prop.Name}: {prop.GetValue(instance)}");
}
```

**When to use:** Plugin/framework development, serialization, ORM mapping, DI containers, test frameworks.

**When to avoid:** Hot paths — reflection is slow (100x+ slower than direct calls). Mitigate with:
- **Compiled expressions:** `Expression.Lambda<Func<T>>(...).Compile()`
- **Source generators** (compile-time codegen, no runtime cost)
- `MethodInfo.CreateDelegate()` for repeated invocations

---

### Q19: What are attributes and how do you create custom ones?

**Answer:** Attributes are metadata annotations attached to code elements (classes, methods, properties, parameters). They are consumed via reflection or by compilers/tools.

```csharp
// Custom attribute definition
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CacheResultAttribute : Attribute
{
    public int DurationSeconds { get; }
    public string? CacheKey { get; set; }

    public CacheResultAttribute(int durationSeconds)
    {
        DurationSeconds = durationSeconds;
    }
}

// Usage
public class ProductService
{
    [CacheResult(300, CacheKey = "all-products")]
    public List<Product> GetAllProducts() { /* ... */ }
}

// Reading at runtime
var method = typeof(ProductService).GetMethod(nameof(ProductService.GetAllProducts));
var attr = method.GetCustomAttribute<CacheResultAttribute>();
if (attr != null)
{
    Console.WriteLine($"Cache for {attr.DurationSeconds}s, key={attr.CacheKey}");
}
```

**Common built-in attributes:** `[Obsolete]`, `[Serializable]`, `[JsonPropertyName]`, `[Required]`, `[Authorize]`, `[HttpGet]`, `[CallerMemberName]`.

**Senior note:** With .NET source generators, attributes are increasingly used as compile-time markers (e.g., `[GeneratedRegex]`, `[JsonSerializable]`) that trigger code generation, avoiding reflection entirely.

---

## Dependency Injection

### Q20: How does Dependency Injection work in .NET Core / .NET 5+?

**Answer:** .NET provides a built-in IoC container via `Microsoft.Extensions.DependencyInjection`.

**Service lifetimes:**

| Lifetime | Behavior |
|---|---|
| `Transient` | New instance every time it is requested |
| `Scoped` | One instance per scope (per HTTP request in ASP.NET Core) |
| `Singleton` | One instance for the application lifetime |

```csharp
// Registration in Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Factory registration for complex construction
builder.Services.AddScoped<IPaymentGateway>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new StripePaymentGateway(config["Stripe:ApiKey"]!);
});
```

```csharp
// Constructor injection (preferred)
public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEmailSender _emailSender;

    public OrderService(IOrderRepository repo, IEmailSender emailSender)
    {
        _repo = repo;
        _emailSender = emailSender;
    }
}
```

**Senior pitfalls:**
- **Captive dependency:** A singleton that depends on a scoped service captures and holds the scoped instance forever, causing bugs. The container validates this in Development (`ValidateScopes`).
- **Service Locator anti-pattern:** Avoid injecting `IServiceProvider` and resolving manually. Prefer constructor injection.
- For more advanced scenarios (decorators, named services, convention-based registration), consider libraries like Scrutor or Autofac.

---

## ASP.NET Core Middleware

### Q21: What is middleware in ASP.NET Core and how does the pipeline work?

**Answer:** Middleware are components assembled into the HTTP request pipeline. Each middleware can process the request, optionally pass it to the next middleware, and then process the response on the way back.

```
Request → [Logging] → [Authentication] → [Authorization] → [Routing] → [Endpoint]
Response ← [Logging] ← [Authentication] ← [Authorization] ← [Routing] ← [Endpoint]
```

```csharp
// Inline middleware
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next(context); // Call next middleware
    sw.Stop();
    context.Response.Headers["X-Response-Time"] = $"{sw.ElapsedMilliseconds}ms";
});

// Terminal middleware (does not call next)
app.Map("/health", app => app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
}));
```

```csharp
// Class-based middleware (recommended for non-trivial logic)
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
        }
    }
}

// Registration — order matters!
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Order is critical:** Exception handling middleware should be registered first so it catches errors from all downstream middleware.

---

## Advanced Topics

### Q22: How do you implement the Dispose pattern correctly alongside async disposal?

**Answer:** .NET Core 3.0+ introduced `IAsyncDisposable` for async cleanup (e.g., flushing a network stream).

```csharp
public class MessageBusConnection : IDisposable, IAsyncDisposable
{
    private readonly TcpClient _client;
    private readonly StreamWriter _writer;
    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _writer?.Dispose();
                _client?.Dispose();
            }
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_writer != null)
            await _writer.DisposeAsync().ConfigureAwait(false);

        _client?.Dispose(); // TcpClient has no async dispose
    }
}

// Usage with await using
await using var connection = new MessageBusConnection();
```

The `await using` declaration automatically calls `DisposeAsync()` at end of scope.

---

### Q23: How do you prevent deadlocks with async/await?

**Answer:** Deadlocks typically occur when blocking code (`task.Result`, `task.Wait()`) runs on a thread with a `SynchronizationContext` that requires returning to the same thread for the continuation.

**Classic deadlock scenario (WPF/WinForms/old ASP.NET):**
```csharp
// DEADLOCK — UI thread waits synchronously, but continuation needs the UI thread
public void Button_Click(object sender, EventArgs e)
{
    var data = GetDataAsync().Result; // Blocks the UI thread
}

public async Task<string> GetDataAsync()
{
    var result = await httpClient.GetStringAsync(url); // Needs UI thread to continue
    return result;
}
```

**Solutions:**
1. **Async all the way** — never block on async code:
```csharp
public async void Button_Click(object sender, EventArgs e)
{
    var data = await GetDataAsync(); // No deadlock
}
```

2. **ConfigureAwait(false)** in library code:
```csharp
var result = await httpClient.GetStringAsync(url).ConfigureAwait(false);
// Continuation runs on thread-pool thread, not the original context
```

3. **ASP.NET Core does not have this problem** — it has no `SynchronizationContext`, so `await` continuations always resume on thread-pool threads.

---

### Q24: What are the key differences between `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, and `IReadOnlyList<T>`?

**Answer:**

```
IEnumerable<T>          — forward-only iteration (GetEnumerator)
    └─ ICollection<T>   — adds Count, Add, Remove, Contains
        └─ IList<T>     — adds indexer (this[int index]), Insert, RemoveAt
```

| Interface | Capabilities | When to use as parameter type |
|---|---|---|
| `IEnumerable<T>` | Iterate only | When you just need to loop; supports deferred execution |
| `ICollection<T>` | Count + Add/Remove | When you need to know the size or modify the collection |
| `IList<T>` | Indexed access + Insert/RemoveAt | When you need index-based access |
| `IReadOnlyList<T>` | Count + indexer (read-only) | When you need indexed read access without modification |

**Best practice:** Accept the most general interface you need. Return concrete types or `IReadOnlyList<T>` / `IReadOnlyCollection<T>` to communicate intent.

```csharp
// Accept general type
public decimal CalculateTotal(IEnumerable<OrderLine> lines)
    => lines.Sum(l => l.Price * l.Quantity);

// Return specific read-only type
public IReadOnlyList<Product> GetFeaturedProducts()
    => _products.Where(p => p.IsFeatured).ToList();
```

**Avoid returning `IEnumerable<T>` backed by a deferred query** from public APIs — callers may enumerate multiple times causing repeated execution.

---

### Q25: How do you write thread-safe code in C# beyond locks?

**Answer:** Beyond `lock`, .NET offers several concurrency primitives:

**1. Interlocked — atomic operations on primitives:**
```csharp
private int _counter;
Interlocked.Increment(ref _counter);
Interlocked.CompareExchange(ref _counter, newValue, expectedOldValue);
```

**2. SemaphoreSlim — async-compatible throttling:**
```csharp
private readonly SemaphoreSlim _semaphore = new(maxCount: 3);

public async Task ProcessAsync()
{
    await _semaphore.WaitAsync();
    try
    {
        await DoWorkAsync();
    }
    finally
    {
        _semaphore.Release();
    }
}
```

**3. ReaderWriterLockSlim — multiple readers, single writer:**
```csharp
private readonly ReaderWriterLockSlim _lock = new();

public string Read()
{
    _lock.EnterReadLock();
    try { return _data; }
    finally { _lock.ExitReadLock(); }
}

public void Write(string value)
{
    _lock.EnterWriteLock();
    try { _data = value; }
    finally { _lock.ExitWriteLock(); }
}
```

**4. Channel\<T\> — async-friendly producer-consumer:**
```csharp
var channel = Channel.CreateBounded<WorkItem>(capacity: 100);

// Producer
await channel.Writer.WriteAsync(new WorkItem());

// Consumer
await foreach (var item in channel.Reader.ReadAllAsync())
{
    await ProcessAsync(item);
}
```

**5. Immutable collections** — inherently thread-safe since they cannot be modified.

**Senior guidance:** Prefer higher-level abstractions (`Channel<T>`, `ConcurrentDictionary`, `ImmutableList`) over low-level locks. Use `lock` only for simple critical sections. Avoid `Monitor.Enter/Exit` directly — `lock` is safer (guarantees release in `finally`).
