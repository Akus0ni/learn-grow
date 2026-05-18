# Routeware — Senior .NET Developer Interview Prep

**Role:** Senior .NET Developer (Christine)
**Rounds:** 3 — AI screen → US team member → Bangalore team member
**Your level:** 6+ yrs (JD asks 7+, you're close — frame depth over years)

## Files in this folder
- **`README.md`** (this file) — JD↔resume map, round strategies, hero stories, sprint plan, question bank
- **`01_intro_script.md`** — 2-minute self-intro tailored per round
- **`02_system_design_prompts.md`** — 3 high-probability R2 design prompts with full answers
- **`03_dotnet_internals_qa.md`** — Top 10 R3 .NET internals questions with pre-written answers

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

## 4. Compressed Sprint Plan (Fri 2026-05-15 → Fri 2026-05-22)

**Reality check:** Round 1 is this weekend, Rounds 2–3 are Mon–Fri next week. You have **~36 hours to R1** and **~5 working days to R2/R3**. Cut depth, keep breadth. Goal: be *credible* on everything in the JD and *strong* on your hero stories.

**Operating rules for the sprint:**
- **Do not learn new frameworks.** No hands-on Docker/K8s/Kafka. Read enough to discuss intelligently — that's it.
- **Speak everything aloud.** Stories, designs, even study notes. Interviews are speaking exercises, not reading ones.
- **One pomodoro per topic max.** If you can't summarize it in 25 min, you can't use it in an interview anyway.
- **Stop at 10pm each night.** Sleep > one more topic.

---

### **Friday evening (TODAY, 2026-05-15) — 2 hrs**
Goal: lock in hero stories + know what's on your resume cold.

- [ ] (45 min) Read your 4 hero stories aloud 3× each. Time them — must hit 90s. Write down the *exact* numbers you'll quote (>30%, 40%, 25%).
- [ ] (30 min) Re-read your own resume line by line. For every bullet, ask: "if they probe this, what's my 2-sentence drill-down?"
- [ ] (30 min) Re-read JD. Circle the 5 phrases you'll echo back: *modernization, microservices, OAuth/JWT, observability, technical leadership*.
- [ ] (15 min) Lay out gear for AI round: wired headset, water, second device for backup, charger in.

---

### **Saturday 2026-05-16 — AI round prep day (6–8 hrs split into blocks)**

**Morning (3 hrs) — coding warm-up**
- [ ] 5 LeetCode Easy in C#, narrated aloud (Two Sum, Valid Parentheses, Merge Two Sorted Lists, Reverse Linked List, Best Time to Buy/Sell Stock).
- [ ] 2 LeetCode Medium: Group Anagrams, Top K Frequent Elements.
- [ ] **Rule:** before writing, state approach + Big-O. Before submitting, state 1 edge case you handled.

**Afternoon (2 hrs) — C# fundamentals MCQ**
- [ ] `IEnumerable` vs `IQueryable` vs `ICollection` vs `IList` (10 min on each, write the answer out)
- [ ] `Task` vs `Thread` vs `Task.Run` vs `async void`
- [ ] Value vs reference types, boxing, `struct` vs `class` vs `record`
- [ ] `string` immutability, `StringBuilder`, interning
- [ ] DI lifetimes: singleton/scoped/transient + captive dependency

**Evening (1–2 hrs) — behavioral rehearsal**
- [ ] Record yourself answering: "Tell me about yourself" (2 min, structured: current → past → why this role)
- [ ] Record: "Tell me about a hard technical decision" (use Story B or C)
- [ ] Record: "Tell me about a conflict" (have one ready — peer review pushback works)
- [ ] Listen back. Cut filler words. Re-record once.

---

### **Sunday 2026-05-17 — AI round (Round 1)**
- [ ] (Morning) Light review only: skim hero stories, do 1 easy LeetCode to warm up the brain.
- [ ] (Pre-interview, 30 min) Quiet room, test mic/camera, water, resume + JD open in tabs.
- [ ] (Post-interview) Write down — while fresh — every question they asked. This is gold for Rounds 2/3 because patterns repeat.

---

### **Monday 2026-05-18 — System design + SQL**
Goal: be able to drive a 30-min design discussion. R2 (US) will lean on this.

**Morning (3 hrs) — system design templates**
- [ ] Learn this frame, write it on a sticky note: **Requirements → API contract → data model → scale → failure modes → observability → rollout**
- [ ] Design out loud (15 min each, voice memo): **file ingestion pipeline** (your Import/Export — most likely prompt), **rate limiter**, **notification service with retries**.
- [ ] For the WinForms modernization angle: learn the term **"strangler fig pattern"** + be able to describe it in 2 sentences. That's it.

**Afternoon (2 hrs) — SQL + EF Core**
- [ ] Indexes: clustered vs non-clustered vs covering. When indexes hurt (writes, low cardinality).
- [ ] Write on paper: "top N per group", "find duplicates", basic window function (`ROW_NUMBER()`).
- [ ] EF Core gotchas: N+1 (and how to spot it), tracking vs `AsNoTracking`, deferred execution.
- [ ] Isolation levels — be able to define read committed, repeatable read, serializable, snapshot.

**Evening (1 hr) — security refresh**
- [ ] OAuth 2.0 auth code flow + PKCE — draw it on paper from memory.
- [ ] JWT structure: header.payload.signature. Why payload is signed not encrypted.
- [ ] Refresh token rotation — what attack it mitigates.

---

### **Tuesday 2026-05-19 — Gap closure (talking points only)**
Goal: don't get caught flat-footed on JD topics you've never used.

**Morning (2 hrs) — messaging/event-driven**
- [ ] Read about: at-least-once vs exactly-once, idempotency keys, **Outbox pattern**, dead-letter queues, **Saga** (choreography vs orchestration).
- [ ] Pick AWS SQS+SNS as your anchor (matches your AWS exp). Be able to say: "I'd use SQS for queueing, SNS for fan-out, DLQ for poison messages, idempotency keys at the consumer."
- [ ] Bridge phrase ready: "I haven't built a full event-driven system from scratch, but I've used AWS Lambda for async flows at eGain — here's how I'd extend that pattern…"

**Afternoon (1.5 hrs) — observability**
- [ ] The three pillars: **logs, metrics, traces**.
- [ ] Stack vocabulary: Serilog for structured logging, OpenTelemetry for tracing, correlation IDs across services, Prometheus/Grafana for metrics.
- [ ] Bridge phrase: "We used validation scripts and CloudWatch at eGain — I'm familiar with the concepts, comfortable adopting OpenTelemetry."

**Afternoon (1 hr) — Docker/K8s vocabulary**
- [ ] Image vs container vs pod. Deployment vs StatefulSet. Service vs Ingress. HPA. ConfigMap vs Secret.
- [ ] You don't need to write YAML. You need to *not look blank* when asked.

**Evening (1.5 hrs) — async/await deep dive (high-probability R3 topic)**
- [ ] Why `.Result` / `.Wait` deadlocks in ASP.NET classic (sync context).
- [ ] `ConfigureAwait(false)` — what it does and when it matters now (.NET 6+ less so).
- [ ] `Task.WhenAll` vs awaiting in a loop.
- [ ] `CancellationToken` — pass it through every async API.

---

### **Wednesday 2026-05-20 — Mock rounds + .NET internals**

**Morning (3 hrs) — mock interview block**
- [ ] (45 min) Self-mock: "Design a service for bulk file ingestion." Voice memo. Listen back.
- [ ] (45 min) Self-mock: "Walk me through the JWT auth module you built at IKS." Anticipate follow-ups: algorithm? where keys? refresh flow? revocation?
- [ ] (45 min) Self-mock: "How would you modernize a legacy WinForms app?" → answer in the frame: assess → instrument → strangler fig → migrate slice by slice → retire legacy.
- [ ] (30 min) Write down the gaps you noticed in your own answers. Fix tomorrow.

**Afternoon (2 hrs) — .NET internals (R3 will drill here)**
- [ ] GC generations 0/1/2, LOH, when finalizers run.
- [ ] `Span<T>` / `Memory<T>` — what problem they solve (stack-allocated views).
- [ ] `IDisposable` / `using` / `IAsyncDisposable`.
- [ ] Dispose pattern — when you need a finalizer (almost never in modern code).

**Evening (1 hr) — testing**
- [ ] xUnit `[Fact]` vs `[Theory]`, `IClassFixture` for shared setup.
- [ ] Integration tests with `WebApplicationFactory<T>`.
- [ ] Moq/NSubstitute basics.
- [ ] Bridge phrase: "We used xUnit and validation scripts — for greenfield, I'd reach for `WebApplicationFactory` + TestContainers for DB integration tests."

---

### **Thursday 2026-05-21 — Polish + R2/R3 logistics**

**Morning (2 hrs) — high-leverage drills**
- [ ] Re-record your 4 hero stories. Listen back. They should sound polished, not memorized.
- [ ] Practice the **2-minute intro**: current role → 1-line on past → why Routeware specifically (mention WinForms modernization + their core domain).
- [ ] Skim Section 5 question bank. For any question you can't answer in 1 sentence, write the answer down.

**Afternoon (2 hrs) — design + SQL one more time**
- [ ] One more system design out loud. Pick a fresh one: **URL shortener** or **audit log service**.
- [ ] One SQL query on paper from scratch.

**Evening (1 hr) — reverse questions + logistics**
- [ ] Memorize 3 reverse questions per round (Section 6).
- [ ] Confirm time zones, calendar invites, meeting links.
- [ ] Test camera/mic again. Charge devices.

---

### **Friday 2026-05-22 — interview day(s)**
- [ ] Light review only. Don't cram. Skim hero stories + JD phrases.
- [ ] 30 min before each round: water, bathroom, stretch, mic check, resume + JD open in tabs.
- [ ] After each round: dump questions asked into a notes file. Pattern-spot for the next round.

---

### **Cut from the sprint** (do NOT touch this week)
- Hands-on Docker/K8s setup
- Building a Kafka or Kubernetes demo
- Reading System Design Interview vol 2 cover to cover
- LeetCode Hard problems
- React (mention Vue/Angular fluency, offer to ramp)
- VB.NET (acknowledge gap honestly, frame as legacy-learning opportunity)

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

## 8. Pre-Interview Ritual (every round, 30 min before)

- [ ] Water + bathroom + 2-min stretch
- [ ] Skim your 4 hero stories (read the bold "Use when:" lines only)
- [ ] Skim JD — re-circle the 5 echo phrases
- [ ] Resume + JD + this README open in tabs
- [ ] Mic/camera test
- [ ] Headset on 2 min early — calm breathing, not last-minute review

---

## Quick links to your existing prep
- `../Qentelli/` — .NET + AWS deep dive, reuse system-design notes
- `../NAV/` — round 2 technical coding strategy
- `../Siemens/` — coming up 2026-03-26, overlapping .NET prep
