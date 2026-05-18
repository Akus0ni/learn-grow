# R2 System Design Prep — 3 High-Probability Prompts

> **The Frame (write this on a sticky note before R2 starts):**
> **Clarify → API contract → Data model → Scale & perf → Failure modes → Observability → Rollout**
>
> Use it for *every* design question. It signals senior-level structured thinking.

> **Golden rules:**
> 1. **Ask 2–3 clarifying questions first.** Never start drawing without scoping.
> 2. **State 1 assumption out loud per minute.** Interviewers love this.
> 3. **Always offer 2 options + the trade-off.** Then recommend one. Picking is senior; listing is junior.
> 4. **Mention observability before they ask.** It's in the JD — score points unprompted.

---

## Prompt 1: File Ingestion Pipeline (YOUR wheelhouse — Import/Export)

> "Design a service that ingests files uploaded by external customers, validates them, transforms them, and stores the results for downstream querying."

### Clarifying questions (ask these — don't skip)
- What's the file format and typical size? (CSV/XML/binary? KB or GB?)
- What's the ingestion volume — 100/day, 100/sec, bursty?
- Is it batch or streaming? Do customers wait synchronously for a result?
- What does "failure" mean — reject the whole file, or partial-success with bad rows quarantined?
- Who consumes the output — another service, a UI, a data warehouse?

**Assume for the walkthrough:**
> "I'll assume CSVs averaging 50MB, bursty uploads peaking at ~500/hour, async with a notification when done, and partial success with bad-row quarantine."

### API contract
```
POST /api/v1/ingestions
  body: { fileName, contentType, checksum, callbackUrl? }
  returns: { ingestionId, uploadUrl (presigned S3) }

PUT  <uploadUrl>                    -- client uploads directly to S3
GET  /api/v1/ingestions/{id}        -- poll status
GET  /api/v1/ingestions/{id}/errors -- bad rows, paginated
```
- **Why presigned URL:** keeps file bytes out of your API, scales independently.
- **Idempotency:** client sends `Idempotency-Key` header on POST — store it for 24h, return same `ingestionId` on retry.

### Data model
- **Ingestions table** (Postgres): `id, customer_id, status, file_uri, checksum, row_count, error_count, created_at, completed_at`
- **Errors table**: `ingestion_id, row_number, error_code, error_message` — partitioned by `ingestion_id`
- **Processed data**: depends on output — could be a data warehouse table, a normalized OLTP table, or a downstream queue.

### Architecture (draw this)
```
Client → API Gateway → Ingestion API ──► Postgres (metadata)
                              │
                              ▼
                          S3 (raw files)
                              │
                          S3 Event → SQS (ingestion-queue)
                                        │
                                        ▼
                              Worker Service (autoscaled)
                                ├─► Validate → SQS (DLQ on poison)
                                ├─► Transform
                                └─► Write output + update Postgres
                                        │
                                        ▼
                                    SNS → email/webhook to customer
```

### Scale & performance
- **Workers are stateless** — scale horizontally on SQS queue depth.
- **Stream the file** — don't load 50MB into memory; use `Stream` + `CsvHelper` with row-by-row processing.
- **Bulk insert** validated rows in batches of ~1000 (`COPY` in Postgres or `SqlBulkCopy` in SQL Server).
- **Backpressure**: SQS visibility timeout + max-in-flight limits prevent worker overload.

### Failure modes (the senior-engineer differentiator)
- **Poison file** (corrupt, malformed) → 3 retries with exponential backoff, then DLQ + alert.
- **Partial parse failure** → continue processing, accumulate errors, return partial-success status.
- **Worker crash mid-file** → checkpoint progress (last processed row) in Postgres; restart resumes from checkpoint.
- **Duplicate upload** → checksum + idempotency key dedupe at API layer.
- **Slow downstream** → circuit breaker (Polly) around the output write; if open, push to retry queue.

### Observability (call this out unprompted — JD asks for it)
- **Structured logs** (Serilog) with `ingestionId` + `customerId` as correlation fields on every log line.
- **Metrics** (Prometheus or CloudWatch): ingestion rate, error rate, p50/p95/p99 processing time per file, queue depth.
- **Tracing** (OpenTelemetry): one trace per ingestion, spans for validate/transform/write.
- **Dashboards**: error rate by customer, queue depth, worker saturation.

### Rollout
- **Behind a feature flag** for first cohort of customers.
- **Shadow mode** first: run new pipeline alongside old, compare outputs, alert on divergence.
- **Canary**: 1% → 10% → 50% → 100% over a week.

### Anchor to your experience
> "This is very close to the Import/Export subsystem I built at Energy Exemplar — same shape: clean ingestion contract, transformation library, multiple consumer surfaces. Main difference is yours is multi-tenant from day one, mine was internal-team consumers."

---

## Prompt 2: Rate Limiter

> "Design a rate limiter for our public API. We have multiple tiers — free customers get 100 req/min, paid get 10K req/min."

### Clarifying questions
- Per-user or per-IP or per-API-key? (Almost always: per-API-key for paid, per-IP for unauthenticated.)
- Hard limit (reject) or soft (throttle/queue)?
- Distributed (multiple API instances) or single-node?
- Acceptable to be slightly over the limit during a burst?

**Assume:**
> "Per-API-key, hard reject with 429, distributed across many instances, allow short bursts."

### Algorithm choice — offer 2, pick 1
| Algorithm | Pros | Cons |
|---|---|---|
| **Fixed window** | Simple, cheap | Burst at window boundary (2x limit possible) |
| **Sliding window log** | Precise | Expensive memory (stores every request) |
| **Sliding window counter** | Good precision, cheap | Slightly approximate |
| **Token bucket** ⭐ | Allows controlled bursts, smooth, industry standard | Slightly more state |

> "I'd go with **token bucket** — it allows legitimate bursts (which our paying customers will want) while still enforcing the long-run rate. Refill rate = limit / window, bucket size = burst allowance."

### API contract (what's the *response* contract?)
- On allow: pass through, add headers:
  ```
  X-RateLimit-Limit: 10000
  X-RateLimit-Remaining: 9847
  X-RateLimit-Reset: 1715800000
  ```
- On deny: `429 Too Many Requests` + `Retry-After: 12` header.

### Data model & storage
- **Redis** is the standard answer. Why: atomic ops (`INCR`, Lua scripts), sub-ms latency, TTLs built in.
- Key: `ratelimit:{apiKey}:{minute}` for fixed window, or store `{tokens, lastRefill}` for token bucket.
- Use Lua script for atomic check-and-decrement (avoids race conditions).

### Architecture
```
Client → API Gateway → Rate Limit Middleware → API
                              │
                              ▼
                         Redis (cluster, replicated)
```
- **Middleware in .NET**: implement as `IMiddleware`, runs before auth-heavy work (cheap rejection).
- **Place limiter before expensive work** — don't auth the request if you're going to reject it for rate.

### Failure modes
- **Redis down** → fail open (allow request) with alarm, OR fail closed (reject all) — **call this out as a business decision, not a tech one**. "Fail open is usually right for public APIs to avoid outage amplification; fail closed if abuse is the bigger risk."
- **Hot key** (one customer hammers one Redis shard) → consistent hashing across cluster, or sharding by `apiKey % N`.
- **Clock skew** across API nodes → use Redis time, not local time.

### Observability
- Metrics: requests allowed/denied per customer tier, Redis latency, fail-open events.
- Alert when a single customer crosses 90% of limit for 5+ min — could be runaway client.

### Rollout
- Start in **observe-only mode** — log what *would* be rejected, don't actually reject. Tune limits against real traffic. Then flip to enforcing.

---

## Prompt 3: Notification Service (with retries + multiple channels)

> "Design a service that lets internal teams send notifications to our customers — email, SMS, in-app. Some are transactional (password reset), some are bulk (marketing)."

### Clarifying questions
- Volume? (10K/day vs 10M/day changes everything.)
- Are transactional and bulk on the same service or separate? (Recommended: same service, different priorities.)
- Multi-tenant — do customers configure their own templates?
- Compliance — GDPR/CAN-SPAM unsubscribe handling needed?

**Assume:**
> "1M/day with bursty peaks, mixed transactional + bulk in one service with priority lanes, templated, with unsubscribe."

### API contract
```
POST /api/v1/notifications
  body: {
    recipient: { customerId, channels: ["email", "sms"] },
    templateId: "password-reset",
    variables: { firstName: "Akash", resetLink: "..." },
    priority: "transactional" | "bulk",
    idempotencyKey: "..."
  }
  returns: { notificationId, status: "queued" }

GET /api/v1/notifications/{id}  -- status + delivery attempts
```

### Architecture
```
Producer ──► Notification API ──► Postgres (state)
                    │
                    ▼
        ┌─────────────────────────┐
        │ SQS: transactional-queue│ (high priority, low latency)
        │ SQS: bulk-queue         │ (separate consumers, rate-limited)
        └─────────────────────────┘
                    │
                    ▼
        Channel Workers (per channel)
        ├─► Email Worker → SES / SendGrid
        ├─► SMS Worker   → Twilio
        └─► InApp Worker → WebSocket / push
                    │
                    ▼
            Status updates → Postgres
                    │
                    ▼
            Webhooks back to producers
```

### Why two queues?
- **Priority lanes prevent bulk from starving transactional.** Password resets can't sit behind 500K marketing emails.
- Different worker pools, different scaling rules.

### Data model
- `notifications` — id, recipient, template, variables (jsonb), status, created_at
- `delivery_attempts` — notification_id, channel, attempt_num, status, provider_response, attempted_at
- `templates` — versioned, immutable once published

### Reliability patterns (this is where you score)
- **Outbox pattern**: notification API writes to Postgres + outbox table in *one transaction*. A relay polls outbox → SQS. **Guarantees no notification is lost if SQS write fails.**
- **Idempotency keys**: producer sends one, we dedupe for 24h.
- **Retry with exponential backoff + jitter**: 1s, 4s, 16s, 64s, 256s. After 5 attempts → DLQ + alert.
- **Per-channel circuit breaker**: if SendGrid 5xx-ing, open circuit for 30s, retry sparingly.
- **Bounce handling**: webhook from SES/SendGrid → mark address as bad → don't retry to known-bad addresses.

### Compliance
- **Unsubscribe table**: every send checks it. Honor List-Unsubscribe header.
- **Audit log**: who sent what to whom, immutable, retained per regulatory requirement.

### Observability
- Metrics: send rate by channel + priority, delivery latency p50/p95/p99, bounce rate, provider error rate.
- One trace per notification end-to-end.
- Alert: transactional latency p95 > 5s, bounce rate spike, DLQ depth > N.

### Rollout
- Start with 1 channel (email), 1 producer team. Expand by channel, then by producer team.
- Shadow mode for templates — render and log, don't send.

### Anchor
> "The reliability patterns here — outbox, retries with jitter, DLQs — are what I'd retrofit onto the Lambda async workflows I built at eGain. We did some of this implicitly; here I'd make it explicit."

---

## What to Do When You Don't Know

If asked something genuinely outside your range:
> *"I haven't built this exact thing in production — let me reason about it from first principles. [Then use the frame.]"*

This is **better** than bluffing. Senior engineers reason; junior engineers recite.

---

## Practice Plan
- **Mon afternoon:** Practice **Prompt 1 (file ingestion)** out loud for 15 min. Voice memo. Listen back.
- **Tue afternoon:** Practice **Prompt 2 (rate limiter)** out loud. Voice memo.
- **Wed morning:** Practice **Prompt 3 (notification service)** out loud. Voice memo.
- **Thu morning:** Pick a *new* prompt (URL shortener / audit log) and do it cold to test if the frame is internalized.
