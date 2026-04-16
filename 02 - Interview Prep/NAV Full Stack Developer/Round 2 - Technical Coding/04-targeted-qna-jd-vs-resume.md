# Targeted Q&A — Your Resume vs NAV JD

> Every question here is something NAV is *likely* to ask based on their JD requirements  
> crossed with *your specific experience*. Answers are written in your voice — rehearse these out loud.

---

## SECTION 1: "Tell Me About Yourself"

### Q: "Walk me through your background."

> **Your answer** (60 seconds, hits JD keywords):

"I'm a full-stack .NET engineer with just over 6 years of experience. My core strength is building API-first backend services in C#/.NET — I've shipped high-throughput integration APIs, designed reusable service libraries, and led platform modernisations involving SQL Server and AWS data stores.

At Energy Exemplar I designed an Import/Export NuGet library from scratch — it became a shared service pattern adopted by multiple product teams, which taught me a lot about clean interface contracts and design patterns that survive real use. Before that, at eGain I led the migration of our entire analytics platform from legacy SSAS to AWS Redshift — a full redesign of ETL pipelines and data-access layers that delivered over 30% query performance improvement.

I work confidently across the stack — Vue.js and Angular on the frontend — and I'm very much at home in Agile Scrum teams with CI/CD. I was drawn to NAV specifically because the Transfer Agency domain is a natural fit for the kind of data-intensive, integration-heavy backend work I enjoy, and I'm excited to apply that experience in a fund administration context."

---

## SECTION 2: Deep-Dive on Your Projects

### Q: "Tell me about the NuGet service library you built at Energy Exemplar. What design patterns did you use?"

> **Your answer:**

"The Import/Export subsystem for PLEXOS Cloud needed to transform PTI Raw files into structured SQLite databases. The challenge was that multiple product teams wanted to use the same core logic — one via a CLI tool, another via an Excel Add-In UI. So I designed it as a reusable NuGet library with a clean interface contract.

Design patterns I applied:
- **Facade pattern** — the NuGet library exposed a simple `IImportService` / `IExportService` interface, hiding the complexity of file parsing, validation, and SQLite writes behind a clean API
- **Strategy pattern** — different file formats had pluggable parsers, so I could add a new format by implementing an interface rather than modifying existing code — that's Open/Closed in practice
- **Builder pattern** — for constructing complex import configuration objects with many optional parameters
- **Dependency Injection** — all dependencies injected via constructor so each layer was independently testable

The result was that the CLI tool and the Excel Add-In both consumed the same library without duplication — exactly the reuse goal."

---

### Q: "Tell me about the Redshift migration at eGain. What were the biggest technical risks and how did you handle them?"

> **Your answer:**

"The migration from SSAS to AWS Redshift was a significant initiative. We had enterprise-scale analytics that multiple stakeholder teams depended on, so the biggest risks were: data integrity during migration, query regression, and zero-downtime for running reports.

How I handled each:
- **Data integrity** — I introduced automated validation scripts that compared row counts and aggregate totals between SSAS and Redshift at each pipeline stage. Nothing moved forward without passing those checks.
- **Query performance regression** — I rewrote the data-access layer to use Redshift-optimised SQL: distribution keys aligned to join columns, sort keys for range-heavy queries, and avoided functions on indexed columns. End result was >30% better query performance.
- **No downtime** — we ran both systems in parallel for a handover period. I used Apache NiFi to orchestrate the ETL pipelines, so the switchover was a config change rather than a hard cutover.

The lesson I took from it: technical risk management is really about making failure detectable early — automated validation beats hoping things work."

---

### Q: "You mention reducing defect leakage by 25% at eGain. How?"

> **Your answer:**

"It was a combination of practices rather than a single fix. I championed a few things in the team:
- Mandatory peer code reviews with a checklist — catching logic errors before merge
- Unit tests at the service layer as a condition of the Definition of Done — I used xUnit and mocked dependencies
- CI/CD via GitHub Actions so every PR ran the test suite automatically — no regressions slipped through silently
- SSRS report validation: I added a QA step where stakeholder-facing numbers were cross-verified against the source data before each release

The 25% reduction was measured against defect leakage to production over two quarters. It wasn't magic — it was just making quality a team habit rather than an afterthought."

---

### Q: "At IKS Health you engineered a JWT authentication module. Walk me through how JWT works and what security issues you fixed."

> **Your answer:**

"JWT is a stateless authentication mechanism — the server issues a signed token (header.payload.signature) that the client stores and sends with each request. The server validates the signature without needing a database lookup, which is great for scalability.

The security vulnerabilities I resolved were:
1. **Weak secret key** — the original implementation used a short, hardcoded secret. I rotated to a strong randomly-generated key stored in a secrets manager.
2. **Missing token expiry validation** — tokens were being accepted even after expiry. I added proper `exp` claim validation.
3. **No refresh token pattern** — long-lived access tokens were in use. I introduced short-lived access tokens (15 min) with a refresh token mechanism stored server-side, allowing revocation.
4. **Document service access control** — the document management module was checking authentication but not authorisation — i.e., authenticated users could access other users' documents. I added resource-based authorisation checks.

The Angular frontend was updated to handle token refresh transparently using an HTTP interceptor."

---

### Q: "You built a CSV/Excel bulk-upload integration at IKS Health. How did you design it to be 'generic'?"

> **Your answer:**

"The challenge was that different teams needed to upload different data shapes — patient records, scheduling data, operations data — and building a separate upload handler for each would have been unmaintainable.

I designed it generically using:
- **Generics in C#** — a `BulkUploadService<T>` where T is the domain entity, with a common interface `IBulkUploadable`
- **Strategy pattern** for mapping — each entity type had its own `IRowMapper<T>` implementation that defined how CSV columns mapped to entity properties
- **Template Method** for the pipeline — the base class defined the steps: validate header → parse rows → map to domain → validate domain rules → persist. Each step could be overridden but the skeleton was fixed.
- **FluentValidation** for per-entity business rules

The result was that adding a new data type was just: write the mapper and the validator — the upload pipeline, error reporting, and partial success handling came for free. That's the 40% reduction in manual effort they got — one tool, many data types."

---

## SECTION 3: JD-Specific Technical Questions

### Q: "The JD says 'Propose Product/Solution Architecture'. Walk me through how you'd approach architecting the Transfer Agency system's trade processing module."

> **Your answer:**

"I'd start by understanding the functional and non-functional requirements separately.

**Functional**: What are the trade lifecycle states? Buy, Sell, Dividend — what triggers state transitions? What external systems need to be notified? Custodians, regulators, investors?

**Non-functional**: What's the SLA? How many trades per day? Does it need to be auditable (yes, definitely in fund admin)? What are the consistency requirements — is eventual consistency acceptable or do we need strong consistency?

**Architecture I'd propose**:
- **Domain-driven design** with a Trade aggregate as the core — Trade has a state machine (Pending → Validated → Settled → Failed)
- **Repository + Unit of Work** for persistence — abstracts SQL Server, makes the domain testable
- **Strategy pattern** for validation rules — each trade type has its own validation strategy, pluggable
- **Command pattern / CQRS** — write commands go through a command pipeline (validate → persist → publish event), reads go through optimised read models
- **Message queue (RabbitMQ)** for downstream notification — when a trade settles, publish an event; downstream systems (email, reporting, custodian) subscribe independently
- **Audit trail** — append-only event log, never update-in-place for compliance

I'd present this as a C4 diagram — Context, Containers, Components — and highlight which decisions have trade-offs that need business input."

---

### Q: "The JD mentions UML design proficiency. What UML diagrams do you use and when?"

> **Your answer:**

"I use UML practically — the right diagram for the right conversation.

| Diagram | When I use it |
|---------|--------------|
| **Class diagram** | Designing domain model and relationships before coding — shows inheritance, composition, interfaces |
| **Sequence diagram** | Explaining an API interaction or async flow — especially useful for showing how services talk to each other |
| **Component/C4 diagram** | Proposing system architecture to stakeholders — context, containers, components |
| **Activity diagram** | Mapping out a business process or state machine (e.g., trade lifecycle) |
| **State machine diagram** | When an entity has complex lifecycle transitions |

At Energy Exemplar I used class diagrams when designing the NuGet library's interface contracts — it helped me see inheritance hierarchies before writing a line of code. At eGain I used sequence diagrams to document the Redshift ETL pipeline flow for the team.

**Relationships I'm precise about**: association vs aggregation vs composition — composition means the child can't exist without the parent (e.g., a `Trade` contains `TradeLines` — if the trade is deleted, the lines go too)."

---

### Q: "The JD mentions building prototypes. How do you approach prototyping a new technology?"

> **Your answer:**

"I follow a time-boxed spike approach — which is a formal Agile technique. I'd:

1. **Define the question** the prototype needs to answer — not 'let's play with RabbitMQ' but 'can RabbitMQ handle 10,000 messages/sec with durability guarantees on our infrastructure?'
2. **Timebox it** — typically 1-2 days, no more, or it becomes a project
3. **Build the simplest thing that answers the question** — not production code, no error handling, just enough to validate the hypothesis
4. **Document findings** — what worked, what didn't, what the production version would need
5. **Throw the prototype away** — spike code is not meant to survive

At eGain I did this with Apache NiFi before committing to it for ETL — built a small flow to verify it could handle our data volumes and integrate with Redshift. It saved us from discovering problems mid-project."

---

### Q: "How do you identify and address technical risks in a project?"

> **Your answer:**

"I categorise risks by likelihood × impact, then address them in order:

**Identification**: I look at integration points (third-party APIs, external data sources), new technologies the team hasn't used before, performance-critical paths, and anything with unclear requirements.

**Mitigation strategies**:
- **Spike/prototype** for technology risk — prove it works before committing architecture to it
- **Interface contracts** first — define `IService` before implementing, so even if the implementation changes, callers are protected
- **Feature flags** for deployment risk — ship code behind a flag, enable only in production when confident
- **Parallel running** for migration risk — what I did with SSAS → Redshift: both systems live simultaneously during cutover

At eGain, the Redshift migration's biggest technical risk was query incompatibility — SSAS MDX queries don't translate to SQL. I flagged this early in the sprint planning and we allocated buffer time for query rewriting rather than discovering it in the last sprint."

---

## SECTION 4: Design Pattern Questions (Tied to Your Work)

### Q: "You mentioned your NuGet library used clean interface contracts. How does that relate to SOLID?"

> **Your answer:**

"Every principle was at play in that library:

- **SRP**: `IImportService` handles only imports, `IExportService` handles only exports — each class had one reason to change
- **OCP**: New file formats were added by implementing a new `IFileParser` — no existing code changed
- **LSP**: All parser implementations were substitutable — the import pipeline didn't care which parser it got, they all honoured the contract
- **ISP**: I kept interfaces narrow — `IFileParser` only had `Parse()`, separate from `IFileValidator` which had `Validate()`. Library consumers only took the interface they needed.
- **DIP**: The import pipeline depended on `IFileParser`, not on `CsvFileParser` — swappable for SQLite, XML, or any future format

The NuGet library being adopted by multiple teams was validation that the contracts were right — they could consume it without knowing the implementation details."

---

### Q: "If you were adding RabbitMQ to the Trade Processing system, what design pattern would you use to structure the consumers?"

> **Your answer:**

"I'd use the **Observer pattern** at the application level, and the **Strategy pattern** for message handling.

The structure:
- RabbitMQ is the physical observer bus
- The message publisher (TradeProcessor) publishes to a Topic exchange — `trade.settled`, `trade.failed`, `trade.pending`
- Each consumer is a subscriber: `EmailNotificationConsumer`, `AuditLogConsumer`, `CustodianFeedConsumer` — they each implement `IMessageHandler<TradeSettledEvent>`
- A **Factory** resolves the right handler based on message type from the DI container
- For retry/failure: messages that fail go to a **Dead Letter Queue** after N retries — I'd configure that in the exchange binding, not in code

I've handled similar async event flows at eGain with AWS Lambda — Lambda subscribed to events and processed them independently, same conceptual pattern, different infrastructure."

---

### Q: "Walk me through the Repository pattern — would you use it at NAV?"

> **Your answer:**

"Yes, definitely. For a Transfer Agency system with entities like Trades, Investors, Funds, Holdings — the Repository pattern is the right choice because:

1. **Testability** — I can mock `ITradeRepository` in unit tests without needing a real SQL Server instance. At eGain and Energy Exemplar I always coded to interfaces, never to concrete data-access classes.
2. **Flexibility** — if NAV wanted to add a read replica for reporting queries, I could swap the read repository implementation without touching business logic.
3. **Encapsulation** — LINQ queries and SQL complexity stay inside the repository; the service layer never writes raw queries.

I'd pair it with Unit of Work for operations that touch multiple repositories in one transaction — for example, settling a trade updates `Holdings`, inserts a `Transaction` record, and updates `NAVHistory` — all in one `SaveChanges()` call.

In practice I'd use EF Core with named queries for the common paths, and drop to Dapper for performance-critical read queries where EF's query generation isn't optimal."

---

## SECTION 5: SQL Deep-Dive (Tied to Your Experience)

### Q: "You led the Redshift migration — what query optimisation techniques did you apply?"

> **Your answer:**

"Several, and they were Redshift-specific as well as general SQL principles:

**Redshift-specific**:
- **Distribution keys**: I aligned distribution keys on the join column between the largest fact table and its most-joined dimension — this minimises data shuffling across nodes
- **Sort keys**: Range-based queries (by date) benefited from compound sort keys on date columns — Redshift skips unsorted blocks entirely
- **Columnar storage**: Ensured we chose the right column compression encoding per data type — Redshift `ANALYZE COMPRESSION` was helpful

**General SQL**:
- **Non-sargable predicates**: Removed functions like `YEAR(date)` in WHERE clauses — rewrote as range predicates so indexes/sort keys could be used
- **Subquery → JOIN**: Replaced correlated subqueries in SELECT with aggregated JOINs — from O(n) executions to O(1)
- **EXISTS vs IN**: Used NOT EXISTS instead of NOT IN when the subquery could return NULLs
- **Covering queries**: Selected only needed columns rather than SELECT * — reduces I/O on Redshift's columnar store

The >30% improvement was a combination of all these — no single silver bullet."

---

### Q: "How did you validate data integrity during the Redshift migration? Show me the SQL mindset."

> **Your answer:**

"I built a validation framework with multiple levels:

**Level 1 — Row counts**: Simple but essential — every table in the source matched row-for-row in the destination after each pipeline run.

```sql
-- Source count vs destination count
SELECT 
    'Source' AS Source, COUNT(*) AS RowCount FROM source_table
UNION ALL
SELECT 
    'Destination', COUNT(*) FROM dest_table;
```

**Level 2 — Aggregate checksums**: Sum and average of key numeric columns by partition (e.g., by month) — any ETL bug that modified values would show up here.

**Level 3 — Null rate checks**: Columns that should never be null were verified — a bad transformation can silently nullify data.

**Level 4 — Referential integrity**: Foreign key relationships were validated manually since Redshift doesn't enforce FK constraints.

All of this ran automatically via Lambda-triggered Python scripts after each NiFi pipeline execution, and any mismatch blocked the pipeline from proceeding to the next stage. That's how we achieved zero data loss."

---

## SECTION 6: Gap Questions (React, MongoDB, RabbitMQ)

### Q: "The JD requires React experience. Your resume shows Vue.js and Angular. How do you see yourself bridging that?"

> **Your answer:**

"React and Vue share the same core mental model — component-based UI, unidirectional data flow, virtual DOM reconciliation. At Energy Exemplar I worked with Vue 3 using the Composition API, which is conceptually very close to React hooks — `ref()` maps to `useState`, `computed()` maps to `useMemo`, `watch()` maps to `useEffect`. The JSX syntax is different from Vue's templates, but the patterns are identical.

For state management: I've used Pinia (Vue's equivalent of Redux Toolkit) — the concepts of actions, state, and derived state map directly. And I have Angular experience too, so I understand component lifecycle, routing, and service injection patterns from a different angle.

I've already been working through React specifically since your first round, and the conceptual bridge has been very fast. I'm confident I'd be productive in a React codebase within a sprint."

---

### Q: "The JD mentions MongoDB. Your resume shows SQL Server and Redshift. How comfortable are you with NoSQL?"

> **Your answer:**

"I understand the data modelling principles deeply from the SQL side — which actually makes the contrast with MongoDB very clear.

MongoDB's document model trades the normalisation of relational databases for query performance by embedding related data. Where SQL Server would have `Investors`, `Holdings`, and `Funds` as separate tables joined at query time, MongoDB would embed holdings inside the investor document — no JOIN needed, one read gets everything for a dashboard.

The trade-off: embedding works when you always access data together; referencing (like a foreign key) works when the referenced document grows large or is shared. The aggregation pipeline is MongoDB's equivalent of GROUP BY + JOINs in SQL.

My Redshift experience is especially relevant here — Redshift is a columnar NoSQL-like store in terms of query patterns. I've thought about data access patterns and storage trade-offs at that level. I'd be comfortable designing schemas in MongoDB for the fund administration domain — investor-centric documents for reads, separate transaction collections for writes."

---

### Q: "You haven't worked with RabbitMQ directly. How would you get up to speed?"

> **Your answer:**

"Conceptually I'm already there — the async event-driven patterns I used with AWS Lambda at eGain are the same pattern, different infrastructure. Lambda subscribed to events, processed them independently, and failures went to DLQ equivalents. The producer/consumer decoupling is identical to RabbitMQ.

What I'd need to learn is the RabbitMQ-specific mechanics: exchange types (Direct for point-to-point, Fanout for broadcast, Topic for pattern routing), acknowledgement modes, dead letter queue configuration, and the .NET `RabbitMQ.Client` library.

I'd approach it exactly how I approached Apache NiFi at eGain — time-boxed spike: build a producer, build a consumer, configure a Topic exchange, verify ACK and DLQ behaviour. I'd expect to be comfortable in 2-3 days."

---

## SECTION 7: Soft Skills / JD Behavioural Questions

### Q: "The JD mentions 'ability to lead a team and mentoring'. Have you done this?"

> **Your answer:**

"Yes, in two ways:

At eGain, when I introduced the CI/CD practices and peer review process, I effectively led the quality improvement initiative — I wrote the code review checklist, ran the first few reviews to set the bar, and coached junior engineers through pull request feedback. The 25% defect leakage reduction was a team outcome but it required someone to drive the cultural change.

At Energy Exemplar, the NuGet library being adopted by multiple teams meant I became the de facto technical contact — I onboarded two other teams onto the library, wrote the API documentation, and fielded integration questions. That's informal technical mentoring at scale.

I enjoy it — explaining a design decision to a junior developer forces me to articulate *why*, which sharpens my own understanding."

---

### Q: "The JD mentions client engagement. Do you have experience working with non-technical stakeholders?"

> **Your answer:**

"At eGain I produced and customised SSRS reports for stakeholder-facing insights. 'Stakeholder-facing' meant finance and operations teams who cared about results, not implementation. I learned quickly to translate between 'the ETL pipeline had a null value in the aggregation key' and 'the February revenue number in the report was off by 3% — here's why and here's the fix.' 

During the Redshift migration I ran update sessions with the analytics team — non-technical users who used the reports daily. Managing their confidence during a major infrastructure change was as important as the technical work. I'd report in plain language: 'the new system is running in parallel, your reports are verified correct, cutover happens next Monday.'"

---

### Q: "Tell me about a time you identified a technical risk before it became a problem."

> **Your answer (STAR):**

"**Situation**: During the SSAS to Redshift migration at eGain, we were 3 sprints in when I was reviewing the query compatibility between SSAS MDX and Redshift SQL.

**Task**: I needed to assess whether our existing analytic queries would translate cleanly, and flag any that wouldn't.

**Action**: I created a mapping document of all 40+ queries the analytics team used. I tested each one against Redshift in our dev environment. About 12 had patterns that either didn't translate (SSAS-specific functions) or performed poorly (missing Redshift-specific optimisations like distribution keys). I flagged this in sprint planning with specific effort estimates for each rewrite and proposed buffering two extra sprints before the cutover deadline.

**Result**: The team approved the buffer. We rewrote all 12 queries during those sprints with proper Redshift optimisations — that's actually where the 30%+ performance improvement came from. If I hadn't flagged it, we'd have hit a wall in the final sprint with no time to fix it."

---

### Q: "How do you handle disagreement with a technical decision made by a senior architect?"

> **Your answer:**

"I approach it as a conversation, not a challenge. My default is to ask questions first — 'Help me understand the reasoning for X' — because often there's context I don't have.

If after understanding the reasoning I still disagree, I'll state my concern clearly and specifically — not 'I don't like this' but 'I'm concerned that approach X will cause Y under load condition Z, because of A.' I'll back it with data or a reference if I can.

At Energy Exemplar I disagreed with using synchronous calls for the import pipeline where the files could be large — I proposed an async queue-based approach instead. I put together a quick comparison document and we discussed it in the architecture review. The team agreed to go async. Even if they hadn't, I'd have documented my concern and moved forward — you can advocate strongly while still being a team player."

---

## SECTION 8: Architecture / System Design (JD-Specific)

### Q: "How would you design a NAV (Net Asset Value) calculation service?"

> **Your answer:**

"This is a core fund administration problem — let me think through it.

**What it needs to do**: Take total fund assets, subtract liabilities, divide by outstanding units to get NAV per unit. But at scale there are complicating factors: multiple share classes, different currencies, audit requirements.

**Architecture**:

```
Input Sources → NAV Calculation Engine → Output / Distribution
```

- **Input layer**: Market data feed (prices), fund holdings, corporate actions, expense accruals — all fed in via integration APIs or message queues
- **Calculation engine**: Stateless service — given a snapshot of inputs for a fund at a point in time, calculates NAV. Stateless means it's horizontally scalable and idempotent (same inputs = same output, always)
- **Strategy pattern**: Different fund types have different calculation rules — Equity fund vs Fixed Income vs Mixed. Each is a pluggable `INAVCalculationStrategy`
- **Audit trail**: Every calculation is persisted with its inputs and outputs — append-only, never updated. This is a regulatory requirement.
- **Persistence**: SQL Server for the NAV history (structured, relational, needs ACID). Historical NAV is time-series in nature — good candidate for a date-partitioned table.
- **Output layer**: Publish calculated NAV to downstream systems (investor portal, custodian, regulator feeds) via message queue — decoupled, each consumer processes at its own pace

**Design patterns used**: Strategy (calculation rules), Repository (persistence), Observer (downstream notification), Builder (building the calculation context object)."

---

### Q: "How would you approach building a REST API for an investor portal?"

> **Your answer:**

"Resource-oriented design first:

```
GET    /api/investors/{id}                    — investor profile
GET    /api/investors/{id}/holdings           — their fund holdings
GET    /api/investors/{id}/transactions       — transaction history
GET    /api/investors/{id}/portfolio-value    — calculated current value
POST   /api/transactions                      — place buy/sell order
GET    /api/funds/{id}/nav/history            — NAV history for a fund
```

**Key decisions**:
- **Versioning**: URI versioning (`/api/v1/`) for clear contract management — when we change breaking behaviour, v2 doesn't break existing clients
- **Auth**: JWT Bearer tokens — short-lived access tokens (15 min), refresh token pattern
- **Pagination**: Cursor-based for transaction history (which can be large and grows continually), offset-based for smaller lists
- **Async where needed**: Placing a trade is async — it goes to a processing queue, returns 202 Accepted with a location header to poll. Don't block the HTTP response on a potentially long operation.
- **Error responses**: RFC 7807 ProblemDetails — consistent structure, machine-readable
- **Rate limiting**: Investor-facing APIs need rate limiting to prevent abuse

I'd also add an API gateway layer — handles cross-cutting: auth validation, rate limiting, logging, routing — so the microservices behind it stay focused on business logic."

---

## SECTION 9: Curveball / Tricky Questions

### Q: "Your most recent role ended in March 2026 and you're interviewing now (April 2026). What have you been doing?"

> **Your answer:**

"I've been intentional about the next step rather than taking the first offer. I've been doing three things: deepening my understanding of design patterns and system architecture (which I knew was an area to sharpen), learning React specifically since fund admin tech stacks tend to use it, and researching NAV specifically to understand the Transfer Agency domain. The interview process here has been part of that — the Round 1 feedback on design patterns was valuable and I've been working on it directly."

---

### Q: "Your experience is mostly in energy and healthcare verticals. How does that prepare you for fund administration?"

> **Your answer:**

"The technical challenges are more similar than the domains suggest. Fund administration is fundamentally about:
- **Data integrity** at scale — same as my Redshift migration work
- **Integration with external systems** (custodians, regulators, market data) — same as my Redox API and AWS integration work at IKS
- **Audit trails and compliance** — healthcare HIPAA compliance is similarly strict to financial regulation
- **High-throughput event processing** — same as the async Lambda workflows at eGain

Domain knowledge about NAV calculation, trade lifecycle, and investor reporting I'll learn on the job. The engineering fundamentals — clean interfaces, testable services, reliable data pipelines — those transfer directly."

---

### Q: "What's the most complex design pattern you've used and why?"

> **Your answer:**

"The most complex to get *right* was the Decorator pattern in the NuGet library at Energy Exemplar.

The library needed to support logging and caching as cross-cutting concerns without embedding them in the core `IImportService` — different consumers needed different combinations (CLI tool: logging but no cache; Excel Add-In: both). 

I implemented stacked decorators — `CachingImportService` wraps `LoggingImportService` wraps the core `ImportService`, all implementing `IImportService`. The DI registration wired them in the right order.

The complexity was in the DI registration: in .NET, registering decorated types requires manual factory methods in `AddScoped` rather than simple type registration. And I had to be careful that each decorator only added its own concern and nothing else — keeping them single-responsibility while they were wrapping each other.

What I learned: Decorator is powerful but gets messy at 3+ levels. If I were doing it again I'd consider `Scrutor` (a .NET library specifically for decorator registration) to keep the DI setup clean."

---

## SECTION 10: Questions to Ask Them

At the end of Round 2, ask at least 2-3 of these:

1. **"The role mentions UML and design patterns proficiency — what patterns are most prevalent in your current Transfer Agency codebase?"**
   *(Shows you're already thinking about contributing, not just passing the interview)*

2. **"What's the biggest architectural challenge the team is working through right now?"**
   *(Opens a real conversation, shows interest in the actual work)*

3. **"How does the engineering team work with the fund operations team day-to-day? Is there a domain expert embedded with engineering or is it more ticket-driven?"**
   *(Signals that you understand domain-driven development)*

4. **"What does the CI/CD and testing infrastructure look like? I've been working with GitHub Actions and xUnit — is the team on something similar?"**
   *(Shows you care about engineering quality)*

5. **"What would success look like for someone in this role after the first 3 months?"**
   *(Shows you're already thinking about onboarding and delivery)*

---

## Quick Reference: Map Your Work to JD Keywords

| JD Requirement | Your Evidence |
|---------------|---------------|
| .NET / C# 3-6 years | 6+ years — Energy Exemplar, eGain, IKS Health |
| REST API | High-throughput integration APIs at eGain, Redox API at IKS |
| SQL Server | Redshift migration (deep SQL optimisation), SSRS reports |
| Design patterns | NuGet library: Facade, Strategy, Builder, DI; IKS: Template Method, Strategy |
| Agile / Scrum | All 3 roles — CI/CD, peer reviews, sprint ceremonies |
| React (gap) | Vue.js + Angular — same component model, bridgeable in 1 sprint |
| MongoDB (gap) | Deep SQL/Redshift understanding, understand NoSQL trade-offs conceptually |
| RabbitMQ (gap) | AWS Lambda async event patterns — same conceptual model |
| UML | Use class, sequence, component, state diagrams — practical not theoretical |
| Architecture proposals | Led Redshift migration architecture; designed NuGet library contract |
| Prototyping | Apache NiFi spike at eGain; explicit Agile spike methodology |
| Client engagement | SSRS stakeholder reports; migration update sessions with analytics team |
| Team lead / mentoring | CI/CD adoption leadership at eGain; NuGet library onboarding for teams |
| Technical risk identification | Flagged MDX→SQL incompatibility 3 sprints before deadline |
