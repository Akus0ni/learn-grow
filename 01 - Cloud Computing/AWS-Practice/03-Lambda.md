# AWS Lambda — Serverless Functions

## 1. Core Concepts

### What is Lambda?
AWS Lambda lets you run code without provisioning or managing servers. You upload your function, configure a trigger, and Lambda runs it on demand — scaling automatically from zero to thousands of concurrent executions.

**Billing**: Pay only for what you use — per-request + per GB-second of compute duration (rounded to 1ms).

### Supported Runtimes
| Runtime | Notes |
|---------|-------|
| Node.js 18/20 | Most popular for APIs |
| Python 3.11/3.12 | Data processing, ML inference |
| Java 17/21 | Enterprise workloads (watch cold start latency) |
| .NET 6/8 | Enterprise .NET workloads |
| Go 1.x | High-performance, fast cold starts |
| Ruby 3.2 | Web/scripting |
| Custom Runtime | Any language via Lambda Runtime API (Rust, C++, Elixir, etc.) |
| Container Image | Up to 10 GB — run any containerized workload as Lambda |

### Limits (Memorize These)
| Parameter | Limit |
|-----------|-------|
| Max execution timeout | **15 minutes** |
| Max memory | **10 GB** |
| Ephemeral storage (`/tmp`) | **512 MB – 10 GB** |
| Max deployment package (zip) | 50 MB (250 MB unzipped) |
| Max container image size | 10 GB |
| Concurrent executions (default) | 1,000 per region (soft limit, can increase) |
| Max payload (sync invocation) | 6 MB request / 6 MB response |
| Max payload (async invocation) | 256 KB |

### Invocation Types
| Type | How | Response | Use Case |
|------|-----|----------|----------|
| **Synchronous** | SDK, API Gateway, ALB, CLI | Wait for result | REST APIs, request/response |
| **Asynchronous** | S3 events, SNS, EventBridge | Immediate ACK, result discarded | Event processing, fire-and-forget |
| **Poll-based (Event Source Mapping)** | Lambda polls SQS, Kinesis, DynamoDB Streams, Kafka | Batch of records | Stream processing, queue processing |

### Cold Starts
A **cold start** is the initialization penalty when Lambda starts a new execution environment:
1. Download function package
2. Start runtime
3. Run initialization code (outside handler)
4. Run handler

**Factors that increase cold start latency:**
- JVM-based runtimes (Java, .NET) — 1–10 seconds
- Large deployment package
- VPC attachment (adds ~0.5s historically; improved with Hyperplane ENIs since 2019)

**Mitigation strategies:**
- **Provisioned Concurrency**: Pre-warms N execution environments — eliminates cold start latency
- **SnapStart** (Java only): Pre-initializes and takes a snapshot of the execution environment
- Use Python/Go/Node for latency-sensitive APIs
- Keep package size small; load large assets lazily

### Concurrency Model
- **Unreserved concurrency**: Share pool with other functions in account (default)
- **Reserved concurrency**: Guarantee N concurrent executions for this function; also acts as a throttle (max)
- **Provisioned concurrency**: Pre-warmed execution environments — no cold starts, you pay even when idle

### Triggers (Event Sources)
| Category | Services |
|----------|---------|
| **Synchronous** | API Gateway, ALB, Function URL, Cognito, Lex, CloudFront (Lambda@Edge) |
| **Asynchronous** | S3, SNS, EventBridge, CodeCommit, CloudWatch Logs, IoT |
| **Poll-based** | SQS, Kinesis Data Streams, DynamoDB Streams, MSK (Kafka), MQ |

### Execution Environment
- Each invocation gets an isolated environment
- Environment is **reused** for subsequent invocations (warm execution)
- Store reusable objects (DB connections, SDK clients) **outside the handler** to benefit from reuse
- `/tmp` is persisted across warm invocations (not across cold starts)

### Lambda Layers
Layers are ZIP archives that can be shared across functions. Used for:
- Common dependencies (requests, boto3, pandas)
- Custom runtimes
- Configuration/shared code

A function can use up to **5 layers** (250 MB total unzipped).

### Lambda@Edge vs CloudFront Functions
| | Lambda@Edge | CloudFront Functions |
|--|-------------|---------------------|
| Runtime | Node.js or Python | JavaScript (ES5) |
| Execution location | CloudFront edge PoPs | CloudFront edge PoPs |
| Max execution time | 30 sec (origin) / 5 sec (viewer) | 1 ms |
| Max memory | 128 MB–10 GB | 2 MB |
| Cost | Higher | Very low |
| Use Case | Complex transformations, auth, A/B tests | URL rewrites, header manipulation, simple auth |

### Destinations & Error Handling
- **Dead Letter Queue (DLQ)**: Failed async invocations sent to SQS or SNS
- **Lambda Destinations**: Route success/failure to SQS, SNS, Lambda, EventBridge
- **Retry behavior**: Async invocations retry twice automatically; event source mappings retry until success or record expires

---

## 2. Interview Questions & Answers

### Q1. What is a Lambda cold start? How do you mitigate it?
A cold start is the latency incurred when Lambda provisions a new execution environment. This includes downloading the package, starting the runtime, and running initialization code. Worst offenders are Java/.NET (~2–10s) and large packages.

**Mitigations:**
- **Provisioned Concurrency** — pre-warms environments (eliminates cold starts, adds cost)
- **SnapStart** (Java) — snapshot initialization phase, restore on invocation
- Use lightweight runtimes (Python, Go, Node.js) for latency-sensitive endpoints
- Keep packages small — use Lambda Layers for shared dependencies
- Keep global/init code warm by using Scheduled EventBridge ping (anti-pattern — use Provisioned Concurrency instead)

---

### Q2. What is the difference between Reserved Concurrency and Provisioned Concurrency?
- **Reserved Concurrency**: Sets a *maximum* concurrent execution limit for a function. Protects downstream services from Lambda thundering-herd and guarantees capacity isn't consumed by other functions. A function with reserved concurrency = 0 is effectively disabled.
- **Provisioned Concurrency**: Pre-initializes N execution environments. These are always ready — no cold start. You pay for them even when idle. Use for latency-sensitive, predictable workloads (e.g., checkout API during sale events).

---

### Q3. How does Lambda handle errors in asynchronous invocations?
For async invocations (S3, SNS, EventBridge), Lambda retries **twice** on failure with exponential backoff. After all retries are exhausted, the event is:
- Sent to a **Dead Letter Queue** (SQS or SNS) if configured
- Sent to a **Lambda Destination** (for failure)

Best practice: Configure both DLQ and Destinations. Monitor DLQ message count as an alarm.

---

### Q4. What are Lambda Layers and when would you use them?
Lambda Layers are ZIP archives attached to functions, providing shared libraries and custom runtimes. Benefits:
- Reuse code across multiple functions without bundling each time
- Separate function code from dependencies — faster deployments for code-only changes
- Share large ML models or data files (up to 5 layers, 250 MB total)

Example: A Layer containing `pandas`, `numpy`, and `scikit-learn` for all ML inference functions.

---

### Q5. How does Lambda integrate with SQS? What is a batch window?
Lambda polls SQS via an **Event Source Mapping**. It retrieves batches of messages (up to 10,000 for SQS Standard, configurable) and passes them to your function.

- **Batch size**: Number of messages per invocation (1–10,000)
- **Batch window**: Maximum wait time (0–300 seconds) before invoking, even if batch size not reached — improves efficiency for low-volume queues
- On failure: Lambda leaves messages in queue (SQS retries until visibility timeout / DLQ)
- **Bisect on error**: If a batch fails, Lambda splits it in half to find the bad message

---

### Q6. What is the Lambda execution environment lifecycle?
1. **Init phase**: Download code, start runtime, run initialization code (outside handler)
2. **Invoke phase**: Run handler; environment waits for next invocation (kept warm for ~15 min)
3. **Shutdown phase**: Runtime sends shutdown event; environment is frozen and eventually terminated

Objects initialized in the **Init phase** (DB connections, SDK clients) are reused across invocations in the same environment.

---

### Q7. How do you handle database connections in Lambda?
Opening a new DB connection per invocation is wasteful. Solutions:
1. **Initialize the connection pool outside the handler** — reused across warm invocations
2. Use **RDS Proxy** — connection pooler that sits between Lambda and RDS; shares a pool of connections, handles thousands of Lambda connections without exhausting DB limits
3. Use DynamoDB (connection-less HTTP API) for Lambda-heavy workloads

---

### Q8. What is Lambda Function URL?
A built-in HTTPS endpoint for a Lambda function without needing API Gateway. Supports:
- IAM authentication (`AWS_IAM`) or no auth (`NONE`)
- Streaming responses (via `RESPONSE_STREAM` invocation mode)
- Good for simple APIs, webhooks, internal tools
- Not suitable when you need API Gateway features (throttling, caching, request validation, usage plans)

---

### Q9. What is Lambda@Edge and when would you use it over a regular Lambda?
Lambda@Edge runs your function at CloudFront edge locations — closest to the user. It can intercept and modify CloudFront requests and responses at four points: viewer request, origin request, origin response, viewer response.

Use cases:
- A/B testing by rewriting URLs based on a cookie
- Authenticating JWT tokens at the edge before origin is called
- Adding security headers to every response
- Serving different content based on device type / language

Regular Lambda runs in one region — Lambda@Edge runs globally in 400+ PoPs.

---

### Q10. How does Lambda pricing work?
Two dimensions:
1. **Requests**: $0.20 per 1 million requests
2. **Duration**: $0.0000166667 per GB-second (e.g., 1GB × 1 second = 1 GB-second)

**Free Tier**: 1 million requests/month + 400,000 GB-seconds/month — never expires.

**Example**: A 512MB function running 100ms, called 1M times/month:
- Duration: 1M × 0.5 GB × 0.1 s = 50,000 GB-seconds × $0.0000166667 = **$0.83**
- Requests: $0.20
- **Total: ~$1.03/month**

---

### Q11. Explain the difference between synchronous and asynchronous Lambda invocations.
- **Synchronous**: Caller waits for result. Lambda returns response after function completes. Used by API Gateway, ALB, direct SDK calls. Errors propagate to caller.
- **Asynchronous**: Caller gets immediate 202 ACK. Lambda queues the event internally and invokes the function. Caller doesn't wait for or receive the result. Used by S3, SNS, EventBridge. Lambda retries twice on failure.

---

### Q12. What is the difference between Lambda and ECS/Fargate?
| | Lambda | ECS/Fargate |
|--|--------|-------------|
| Execution model | Request-based, auto-scale to zero | Long-running containers |
| Max runtime | 15 minutes | Unlimited |
| State | Stateless | Can be stateful |
| Cold start | Yes | No (container already running) |
| Billing | Per ms of execution | Per vCPU-hour and GB-hour while running |
| Use case | Event-driven, short tasks, APIs | Long-running workers, WebSocket servers, batch |

---

### Q13. What is Lambda SnapStart?
SnapStart is a performance feature for **Java** Lambda functions. Lambda initializes the function at publish time, takes a snapshot (Firecracker microVM snapshot), and restores from it on invocation. This reduces Java cold start from ~5–10 seconds to milliseconds. Available for Java 11+ on arm64 and x86.

---

### Q14. How do you pass configuration to a Lambda function?
1. **Environment variables**: Simple key-value pairs, encrypted at rest with KMS (up to 4 KB)
2. **SSM Parameter Store**: Hierarchical config, versioning, SecureString (KMS encrypted), free standard tier
3. **AWS Secrets Manager**: Managed secret rotation, automatic database credential rotation, higher cost
4. **S3**: Large config files, JSON/YAML configurations
5. **AppConfig**: Feature flags and dynamic config with validation and deployment strategies

---

### Q15. What monitoring and observability does Lambda provide out of the box?
- **CloudWatch Metrics**: Invocations, Duration, Errors, Throttles, ConcurrentExecutions, IteratorAge (for stream-based)
- **CloudWatch Logs**: Lambda automatically sends stdout/stderr to a log group (`/aws/lambda/function-name`)
- **AWS X-Ray**: Distributed tracing — enable active tracing on the function; use the X-Ray SDK to instrument downstream calls
- **Lambda Insights**: Enhanced CloudWatch metrics for memory utilization, init duration, CPU usage (uses a Lambda Extension)

---

## 3. Real-World Use Case: Event-Driven Order Processing System

### Scenario
An e-commerce platform needs to:
- Process orders as they arrive without managing servers
- Handle peaks of 50,000 orders/minute during flash sales
- Send confirmation emails, update inventory, generate invoices
- Process failed orders with retries and alerting
- Run under $100/month at normal volume

### Architecture

```
Customer Places Order
        │
        ▼
[API Gateway] ──► [Lambda: Order Validator]
        │               │
        │         Validates schema,
        │         writes to DynamoDB
        │               │
        │               ▼
        │         [SQS: order-queue]  ◄── Dead Letter Queue: order-dlq
        │               │
        ▼               ▼
[Lambda: Order Processor]  (batch size: 10, batch window: 5s)
        │
        ├──► [Lambda: Email Sender] (SNS → Lambda)
        │           │
        │     [SES] sends confirmation email
        │
        ├──► [Lambda: Inventory Updater]
        │           │
        │     [DynamoDB] decrements stock
        │
        └──► [Lambda: Invoice Generator]
                    │
              [S3] stores PDF invoice
              [Pre-signed URL] sent to email

[EventBridge Scheduler] ──► [Lambda: Daily Report]
        │
  Aggregates orders from DynamoDB
  Generates CSV report → S3
  Notifies business team via SNS

[CloudWatch Alarm] (order-dlq depth > 10)
        │
        ▼
[SNS] → PagerDuty / Slack alert
```

### Lambda Functions Breakdown

| Function | Trigger | Memory | Timeout | Key Design |
|----------|---------|--------|---------|------------|
| Order Validator | API Gateway (sync) | 256 MB | 10s | Validates JSON schema, returns 400 on invalid |
| Order Processor | SQS poll (async) | 512 MB | 60s | Idempotent — checks DynamoDB if already processed |
| Email Sender | SNS (async) | 128 MB | 15s | Retry via Lambda Destinations on SES failure |
| Inventory Updater | SNS (async) | 128 MB | 15s | DynamoDB conditional write — prevents oversell |
| Invoice Generator | SNS (async) | 512 MB | 30s | Generates PDF, uploads to S3, returns pre-signed URL |
| Daily Report | EventBridge (async) | 1 GB | 5 min | Reserved concurrency: 1 (only one report at a time) |

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| SQS between API Gateway and Processor | Decouples order intake from processing; absorbs flash sale spikes |
| Batch size 10, batch window 5s | Groups orders for efficiency without adding significant latency |
| DLQ on SQS | Failed orders preserved for investigation and manual reprocessing |
| Idempotency in Order Processor | SQS At-Least-Once delivery — same order could arrive twice |
| RDS Proxy for DynamoDB substitute | If using RDS, RDS Proxy prevents connection exhaustion under Lambda spikes |
| Provisioned Concurrency on Validator | API-facing — must not cold-start during checkout |

### Cost Estimate (Normal Volume: 100k orders/day)

| Function | Invocations/mo | Duration | Cost/mo |
|----------|---------------|---------|---------|
| Order Validator | 3M | 50ms @ 256MB | ~$0.10 |
| Order Processor | 3M | 200ms @ 512MB | ~$0.83 |
| Email/Inventory/Invoice | 9M | 100ms @ 128MB | ~$0.30 |
| Daily Report | 30 | 4min @ 1GB | ~$0.01 |
| **Total** | | | **~$1.24/month** |

### Interview Narration (White-board Script)
> "For the event-driven order system I'd put SQS between API Gateway and the order processor. This decouples order intake from processing — during a flash sale we might get 50,000 orders/minute but the processor scales gradually through the batch polling model. Each processor invocation gets a batch of up to 10 messages. Lambda's event source mapping handles scaling concurrency automatically. I make the processor idempotent using a DynamoDB conditional write — if the order ID already exists, skip it. This handles the At-Least-Once SQS delivery guarantee. Failed messages go to the DLQ, and a CloudWatch alarm on DLQ depth triggers a PagerDuty alert."
