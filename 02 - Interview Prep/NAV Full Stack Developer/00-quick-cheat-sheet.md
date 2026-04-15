# Quick Cheat Sheet — Read This Right Before the Interview

---

## Your 60-second Intro

> "Full-stack .NET engineer, 6 years. C#/.NET Core for high-throughput APIs, Angular/Vue.js for enterprise UIs.
> Led AWS Redshift migration at eGain (>30% perf gain). Built reusable NuGet service library at Energy Exemplar,
> adopted by multiple teams. JWT security hardening at IKS Health. Agile/Scrum across all roles.
> Looking to apply this in a high-stakes financial systems context at NAV."

---

## C# — Must Know

| Topic | One-liner |
|---|---|
| Abstract class vs Interface | Abstract: shared base + fields; Interface: pure contract, multiple inheritance |
| ref vs out | ref: pre-initialized, two-way; out: method assigns it, one-way |
| async/await | Non-blocking I/O; returns Task; never use .Result (deadlock) |
| DI lifetimes | Singleton (app), Scoped (request), Transient (each time) |
| Value vs Reference type | struct/int = stack; class/string = heap |
| sealed class | Cannot be subclassed (e.g. string) |
| LINQ | Querying IEnumerable/IQueryable with C# syntax |

---

## REST — Must Know

| Topic | One-liner |
|---|---|
| Stateless | Every request is self-contained, no server session |
| POST vs PUT | POST = create (not idempotent); PUT = replace (idempotent) |
| 401 vs 403 | 401 = not authenticated; 403 = authenticated but not authorized |
| JWT structure | header.payload.signature — claims in payload, verified by signature |
| CORS | Browser blocks cross-origin requests unless server allows via headers |

---

## SOLID — One Line Each

- **S** — One class = one reason to change
- **O** — Add new behavior via new classes, don't modify existing
- **L** — Subtypes must be substitutable for base types
- **I** — Don't force clients to implement interfaces they don't use
- **D** — Depend on abstractions, not concrete classes

---

## SQL — Must Know

| Topic | One-liner |
|---|---|
| INNER JOIN | Only matching rows |
| LEFT JOIN | All left rows + matching right (NULL if no match) |
| WHERE vs HAVING | WHERE = before grouping; HAVING = after GROUP BY |
| Clustered vs Non-clustered index | Clustered sorts data physically (1 per table); Non-clustered is separate (many) |
| ACID | Atomicity, Consistency, Isolation, Durability |
| CTE | Named temp result set, improves readability |
| Window functions | ROW_NUMBER, RANK, SUM OVER PARTITION — aggregate without collapsing rows |

---

## React — Key Concepts

| Topic | One-liner |
|---|---|
| Virtual DOM | In-memory DOM; React diffs and updates only changed nodes |
| useState | `[value, setter] = useState(initial)` — triggers re-render on change |
| useEffect | Side effects; `[]` = mount only, `[dep]` = on dep change |
| Props | Read-only, parent → child (events bubble up via callbacks) |
| Redux | Global store; Action → Reducer → New State; `useSelector`/`useDispatch` |
| key prop | Helps React identify list items; use stable unique IDs, not index |

---

## MongoDB — Quick Concepts

- Document database — JSON-like documents in collections (not rows in tables)
- Schema-less — flexible, each document can have different fields
- When to use over SQL: flexible schema, nested data, horizontal scale, high write throughput
- Aggregation pipeline: `$match → $group → $sort → $limit` (like SQL's WHERE+GROUP BY+ORDER BY)

---

## RabbitMQ — Quick Concepts

- Message broker for async communication between services
- Producer → Exchange → Queue → Consumer
- Exchange types: Direct (exact key), Fanout (broadcast), Topic (pattern)
- Reliability: ACK, durable queues, dead letter queues
- In .NET: `RabbitMQ.Client` or `MassTransit`

---

## Design Patterns — Name-Drop Ready

| Pattern | What it does | Your example |
|---|---|---|
| Repository | Abstracts data access | IOrderRepository at any project |
| Factory | Creates objects without exposing logic | `NotificationFactory.Create("email")` |
| Singleton | One instance app-wide | DI `AddSingleton` in .NET Core |
| Strategy | Interchangeable algorithms | `IExportStrategy` for CSV/Excel/PDF |
| Decorator | Adds behavior without changing class | Caching wrapper around a service |
| Observer | Notify subscribers on state change | C# events, MediatR notifications |

---

## Agile Quick-Fire

- **Sprint** — Fixed iteration (2 weeks)
- **Velocity** — Story points per sprint (for planning)
- **DoD** — Definition of Done (code + tests + review + deployed)
- **PO** — Owns backlog and priorities
- **Scrum Master** — Removes blockers, facilitates

---

## Bridge Statements (When Gaps Come Up)

**If asked about React specifically:**
> "I've built enterprise UIs with Vue.js and Angular, which share React's component model. I've been actively studying React and find the hooks API familiar — `useState` maps to Vue's `ref`, `useEffect` to `watch`/`onMounted`. I'd be productive quickly."

**If asked about MongoDB:**
> "I've worked extensively with SQL Server and understand relational data modeling well. I understand the key tradeoffs with document databases — when to embed vs reference, and when schema flexibility outweighs consistency guarantees."

**If asked about RabbitMQ:**
> "I haven't used RabbitMQ directly, but I understand message broker patterns — I've worked with async event flows using AWS Lambda and SNS/SQS patterns. The producer/consumer model and reliability guarantees like ACKs are familiar concepts."

---

## Numbers to Drop Naturally

- **6+ years** — full-stack .NET experience
- **>30%** — query performance improvement at eGain (AWS Redshift migration)
- **40%** — manual data entry reduction at IKS Health
- **25%** — defect leakage reduction via CI/CD at eGain
- **Multiple product teams** — adopted your NuGet library at Energy Exemplar
- **2300+ clients, $310B AUM** — NAV's scale (shows you researched them)
