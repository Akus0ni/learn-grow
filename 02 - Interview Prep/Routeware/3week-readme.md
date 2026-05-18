# Routeware — Senior .NET Developer Interview Prep

**Role:** Senior .NET Developer (Christine)
**Rounds:** 3 — AI screen → US team member → Bangalore team member
**Your level:** 6+ yrs (JD asks 7+, you're close — frame depth over years)

---

## 1. JD ↔ Resume Mapping (know this cold)

| JD requirement | Your evidence | Confidence | Action |
|---|---|---|---|
| 7+ yrs .NET / C# | 6+ yrs across IKS, eGain, EE | Medium | Lead with depth (NuGet design, perf, security) |
| Microservices + event-driven + messaging | Lambda async flows, integration APIs | **Weak** | Prep: SQS/SNS, Kafka concepts, Saga, Outbox |
| OOP + SOLID + system design | Implicit in NuGet library design | Medium | Prep crisp examples per SOLID letter from your work |
| OAuth/OIDC/JWT + error handling | **JWT hardening at IKS** | **Strong** | Make this your hero story |
| SQL/NoSQL + perf tuning | Redshift migration, >30% query gain | **Strong** | Prep: indexes, execution plans, N+1 |
| Automated testing / testable arch | xUnit, validation scripts | Medium | Prep: DI, mocking, integration vs unit, TestContainers |
| CI/CD + source control | GitHub Actions, peer review culture | Medium | Prep: branching strategy, blue/green, canary |
| Lead initiatives end-to-end | Import/Export subsystem ownership | **Strong** | Use as architect-judgment story |
| **VB.NET + WinForms** (preferred) | None | **Gap** | Read 1 primer — say "willing to ramp, comfortable with legacy" |
| Cloud (Azure/AWS/GCP) | AWS Lambda/EC2/Redshift/Secrets Mgr | **Strong** | — |
| Observability (logging/metrics/tracing) | Validation scripts only | **Weak** | Prep: Serilog, OpenTelemetry, correlation IDs |
| Docker/K8s | None on resume | **Gap** | Prep concepts: image vs container, pods, services, HPA |
| Perf optimization (backend/distributed) | JSON serialization, Redshift perf | Medium | Prep: async/await pitfalls, GC, span/memory |
| React + TS (preferred) | Vue + Angular, TS | Medium | Frame: "same mental model, can ramp in days" |

**Top 3 gaps to close:** event-driven messaging, observability stack, Docker/K8s vocabulary.
**Top 3 strengths to lead with:** JWT auth hardening, SSAS→Redshift modernization, NuGet service library ownership.

---

## 2. Round-by-Round Strategy

### Round 1 — AI Screen
**Format:** likely a structured voice/chat bot or coding pad (HackerRank/CodeSignal-style). Designed for signal extraction, not depth.

**What to expect:**
- Behavioral prompts ("tell me about a time…") with strict time caps
- 1–2 coding problems: easy/medium DS&A in C# (string, array, hashmap, LINQ-able)
- Possibly a "design a small API" prompt
- MCQs on C#/.NET fundamentals

**Tactics:**
- **Speak in STAR.** Keep each story to 90 seconds. AI graders favor concise, structured answers with concrete numbers.
- **Always name a metric** — ">30% query perf", "40% manual effort cut", "25% defect leakage drop". You have these — use them.
- **Coding:** narrate intent first, then write. State complexity (Big-O) before you finish. Use idiomatic C# (`Dictionary`, `HashSet`, LINQ where readable, `StringBuilder`).
- **No long preambles.** Get to the point in sentence one.

**Practice list (week of):**
- 10 LeetCode Easy + 5 Medium in C# (Two Sum, Group Anagrams, LRU Cache, Valid Parentheses, Merge Intervals, Top K Frequent, Longest Substring w/o Repeating, Word Ladder is overkill — skip)
- Re-record your top 5 STAR stories aloud — time them
- Skim C# fundamentals MCQ banks (value vs ref, boxing, `IEnumerable` vs `IQueryable`, `async`/`await`, `Task` vs `Thread`)

### Round 2 — US Team Member
**Format:** technical + cultural fit. Americans typically front-load rapport, value clear communication, and probe **ownership + judgment**. Expect deeper system-design and architectural-trade-off discussion.

**What to expect:**
- 10 min rapport + your intro (≤2 min, structured)
- 30–40 min technical: system design, API design, deep .NET, possibly live code
- 10 min reverse questions

**Design prompts likely:**
- "Design a service for X" — pick from: rate limiter, URL shortener, **file ingestion pipeline** (your Import/Export fits!), notification service, audit log
- "How would you modernize a legacy WinForms app?" — JD signals this is real work
- "Walk me through your Import/Export NuGet library architecture"

**Tactics:**
- **Drive the conversation.** US interviewers respect candidates who ask clarifying questions, state assumptions, and propose 2 options with trade-offs.
- **Use a frame:** Requirements → constraints → API contract → data model → scale/perf → failure modes → observability → testing → rollout. Hit each in 1–2 lines unless probed.
- **Anchor on your work.** Every abstract question can land on PLEXOS / eGain Redshift / IKS JWT — make it concrete.
- **Be honest about gaps.** "I haven't worked with VB.NET directly, but here's how I'd approach learning a legacy codebase…" — Americans value calibrated confidence over bluffing.

### Round 3 — Bangalore Team Member
**Format:** deep technical, code-heavy. Indian engineering rounds tend to drill **fundamentals** harder — expect "why" questions, edge cases, and live debugging.

**What to expect:**
- Deeper C#/.NET internals (GC, threading, memory)
- SQL query writing on a whiteboard/screen-share
- Possibly a live debugging or refactoring exercise
- LINQ + EF Core gotchas (deferred execution, tracking, N+1)
- Async/await pitfalls (deadlocks, `ConfigureAwait`, `.Result`)

**Tactics:**
- **Show working memory.** Talk through your reasoning, including dead ends. They want to see how you think under pressure.
- **Be precise.** "Reference type" not "object thing." "Heap allocation" not "stored somewhere."
- **Defend your resume.** Be ready for: "Walk me through exactly how your JWT module worked — what algorithm, where keys lived, refresh flow, revocation?"

---

## 3. Hero Stories (memorize these — 90s each)

### Story A — JWT Auth Hardening (security, ownership)
- **S:** Scribble WFM at IKS had session vulnerabilities — document-management endpoints accessible with stale/forged tokens.
- **T:** Re-architect auth so HIPAA-adjacent data was access-controlled end-to-end.
- **A:** Designed JWT module in Angular/TS frontend + ASP.NET backend. Short-lived access tokens + refresh flow. Rotated signing keys. Audited every doc-mgmt endpoint for claim validation. Added role-based guards on the frontend, server-side claim re-validation (defense in depth).
- **R:** Closed the critical vuln, passed internal security audit, integrated cleanly with Redox EHR exchange.
- **Use when:** asked about security, OAuth/OIDC/JWT, ownership of a hard problem, working with constraints (HIPAA).

### Story B — SSAS → AWS Redshift Modernization (system design, modernization)
- **S:** eGain analytics on legacy SSAS — slow queries, brittle ETL, scaling ceiling.
- **T:** Modernize without data loss while keeping stakeholders' SSRS reports working.
- **A:** Designed new Redshift schema (sort/dist keys), rebuilt ETL on Apache NiFi, refactored DAL to repository pattern with read replicas. Lambda + EC2 for orchestration, Secrets Manager for creds. Wrote validation scripts comparing old vs new row counts and aggregates per pipeline run.
- **R:** >30% query perf gain, zero data loss in cutover, stakeholders saw no SSRS regression.
- **Use when:** asked about modernization (exactly what this JD wants), system design, SQL perf, cloud migration.

### Story C — Import/Export NuGet Library + CLI (technical leadership, API design)
- **S:** PLEXOS Cloud needed PTI Raw file ingestion; multiple product teams were duplicating logic.
- **T:** Build one canonical service pattern teams could consume — clean interface, no per-team forking.
- **A:** Designed a NuGet library with a clean public contract (interfaces for ingestion, transformation, persistence). Layered architecture — domain core independent of SQLite/Excel. Built CLI on top using same library, then Excel Add-In (Vue) — proving the contract worked across surfaces.
- **R:** Adopted by multiple teams, established the "shared service" pattern internally.
- **Use when:** asked about API/library design, SOLID, leadership, mentoring (you set a pattern others followed).

### Story D — JSON Serialization Perf (deep .NET)
- **S:** Integration APIs at eGain handling enterprise data exchange, JSON payloads were a hotspot.
- **T:** Reduce CPU + memory on serialization without breaking compatibility.
- **A:** Profiled with dotMemory/dotTrace, moved from Newtonsoft to `System.Text.Json` where safe, used source generators for hot paths, switched to `Utf8JsonWriter` for streaming.
- **R:** Throughput improved, GC pressure dropped.
- **Use when:** asked about perf, .NET internals, profiling. *(If you don't have exact numbers, say "meaningful reduction" — don't invent.)*

---

## 4. Technical Topics — Study Order (3-week plan)

### Week 1 — Close the gaps
1. **Event-driven / messaging** — at-least-once vs exactly-once, Outbox pattern, Saga (choreography vs orchestration), idempotency keys, dead-letter queues. Pick one stack to speak fluently: AWS SQS+SNS (matches your AWS exp) or Kafka.
2. **Microservices** — bounded contexts, API gateway, service discovery, circuit breaker (Polly), bulkhead, retry w/ exponential backoff + jitter.
3. **Observability** — structured logging (Serilog + Seq/ELK), metrics (Prometheus), tracing (OpenTelemetry), correlation IDs across services. Know "the three pillars."
4. **Docker/K8s vocabulary** — image vs container vs pod, deployment vs statefulset, service vs ingress, HPA, secrets, configmap. You don't need to write YAML — you need to discuss.

### Week 2 — Sharpen strengths
1. **OOP + SOLID** — one example per letter, from YOUR code (NuGet library is gold for this).
2. **System design templates** — practice 5 designs out loud (rate limiter, file ingestion, notification svc, URL shortener, audit log).
3. **API design** — REST maturity (Richardson), versioning strategies, idempotency, pagination, error envelopes, OpenAPI.
4. **SQL deep dive** — indexes (clustered vs non-clustered, covering), execution plans, query hints, deadlocks, isolation levels, EF Core gotchas (tracking, N+1, projection vs materialization).
5. **OAuth/OIDC/JWT** — auth code flow + PKCE, refresh tokens, token introspection vs validation, key rotation, JWE vs JWS.

### Week 3 — .NET internals + practice
1. **C# / .NET internals** — value vs ref types, struct vs class, boxing, GC generations, `Span<T>`/`Memory<T>`, `IDisposable`/`using`/`IAsyncDisposable`.
2. **Async/await** — sync context, deadlocks (`.Result`/`.Wait`), `ConfigureAwait(false)`, `Task.WhenAll` vs sequential awaits, cancellation tokens.
3. **Testing** — xUnit fixtures, `IClassFixture`, integration tests w/ `WebApplicationFactory`, TestContainers for DB, mocking with Moq/NSubstitute.
4. **CI/CD** — trunk vs gitflow, semantic versioning, blue/green, canary, feature flags.
5. **Mock interviews** — 2 system-design + 2 behavioral, recorded.

---

## 5. High-Probability Question Bank

### C# / .NET
- `IEnumerable` vs `ICollection` vs `IList` vs `IQueryable` — when each?
- What does `async` actually compile to? State machine + `Task`.
- Why is `async void` dangerous? (only event handlers)
- Difference between `Task.Run`, `Task.Factory.StartNew`, `new Thread()`.
- How does GC decide what to collect? Gen 0/1/2, LOH, server vs workstation GC.
- `string` vs `StringBuilder` — when does the JIT optimize concat?
- Dispose pattern + when finalizers are needed (almost never in modern code).
- `record` vs `class` vs `struct` vs `readonly struct`.
- Dependency injection lifetimes — singleton/scoped/transient pitfalls (captive dependency).

### System Design
- Design a file-ingestion pipeline (your wheelhouse — Import/Export).
- Design a rate limiter — fixed window, sliding window, token bucket, Redis-backed.
- How would you modernize a 10-year-old WinForms app? (strangler fig pattern — say this phrase)
- Multi-tenant API — how do you isolate data? (DB-per-tenant vs shared with discriminator)
- Idempotent POST — how?

### SQL
- Write a query: top N per group, find duplicates, gaps & islands.
- When does an index hurt performance? (writes, low cardinality, fragmentation)
- Difference between `WHERE` and `HAVING`.
- Read isolation levels — what's a phantom read? Snapshot vs read committed.
- Why is `SELECT *` bad?

### Security
- How does OAuth 2.0 auth code flow + PKCE work? Draw it.
- What's in a JWT? (header.payload.signature) Why is the payload not encrypted by default?
- Refresh token rotation — what does it solve?
- HTTPS — TLS handshake at a high level. Why mutual TLS for service-to-service?

### Behavioral (US round will weight these heavily)
- Tell me about a time you disagreed with a teammate / manager.
- Tell me about a production incident you caused or fixed.
- How do you decide between rewriting vs refactoring?
- How do you mentor someone less experienced?
- What's a technical decision you regret?

---

## 6. Reverse Questions (have 3 ready per round)

**For US team member:**
- What does the modernization roadmap look like for the WinForms/VB.NET footprint? Strangler fig, or full rewrite?
- How does the team balance new feature work against tech debt reduction?
- What does "good" look like 6 months in for this role?

**For Bangalore team member:**
- How is the work split between US and India teams — handoff or parallel ownership?
- What's the typical sprint cadence and on-call expectation?
- What's the most painful part of the codebase today?

**For both:**
- What's the testing culture — coverage targets, integration vs unit ratio?
- How do release decisions get made — release train, continuous, gated?

---

## 7. Logistics & Mental Prep

- **AI round:** quiet room, wired headset, second device for backup, water nearby. Test camera/mic 30 min before.
- **Time-zone math:** US interviewer likely PT/ET — confirm AM/PM. Bangalore interviewer is IST = your TZ.
- **Speak slower than feels natural** on the US call — accents + network = misunderstandings.
- **Have your resume open in a tab** during all rounds. Have the JD open too.
- **End every round with one specific question** that shows you read the JD ("Saw VB.NET/WinForms mentioned — how much of the daily work touches that?").

---

## 8. Daily Checklist (last 3 days)

- [ ] Re-read your 4 hero stories aloud, time each at 90s
- [ ] Re-read JD, circle 3 phrases to echo back during interview
- [ ] One LeetCode medium in C#, narrated aloud
- [ ] Draw one system design from memory on paper
- [ ] Sleep 8h the night before — non-negotiable

---

## Quick links to your existing prep
- `../Qentelli/` — .NET + AWS deep dive, reuse system-design notes
- `../NAV/` — round 2 technical coding strategy
- `../Siemens/` — coming up 2026-03-26, overlapping .NET prep
