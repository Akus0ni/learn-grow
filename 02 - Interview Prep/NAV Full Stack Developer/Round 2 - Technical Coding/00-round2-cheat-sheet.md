# Round 2 — Last-Minute Cheat Sheet

> Read this 30 minutes before the interview. Do NOT try to learn new things now.

---

## Mental State Checklist
- [ ] You have 6+ years of .NET — you've used these patterns in real projects
- [ ] Talk out loud — they want to see HOW you think
- [ ] If stuck: restate the problem, think out loud, pseudocode first
- [ ] It's OK to say "I'd look this up in production" — but show you know the concept

---

## Design Patterns — 10-Second Recall

| Pattern | One sentence | Trigger phrase |
|---------|-------------|----------------|
| **Repository** | Abstracts data layer behind interface | "testable", "swap DB", "mock" |
| **Unit of Work** | One SaveChanges for multiple repos | "atomicity", "transaction" |
| **Factory** | Creates objects without knowing exact type | "centralize creation", "switch on type" |
| **Strategy** | Swap algorithm at runtime | "different fee calc", "multiple ways to" |
| **Builder** | Fluent step-by-step construction | "many optional params", "complex object" |
| **Decorator** | Add behaviour by wrapping same interface | "logging", "caching without modifying" |
| **Singleton** | One instance — use `Lazy<T>` | "config", "cache", "thread-safe" |
| **Observer** | Publisher notifies many subscribers | "event", "notify on change" |

---

## SOLID — One Line Each

| | Principle | Violation to spot |
|--|-----------|-------------------|
| **S** | One class = one reason to change | God class doing DB + Email + Log |
| **O** | Open for extension, closed for modification | `switch` on type to add behaviour |
| **L** | Subclass can replace base class | Square extends Rectangle and breaks Area() |
| **I** | Small specific interfaces, not fat ones | `IWorker` with `Work()` and `Eat()` |
| **D** | Depend on abstractions, not concretions | `new SqlRepo()` inside service class |

---

## SQL Window Functions — Copy-Paste Mental Model

```sql
FUNCTION_NAME() OVER (
    PARTITION BY column    -- group within (like GROUP BY but keeps rows)
    ORDER BY column        -- order within the partition
    ROWS BETWEEN ...       -- optional window frame
)
```

| Function | Use |
|----------|-----|
| `ROW_NUMBER()` | Unique 1,2,3,4 (no ties) |
| `RANK()` | Ties share rank, gaps after: 1,2,2,4 |
| `DENSE_RANK()` | Ties share rank, no gaps: 1,2,2,3 |
| `LAG(col, n)` | Value from n rows before |
| `LEAD(col, n)` | Value from n rows ahead |
| `SUM(...) OVER` | Running total |
| `AVG(...) OVER (... ROWS BETWEEN 6 PRECEDING AND CURRENT ROW)` | Moving avg |

---

## CTE Template

```sql
WITH CTE_Name AS (
    SELECT ...
    FROM ...
    WHERE ...
),
Another_CTE AS (
    SELECT ... FROM CTE_Name
)
SELECT * FROM Another_CTE;
```

**Recursive CTE:**
```sql
WITH Recursive_CTE AS (
    SELECT ... -- anchor (base case)
    UNION ALL
    SELECT ... FROM Recursive_CTE WHERE ... -- stop condition
)
SELECT * FROM Recursive_CTE OPTION (MAXRECURSION 100);
```

---

## C# Interview Gotchas — Don't Trip Up

```csharp
// ✅ Null-safe: use ?. and ??
var name = customer?.Address?.City ?? "Unknown";

// ✅ Tuple swap (C# 7+)
(a, b) = (b, a);

// ✅ Range indexer (C# 8+)
var last = list[^1];
var slice = array[1..4];

// ✅ Null coalescing assignment (C# 8+)
list ??= new List<int>();

// ✅ Switch expression (C# 8+)
string result = shape switch
{
    Circle c => $"Circle r={c.Radius}",
    Rectangle r => $"Rect {r.Width}x{r.Height}",
    _ => "Unknown"
};

// ✅ Pattern matching
if (obj is string s && s.Length > 0)
    Console.WriteLine(s.ToUpper());

// ❌ NEVER block on async
var result = asyncMethod().Result;  // deadlock risk
await asyncMethod();                // ✅

// ❌ NOT IN with nulls — use NOT EXISTS instead
// ❌ Functions on indexed columns — use range predicates
```

---

## LINQ Quick-Fire

```csharp
// Grouping + projection
var grouped = list
    .GroupBy(x => x.Category)
    .Select(g => new { Category = g.Key, Count = g.Count(), Total = g.Sum(x => x.Amount) });

// Flattening
var flat = nested.SelectMany(x => x.Items);

// First with default
var item = list.FirstOrDefault(x => x.Id == id);

// Distinct by property (C# 6+)
var distinct = list.DistinctBy(x => x.FundId);

// Partition (take/skip)
var page = list.Skip((pageNum - 1) * pageSize).Take(pageSize);

// Aggregate (custom fold)
var total = list.Aggregate(0m, (sum, x) => sum + x.Amount);
```

---

## Stored Procedure Checklist

When writing any SP, always include:
1. `SET NOCOUNT ON` — performance
2. Input validation — check IDs exist, values in range
3. `BEGIN TRY / BEGIN CATCH` — error handling
4. `BEGIN TRANSACTION / COMMIT / ROLLBACK` — if modifying data
5. `SCOPE_IDENTITY()` not `@@IDENTITY` — for new IDs
6. Meaningful `RAISERROR` / `THROW` messages

---

## If They Ask You to Design Something

**Say this structure**:
```
1. "Let me make sure I understand the requirement..." [restate]
2. "I'll define the interface first..."
3. "For the implementation, I'll use [pattern] because..."
4. "I'll inject this via DI so it's testable..."
5. "Edge cases to handle: nulls, empty inputs, ..."
6. "If I had more time I'd add..."
```

---

## Numbers to Drop

- **6+ years** .NET experience
- **30%+ performance improvement** at Energy Exemplar (Redshift optimisation)
- **Multiple teams** collaborated with at IKS Health
- **Fund Administration** domain — you understand NAV, trades, investors, holdings

---

## Questions to Ask THEM

1. "What does the tech stack look like for the Transfer Agency system day-to-day?"
2. "What design patterns are most commonly used in your codebase?"
3. "How does the team approach testing — unit, integration, E2E?"
4. "What's the biggest technical challenge the team is currently working through?"
5. "How does engineering collaborate with the fund operations team?"

---

## Last 5 Minutes Before the Interview

1. Breathe — you know this material
2. Remember: Energy Exemplar = complex .NET systems, real performance work
3. They gave you feedback about design patterns = they WANT you to do well
4. Talk out loud even when thinking — silence looks uncertain, talking looks confident
5. It's fine to say "Let me think about that for a second"

**You've got this. Good luck! 🚀**
