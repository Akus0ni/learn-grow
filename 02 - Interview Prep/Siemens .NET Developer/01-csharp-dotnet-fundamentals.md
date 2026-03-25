# C# / .NET Fundamentals — Siemens Interview

## Q1. Abstract Class vs Interface

| Feature | Abstract Class | Interface |
|---------|---------------|-----------|
| Methods | Abstract + concrete | All abstract (pre-C# 8) |
| Fields | Can have fields | No fields (only properties) |
| Constructor | Yes | No |
| Access modifiers | Any | Public by default |
| Multiple inheritance | No | Yes |
| When to use | Shared base + some impl | Contract/capability |

Since C# 8, interfaces can have **default method implementations**.

## Q2. Four Pillars of OOP

**Encapsulation** — bundling data + methods, hiding internals:
```csharp
public class BankAccount
{
    private decimal _balance;
    public decimal Balance => _balance;
    public void Deposit(decimal amount) => _balance += amount;
}
```

**Abstraction** — hiding complexity, exposing essentials:
```csharp
public abstract class Shape { public abstract double Area(); }
```

**Inheritance** — reusing code via parent-child:
```csharp
public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;
}
```

**Polymorphism** — one interface, many forms:
```csharp
Shape s = new Circle { Radius = 5 };
Console.WriteLine(s.Area()); // calls Circle.Area() at runtime
```

## Q3. SOLID Principles (Frequently asked at Siemens)

**S — Single Responsibility:** One class, one reason to change.
```csharp
public class OrderService { public void PlaceOrder(Order o) { } }
public class EmailService { public void SendConfirmation(Order o) { } }
```

**O — Open/Closed:** Open for extension, closed for modification.
```csharp
public abstract class DiscountStrategy { public abstract decimal Calculate(decimal price); }
public class SeasonalDiscount : DiscountStrategy
{
    public override decimal Calculate(decimal price) => price * 0.9m;
}
```

**L — Liskov Substitution:** Subtypes must be substitutable for base types.
```csharp
// If Square inherits Rectangle but overrides Width to also set Height,
// it breaks LSP. Use separate abstractions instead.
```

**I — Interface Segregation:** Don't force clients to depend on unused methods.
```csharp
public interface IWorkable { void Work(); }
public interface IFeedable { void Eat(); }
```

**D — Dependency Inversion:** Depend on abstractions, not concretions.
```csharp
public class OrderService
{
    private readonly IDatabase _db;
    public OrderService(IDatabase db) => _db = db;
}
```

## Q4. Dependency Injection — Types & Service Lifetimes

```csharp
// 1. Constructor Injection (preferred)
public class OrderService(IRepository repo) { }

// 2. Method Injection
public void Process(ILogger logger) { }

// 3. Property Injection
public ILogger Logger { get; set; }
```

**Service lifetimes in .NET DI:**
- `AddTransient<T>()` — new instance every time
- `AddScoped<T>()` — one instance per request/scope
- `AddSingleton<T>()` — one instance for app lifetime

## Q5. Async/Await & Common Pitfalls

```csharp
public async Task<string> GetDataAsync()
{
    var result = await _httpClient.GetStringAsync("https://api.example.com/data");
    return result;
}
```

**Pitfalls (Siemens asks this):**
1. **Deadlock:** `.Result` or `.Wait()` on async code blocks the thread — always use `await`
2. **async void:** Only for event handlers. Use `async Task` otherwise
3. **ConfigureAwait(false):** Use in library code to avoid capturing sync context
4. **Fire and forget:** Without error handling = swallowed exceptions

## Q6. `ref`, `out`, and `in` Parameters

```csharp
// ref: must be initialized before passing, can read+write
void Increment(ref int x) => x++;

// out: need not be initialized, MUST be assigned inside method
bool TryParse(string s, out int result) { result = 0; return true; }

// in: passed by ref but READ-ONLY (perf optimization for large structs)
double Calculate(in LargeStruct data) => data.Value * 2;
```

## Q7. IEnumerable<T> vs IQueryable<T>

| Feature | IEnumerable<T> | IQueryable<T> |
|---------|----------------|---------------|
| Execution | In-memory (client-side) | Out-of-process (e.g., SQL) |
| Deferred exec | Yes | Yes |
| Best for | LINQ to Objects | LINQ to SQL / EF Core |
| Filter | Loads all, then filters | Translates to SQL, filters server-side |

## Q8. Garbage Collection in .NET

- **Generational GC:** Gen 0 (short-lived, frequent), Gen 1 (buffer), Gen 2 (long-lived, rare)
- **Large Object Heap (LOH):** Objects >= 85KB, collected with Gen 2
- Use `IDisposable` + `using` for deterministic cleanup of unmanaged resources

## Q9. Delegates and Events

```csharp
// Delegate: type-safe function pointer
public delegate void Notify(string message);

// Built-in delegates
Action<string> log = msg => Console.WriteLine(msg);   // void return
Func<int, int, int> add = (a, b) => a + b;            // returns value
Predicate<int> isEven = n => n % 2 == 0;               // returns bool

// Event: restricted delegate (only += / -= from outside)
public class Button
{
    public event EventHandler Clicked;
    protected virtual void OnClicked() => Clicked?.Invoke(this, EventArgs.Empty);
}
```

## Q10. Value Types vs Reference Types

| Feature | Value Type (struct, int, bool) | Reference Type (class, string) |
|---------|-------------------------------|-------------------------------|
| Stored in | Stack (usually) | Heap |
| Contains | Actual data | Reference to data |
| Assignment | Copies value | Copies reference |
| Nullable | Need `Nullable<T>` | Nullable by default |
| Default | 0 / false / etc. | null |

## Quick-Fire Concepts

**Stack vs Queue:**
- Stack = LIFO (Push/Pop) — undo operations, DFS
- Queue = FIFO (Enqueue/Dequeue) — BFS, task scheduling

**Sorting Complexities:**

| Algorithm | Best | Average | Worst | Space | Stable |
|-----------|------|---------|-------|-------|--------|
| Merge Sort | O(nlogn) | O(nlogn) | O(nlogn) | O(n) | Yes |
| Quick Sort | O(nlogn) | O(nlogn) | O(n²) | O(logn) | No |
| Heap Sort | O(nlogn) | O(nlogn) | O(nlogn) | O(1) | No |

C# `Array.Sort` uses IntroSort (QuickSort + HeapSort + InsertionSort hybrid).

**Marshalling:** Converting object memory representation for storage/transmission. Used in COM interop, P/Invoke.

**OSI Model:** Application (7) → Presentation → Session → Transport → Network → Data Link → Physical (1)
