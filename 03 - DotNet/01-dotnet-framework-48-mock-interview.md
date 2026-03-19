# .NET Framework 4.8 — Mock Interview Q&A

> Windows-only, enterprise legacy platform. Still widely deployed in banking, insurance, healthcare, and government.

---

## Core Concepts

---

**Q: What is the CLR and what does it do?**

The **Common Language Runtime (CLR)** is the virtual machine component of .NET Framework. It handles:
- **JIT compilation** — converts IL (Intermediate Language) to native machine code at runtime
- **Garbage collection** — automatic memory management via generational GC
- **Type safety** — enforces CTS (Common Type System) across languages
- **Exception handling** — structured SEH wrapping
- **Security** — Code Access Security (CAS), role-based security

> *"When you compile C# code, the compiler produces IL bytecode, not machine code. The CLR's JIT compiler converts that IL to native instructions the first time each method is called — that's why .NET apps can have a warm-up cost."*

---

**Q: What is the difference between the Stack and the Heap in .NET?**

| | Stack | Heap |
|---|---|---|
| **Stores** | Value types, method frames, local variables | Reference types, boxed values |
| **Allocation** | LIFO, automatic | Managed by GC |
| **Speed** | Very fast | Slower (GC overhead) |
| **Size** | Limited (~1MB default per thread) | Large (process memory) |

```csharp
int x = 5;               // x lives on the stack
string s = "hello";      // s (reference) on stack, string object on heap
Person p = new Person(); // p (reference) on stack, Person object on heap
```

---

**Q: Explain the difference between value types and reference types.**

- **Value types** (`int`, `double`, `bool`, `struct`, `enum`): stored inline on the stack or inside other objects; copying creates a full duplicate.
- **Reference types** (`class`, `string`, `array`, `delegate`): stored on the heap; variable holds a pointer; copying copies the reference, not the object.

```csharp
// Value type — copy behavior
int a = 10;
int b = a;
b = 20;
Console.WriteLine(a); // 10 — unchanged

// Reference type — shared reference
var list1 = new List<int> { 1, 2 };
var list2 = list1;
list2.Add(3);
Console.WriteLine(list1.Count); // 3 — both point to same object
```

**Boxing** is when a value type gets wrapped in an `object` on the heap — avoid in tight loops.

---

**Q: What is the difference between `==` and `.Equals()` in C#?**

- `==` for value types: compares values. For reference types: compares references by default (unless overloaded).
- `.Equals()` for reference types: compares values if overridden (e.g., `string`, `int`); otherwise compares references.
- `string` overloads both `==` and `.Equals()` to do value comparison.

```csharp
string a = new string("hello".ToCharArray());
string b = new string("hello".ToCharArray());

Console.WriteLine(a == b);          // true  — string overloads ==
Console.WriteLine(a.Equals(b));     // true
Console.WriteLine(object.ReferenceEquals(a, b)); // false — different objects
```

---

**Q: What is GAC (Global Assembly Cache)?**

The GAC is a machine-wide repository for .NET Framework assemblies meant to be shared across multiple applications (e.g., installed libraries, framework DLLs). Key points:
- Only strong-named assemblies (signed with a key pair) can go in the GAC
- Solved DLL Hell by allowing multiple versions of the same assembly side-by-side
- Not present in .NET Core — each app carries its own dependencies (no shared GAC)

---

**Q: What are AppDomains?**

An **AppDomain** is an isolation boundary within a .NET process. It provided fault isolation between components in the same process (e.g., ASP.NET used AppDomains to isolate web applications sharing an IIS worker process).

- .NET Core removed AppDomains — they don't exist in modern .NET
- In Framework, you could unload an AppDomain to reclaim memory from plugins

---

## ASP.NET (Web Forms & MVC)

---

**Q: What is the ASP.NET Web Forms page lifecycle?**

```
PreInit → Init → InitComplete → PreLoad → Load → Control Events
→ LoadComplete → PreRender → SaveViewState → Render → Unload
```

Key events:
- **Page_Load**: fires every request (check `IsPostBack` to avoid re-initializing)
- **ViewState**: persists control state between postbacks via a hidden field (`__VIEWSTATE`)
- **Postback**: form submission that triggers server-side event handlers

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        // Only runs on first load, not postbacks
        BindDropDown();
    }
}
```

---

**Q: What is ViewState and what are its drawbacks?**

ViewState is a base64-encoded hidden field that stores control values between postbacks.

**Drawbacks:**
- Increases page size (can be 100KB+ on complex pages)
- Sent back and forth on every request
- Not encrypted by default (sensitive data exposure risk)
- Not suitable for sensitive data unless encrypted

**Mitigation:** Disable ViewState on controls that don't need it (`EnableViewState="false"`).

---

**Q: What is the difference between `Response.Redirect` and `Server.Transfer`?**

| | `Response.Redirect` | `Server.Transfer` |
|---|---|---|
| **Mechanism** | Sends 302 to browser, browser makes new request | Server-side transfer, no round-trip |
| **URL change** | Yes — browser URL changes | No — URL stays the same |
| **Performance** | Two HTTP requests | One request |
| **Cross-server** | Can redirect anywhere | Same server only |

---

**Q: What is MVC pattern and how does ASP.NET MVC implement it?**

- **Model** — data and business logic
- **View** — Razor templates (.cshtml) — presentation only
- **Controller** — handles HTTP requests, calls model, returns view or data

```csharp
// Controller
public class ProductsController : Controller
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    public ActionResult Index()
    {
        var products = _service.GetAll();
        return View(products); // passes model to view
    }

    [HttpPost]
    public ActionResult Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _service.Create(model);
        return RedirectToAction("Index");
    }
}
```

---

**Q: What is the difference between `ActionResult` types in MVC?**

| Return Type | Use Case |
|---|---|
| `ViewResult` | Renders a Razor view |
| `PartialViewResult` | Renders a partial view (AJAX, reusable UI) |
| `JsonResult` | Returns JSON (old-style AJAX) |
| `RedirectResult` | HTTP 302 redirect to URL |
| `RedirectToRouteResult` | Redirect to named route/action |
| `HttpNotFoundResult` | 404 response |
| `FileResult` | Returns file download |

---

## Entity Framework (EF 6)

---

**Q: What is the difference between Code First, Database First, and Model First in EF6?**

| Approach | Starts From | Workflow |
|---|---|---|
| **Code First** | C# classes | Write models → EF generates DB via migrations |
| **Database First** | Existing DB | Scaffold models from DB schema |
| **Model First** | EDMX diagram | Design in designer → generates both DB and classes |

Code First is the modern default:

```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
}
```

---

**Q: What is lazy loading vs. eager loading in EF6?**

```csharp
// Lazy loading (default if virtual navigation property) — N+1 problem risk
var orders = db.Orders.ToList();
foreach (var o in orders)
    Console.WriteLine(o.Customer.Name); // each triggers a DB query

// Eager loading — single query with JOIN
var orders = db.Orders.Include(o => o.Customer).ToList();

// Explicit loading — load related data on demand
db.Entry(order).Reference(o => o.Customer).Load();
```

**N+1 Problem:** Lazy loading inside a loop = 1 query to get list + N queries for each related entity. Always prefer `.Include()` when you know you need related data.

---

**Q: What are EF6 migrations?**

Migrations track schema changes as versioned C# files that can be applied/rolled back:

```powershell
# Package Manager Console
Add-Migration AddOrderStatus      # creates migration file
Update-Database                   # applies to DB
Update-Database -TargetMigration 20240101_Initial  # roll back
```

---

## Threading & Async

---

**Q: What is the difference between `Thread`, `ThreadPool`, and `Task`?**

| | Thread | ThreadPool | Task |
|---|---|---|---|
| **Created** | Manually, dedicated OS thread | Reused from pool | Abstraction over ThreadPool |
| **Overhead** | High (~1MB stack) | Low | Low |
| **Use Case** | Long-running, background work | Short-lived work items | Async operations, composable |

```csharp
// Avoid: manual Thread for short work
var t = new Thread(() => DoWork());
t.Start();

// Prefer: Task for async/parallel work
var task = Task.Run(() => DoWork());
await task;
```

---

**Q: What does `async/await` do under the hood in .NET Framework?**

`async/await` is compiler sugar. The compiler transforms an `async` method into a state machine. `await` doesn't block the thread — it registers a continuation and returns the thread to the pool while the awaited operation completes.

```csharp
// What you write
public async Task<string> GetDataAsync()
{
    var result = await httpClient.GetStringAsync("https://api.example.com");
    return result.ToUpper();
}

// What the compiler generates (simplified)
// A state machine class with MoveNext() that handles the continuation
```

**Common mistake:** `async void` — fire-and-forget, exceptions are unobservable. Only use for event handlers. Always use `async Task`.

---

**Q: What is a deadlock in async code and how do you avoid it?**

Classic deadlock pattern in ASP.NET Framework (synchronization context):

```csharp
// DEADLOCK: calling .Result/.Wait() on async in ASP.NET Framework
public ActionResult BadAction()
{
    var data = GetDataAsync().Result; // blocks thread, holds sync context
    return View(data);               // continuation can never resume
}

// FIX: go async all the way
public async Task<ActionResult> GoodAction()
{
    var data = await GetDataAsync();
    return View(data);
}

// Or: use ConfigureAwait(false) in library code
var data = await GetDataAsync().ConfigureAwait(false);
```

---

## Memory & Garbage Collection

---

**Q: Explain the .NET GC generations.**

The GC uses a generational model to optimize collection:

| Generation | Contents | Collected |
|---|---|---|
| **Gen 0** | Newly allocated short-lived objects | Most frequently |
| **Gen 1** | Objects that survived Gen 0 | Buffer between Gen 0 and 2 |
| **Gen 2** | Long-lived objects (static, cached) | Rarely — expensive full GC |
| **LOH** | Objects ≥ 85KB | With Gen 2, not compacted by default |

**Rule:** Keep Gen 2 and LOH small. Many large allocations → frequent full GCs → pauses.

---

**Q: What is `IDisposable` and the `using` statement?**

`IDisposable` allows deterministic release of unmanaged resources (file handles, DB connections, network sockets) before GC runs.

```csharp
public class FileProcessor : IDisposable
{
    private FileStream _stream;
    private bool _disposed;

    public FileProcessor(string path)
    {
        _stream = new FileStream(path, FileMode.Open);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stream?.Dispose();
            _disposed = true;
        }
    }
}

// using ensures Dispose is called even if an exception occurs
using (var processor = new FileProcessor("data.txt"))
{
    processor.Process();
} // Dispose called here
```

---

## Security

---

**Q: What is Code Access Security (CAS)?**

CAS is a .NET Framework mechanism that restricts what code can do based on where it came from (zone, URL, publisher). It allowed partial-trust hosting (e.g., running untrusted plugins in restricted AppDomains).

- Largely deprecated — complex and rarely used correctly
- .NET Core removed it entirely
- Modern security relies on OS-level isolation and proper authentication/authorization

---

**Q: How do you prevent SQL injection in .NET Framework?**

```csharp
// VULNERABLE — never do this
var query = "SELECT * FROM Users WHERE Username = '" + username + "'";

// SAFE — parameterized query
using (var conn = new SqlConnection(connectionString))
using (var cmd = new SqlCommand("SELECT * FROM Users WHERE Username = @username", conn))
{
    cmd.Parameters.AddWithValue("@username", username);
    conn.Open();
    var reader = cmd.ExecuteReader();
}

// SAFE — with EF6 (parameterized automatically)
var user = db.Users.Where(u => u.Username == username).FirstOrDefault();
```

---

## Common Interview Scenarios

---

**Q: Your .NET Framework 4.8 app has high memory usage. How do you investigate?**

1. **Profile with dotMemory or PerfView** — identify what's on the heap
2. **Check Gen 2 and LOH** — large objects or long-lived caches?
3. **Look for event handler leaks** — subscribing to events without unsubscribing keeps objects alive
4. **Check for missing `Dispose` calls** — undisposed `SqlConnection`, `HttpClient` instances
5. **Review static collections** — static `Dictionary`/`List` that grows unbounded

```csharp
// Common leak: event subscription without unsubscribe
publisher.SomeEvent += subscriber.Handler; // subscriber held alive by publisher
// Fix:
publisher.SomeEvent -= subscriber.Handler; // or implement IDisposable to do this
```

---

**Q: What is the difference between `.NET Framework 4.8` and `.NET Core`?**

| | .NET Framework 4.8 | .NET Core / .NET 8 |
|---|---|---|
| **Platform** | Windows only | Cross-platform |
| **Deployment** | Machine-wide install | Self-contained or framework-dependent |
| **Web** | ASP.NET (System.Web) | ASP.NET Core |
| **Performance** | Slower, heavier | Significantly faster |
| **Containers** | Limited (Windows containers) | First-class Docker support |
| **Future** | Maintenance mode only | Active development |
| **GAC** | Yes | No |
| **WCF / WinForms** | Full support | Partial (WinForms/WPF ported, WCF limited) |

---

**Q: When would you choose .NET Framework 4.8 over .NET 8?**

- Application heavily uses **WCF** (Windows Communication Foundation) — WCF server is not in .NET Core
- Deep **COM interop** dependencies
- Legacy third-party libraries with no .NET Standard equivalent
- Enterprise policy mandates Windows + IIS deployment with full Framework installed
- Application uses **WebForms** and migration budget doesn't exist

> *"In practice, if you have the choice, you'd always target .NET 8 for new work. But in enterprise environments, you often inherit Framework apps where migration risk outweighs the benefits for now."*
