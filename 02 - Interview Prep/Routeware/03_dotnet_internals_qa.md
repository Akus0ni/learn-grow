# R3 .NET Internals — Top 10 Questions with Pre-Written Answers

> R3 (Bangalore) will drill **fundamentals**. They want to see precise vocabulary, edge-case awareness, and the ability to explain *why*, not just *what*.
>
> **How to use this file:** read each answer aloud once. Then close the file and explain the same concept to an imaginary listener. If you stumble, re-read. Goal is fluency, not memorization.

---

## 1. Value Types vs Reference Types

**Question:** "Explain the difference between value types and reference types. Where are they stored?"

**Answer:**
> "Value types — `int`, `bool`, `struct`, `DateTime`, `enum` — hold their data **inline**. When you pass them to a method, you get a copy by default. They live where they're declared: on the stack if they're local variables, or inline inside the containing object on the heap if they're fields.
>
> Reference types — `class`, `string`, `array`, `delegate` — store a reference; the actual object lives on the heap. The reference itself can sit on the stack, but the data it points to is heap-allocated.
>
> The 'stack vs heap' rule is **a generalization, not a strict truth** — for example, a `struct` field inside a class instance lives on the heap with its parent. The accurate way to say it: value types are *copied by value*, reference types are *copied by reference*."

**Likely follow-up: "What's boxing?"**
> "Boxing wraps a value type in a heap-allocated object so it can be treated as `object` or an interface. It costs an allocation. Unboxing reverses it — and throws `InvalidCastException` if the types don't match exactly. Generics largely eliminated this for collections after .NET 2.0 — `List<int>` doesn't box; `ArrayList` did."

---

## 2. `struct` vs `class` vs `record`

**Question:** "When would you use a struct over a class? What's a record?"

**Answer:**
> "Use a **struct** for small, immutable, value-semantic data — coordinates, money, vectors. Rule of thumb: under 16 bytes, no inheritance needed, equality based on value. Beyond that, the copy cost outweighs the heap-allocation cost it would have saved.
>
> Use a **class** for everything else — anything with identity, anything large, anything that benefits from inheritance or polymorphism.
>
> A **record** (C# 9+) is a class — *or* a struct with `record struct` — that the compiler gives you value-based equality, a `ToString`, and a `with` expression for non-destructive mutation. Records are perfect for DTOs and domain values where two instances with the same data should be considered equal."

**Watch out for:**
- A `readonly struct` is the safer way to declare value types — it prevents accidental mutation through method calls that would otherwise create silent defensive copies.

---

## 3. Garbage Collection — Generations

**Question:** "Explain how .NET's garbage collector works."

**Answer:**
> ".NET uses a **generational, mark-and-compact** GC with three generations: 0, 1, and 2.
>
> - **Gen 0** — short-lived objects. Method-local allocations land here. Collected very frequently and cheaply.
> - **Gen 1** — survivors of one Gen 0 collection. A buffer between short-lived and long-lived.
> - **Gen 2** — long-lived objects. Static caches, app-wide singletons. Gen 2 collections are expensive — they scan the whole managed heap.
>
> Separately, the **Large Object Heap (LOH)** holds objects over 85KB. The LOH historically wasn't compacted (it is now opt-in), so it can fragment.
>
> The GC's core insight is the **generational hypothesis**: most objects die young. By collecting Gen 0 frequently and Gen 2 rarely, you spend cycles where they matter most."

**Likely follow-up: "What's a finalizer and when does it run?"**
> "A finalizer is a destructor — `~MyClass()`. The GC runs it on a separate finalizer thread *before* reclaiming the object. Finalizers add cost — finalizable objects survive one extra GC cycle. In modern .NET, you almost never write one. The right pattern is `IDisposable` + `using`. Finalizers are only justified for unmanaged resources where you can't trust callers to dispose."

**Likely follow-up: "Server vs Workstation GC?"**
> "Workstation GC is single-threaded, low-latency — good for desktop apps. Server GC uses one heap per CPU core and parallel collection threads — higher throughput for services. ASP.NET Core defaults to Server GC."

---

## 4. `async` / `await` — How Does It Work?

**Question:** "What does `async` / `await` actually compile to?"

**Answer:**
> "The C# compiler rewrites an `async` method into a **state machine** — effectively a struct that captures local variables and tracks which `await` you're currently suspended at. When you `await` an incomplete `Task`, the state machine registers a continuation and returns control to the caller. When the awaited task completes, the continuation resumes the state machine from where it left off.
>
> Key points:
> - `async` doesn't create threads. It frees the current thread to do other work.
> - The continuation may run on the same thread (in console apps with no sync context) or be marshaled back to a captured context (UI thread in WinForms/WPF, request context in ASP.NET classic).
> - In ASP.NET Core, there is no sync context — continuations run on thread-pool threads, which is why deadlocks from `.Result` are much rarer there."

**Likely follow-up: "Why is `.Result` or `.Wait()` dangerous?"**
> "In ASP.NET classic or WinForms, calling `.Result` blocks the current thread while waiting for the task. But the task's continuation needs to resume *on that same context* — which is now blocked. Classic deadlock. In ASP.NET Core there's no sync context, so this specific deadlock doesn't occur — but `.Result` is still bad because it blocks a thread-pool thread, hurting scalability. **Rule: never block on async code.**"

**Likely follow-up: "What's `ConfigureAwait(false)`?"**
> "It tells the awaiter: 'don't bother marshaling back to the original sync context — resume on whatever thread is convenient.' Useful in library code where you don't need UI thread affinity, and historically the fix for the `.Result` deadlock. In ASP.NET Core it's a no-op for the sync-context reason. I still use it in libraries for portability."

---

## 5. `Task.Run` vs `Task.Factory.StartNew` vs `new Thread()`

**Question:** "Difference between these?"

**Answer:**
> "- **`new Thread()`** — creates a dedicated OS thread. Expensive (~1MB stack). Use only when you need a long-running, dedicated thread that won't return to the pool — rare.
> - **`Task.Factory.StartNew()`** — older API, lots of options, easy to misuse. Notably, it doesn't unwrap async lambdas — `StartNew(async () => ...)` returns `Task<Task>`. Avoid in new code.
> - **`Task.Run()`** — the modern shortcut. Queues work to the thread pool, unwraps async lambdas correctly. Use this for offloading CPU-bound work to a background thread.
>
> Rule: for CPU-bound work, `Task.Run`. For I/O-bound work, *don't* use `Task.Run` — just `await` the I/O API directly; it's already async."

---

## 6. `IEnumerable` vs `ICollection` vs `IList` vs `IQueryable`

**Question:** "When would you use each?"

**Answer:**
> "- **`IEnumerable<T>`** — forward iteration, lazy. The leanest contract. Use as method *input* when you only need to iterate.
> - **`ICollection<T>`** — adds `Count`, `Add`, `Remove`, `Contains`. Use when you need size or mutation.
> - **`IList<T>`** — adds indexer and `IndexOf`. Use when callers need positional access.
> - **`IReadOnlyList<T>`** — indexer + `Count`, no mutation. Often the right *return* type — exposes the access pattern without leaking mutability.
> - **`IQueryable<T>`** — looks like `IEnumerable` but expression-tree based. The provider (EF Core, LINQ-to-SQL) translates the query to SQL. Filter and project at the database, not in memory.
>
> The classic mistake: calling `.ToList()` too early on an `IQueryable` — you pull every row into memory and then filter. Keep it as `IQueryable` until the final materialization."

---

## 7. `string` — Why Immutable? `StringBuilder`?

**Question:** "Why is string immutable? When do you need StringBuilder?"

**Answer:**
> "Strings are immutable for safety, thread-safety, and interning. Because they can't change, the runtime can share references safely and intern identical literals into a single instance.
>
> The cost: every `+` allocates a new string. In a tight loop, that's catastrophic — N concatenations is O(N²) in memory.
>
> `StringBuilder` is a mutable buffer that resizes geometrically. Use it when you're building a string in a loop. For a small, fixed number of concatenations the compiler already optimizes `+` into `String.Concat` and `StringBuilder` is overkill.
>
> Rule: in a `for` loop → `StringBuilder`. A handful of concatenations → just use `+` or `string.Format` or interpolation."

---

## 8. DI Lifetimes — Singleton / Scoped / Transient

**Question:** "Explain DI lifetimes. What's a captive dependency?"

**Answer:**
> "- **Transient** — new instance every time it's requested. Cheap, stateless services.
> - **Scoped** — one instance per scope. In ASP.NET Core, scope = HTTP request. Perfect for `DbContext` — one per request, disposed at the end.
> - **Singleton** — one instance for the app's lifetime. Caches, configuration, thread-safe utilities.
>
> The classic bug: **captive dependency**. If a **singleton** depends on a **scoped** service, the scoped service gets captured by the singleton and effectively becomes a singleton too — bypassing the lifetime contract.
>
> Concrete example: injecting `DbContext` (scoped) into a singleton service. The singleton holds onto the `DbContext` forever — across requests — and you get threading bugs and stale data. The container will actually warn you about this in development if `ValidateScopes` is on.
>
> Rule: a dependency's lifetime must be **equal to or longer than** its consumer's."

---

## 9. `IDisposable` and the Dispose Pattern

**Question:** "When do you implement `IDisposable`? What's the dispose pattern?"

**Answer:**
> "Implement `IDisposable` when your type owns an **unmanaged resource** (file handle, socket, DB connection) or owns other `IDisposable` types.
>
> The simple case — you only hold managed `IDisposable`s — is one method:
> ```csharp
> public void Dispose() {
>     _httpClient?.Dispose();
>     _stream?.Dispose();
> }
> ```
>
> The full **dispose pattern** is needed only when you hold unmanaged resources directly. It has a protected virtual `Dispose(bool disposing)` plus a finalizer as a safety net. The finalizer runs only if the caller forgot to dispose — it's a last-chance cleanup, not the happy path.
>
> Modern C# rarely needs the full pattern — wrap unmanaged resources in `SafeHandle` and you get correct cleanup for free.
>
> `IAsyncDisposable` (C# 8+) is for resources whose cleanup is itself async — flushing a network stream, for instance. Pair with `await using`."

---

## 10. SOLID — One Example Per Letter (from your work)

**Question:** "Walk me through SOLID with examples."

**Answer:**

> **S — Single Responsibility.**
> "In my NuGet library, the PTI parser, the SQLite writer, and the validation engine are separate classes. Each has one reason to change. If SQLite's API changes, only the writer cares; the parser is untouched."

> **O — Open / Closed.**
> "The library defines an `ITransformer` interface. Adding a new file format means writing a new `ITransformer` implementation — no edits to existing code. Open for extension, closed for modification."

> **L — Liskov Substitution.**
> "Every `ITransformer` honors the same contract — same exceptions on the same conditions, same null-handling. A caller can swap implementations without surprise. This is where Liskov violations usually bite — a subclass that throws on an operation the base supports breaks polymorphism."

> **I — Interface Segregation.**
> "I split a coarse `IFileService` into `IFileReader` and `IFileWriter`. Consumers that only read shouldn't be forced to depend on write methods they don't use — it bloats mocks and surfaces accidental misuse."

> **D — Dependency Inversion.**
> "The core domain layer depends on abstractions — `IRepository`, `IClock` — never on concrete `SqlRepository` or `DateTime.Now`. The composition root wires up the concretes. Makes unit testing trivial — inject a fake clock to test time-dependent logic."

**Why this lands well:** every example is from *your actual code*, not textbook. Interviewers can tell.

---

## Bonus — Likely Curveballs

### "What's `Span<T>` for?"
> "A stack-allocated view over contiguous memory — could be an array, a stack-allocated buffer, or unmanaged memory. Lets you slice and process buffers without allocating. Used heavily in high-perf code — JSON parsers, text processing. Constraint: can't be stored as a field on a class or used in async methods because it's `ref struct`. For those cases, `Memory<T>` is the heap-friendly cousin."

### "What's the difference between `const` and `readonly`?"
> "`const` is compile-time, baked into the calling assembly — so changing it requires recompiling everything that referenced it. `readonly` is runtime, set in the constructor — safer for values that might change between releases. Rule: use `const` only for true constants like `Pi`. Use `readonly` for everything else."

### "How does `using` work?"
> "It's syntactic sugar for `try { ... } finally { x.Dispose(); }`. C# 8 added the **using declaration** — `using var x = ...;` — which scopes to the end of the enclosing block rather than requiring braces. Cleaner code, same semantics."

### "Tell me about `yield return`."
> "Compiler-generated state machine that turns a method into a lazy iterator. Each `yield return` pauses execution and returns one value; the next `MoveNext()` resumes. Lets you process arbitrarily large or infinite sequences without materializing them. Pairs naturally with `IEnumerable<T>`."

---

## How to Recover If You Don't Know

If asked something you genuinely don't know — **don't bluff**.

> *"I haven't gone deep on that — but here's what I'd reason about: [give the framework you'd think with]."*

This is a senior-level recovery. Bluffing is a junior-level fail. Bangalore interviewers especially will respect calibrated honesty, then probe what you *do* know.
