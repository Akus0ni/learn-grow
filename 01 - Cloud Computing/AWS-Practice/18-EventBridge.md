# AWS EventBridge — Serverless Event Bus

## 1. Core Concepts

### What is EventBridge?
EventBridge (formerly CloudWatch Events) is a **serverless event bus** that enables event-driven architectures. It routes events from AWS services, custom applications, and SaaS partners to targets like Lambda, SQS, SNS, Step Functions, and more — using **rules with pattern matching**.

### EventBridge Components
| Component | Description |
|-----------|-------------|
| **Event Bus** | A pipeline that receives events. Comes in three flavors |
| **Default Event Bus** | Receives all AWS service events (e.g., EC2 state changes, S3 notifications) |
| **Custom Event Bus** | For your application's domain events |
| **Partner Event Bus** | For SaaS integrations (Datadog, Shopify, Zendesk, etc.) |
| **Rule** | Defines which events to match and where to send them |
| **Event Pattern** | JSON filter that matches incoming events |
| **Target** | Where matched events are sent (Lambda, SQS, SNS, Step Functions, etc.) |
| **Schedule** | Cron or rate expressions — EventBridge Scheduler replaces CloudWatch Events schedules |

### Event Structure
Every EventBridge event is a JSON object with a standard envelope:
```json
{
  "version": "0",
  "id": "12345678-1234-1234-1234-123456789012",
  "source": "com.shopwave.orders",
  "account": "123456789012",
  "time": "2024-01-15T10:30:00Z",
  "region": "us-east-1",
  "detail-type": "Order Placed",
  "detail": {
    "orderId": "ORD-001",
    "customerId": "USR-123",
    "total": 99.99
  }
}
```

### Event Pattern Matching
Rules use JSON patterns to match events. Patterns can match on any field:
```json
{
  "source": ["com.shopwave.orders"],
  "detail-type": ["Order Placed"],
  "detail": {
    "total": [{"numeric": [">=", 100]}]
  }
}
```
Matching operators: exact value, prefix, suffix, `anything-but`, numeric ranges, `exists`, `equals-ignore-case`, `wildcard`.

### Targets (up to 5 per rule)
| Target | Notes |
|--------|-------|
| **Lambda** | Most common; invoke synchronously |
| **SQS** | Queue events for async processing |
| **SNS** | Fan-out to multiple subscribers |
| **Step Functions** | Start state machine execution |
| **API Gateway** | HTTP call to REST/HTTP API |
| **EventBridge Bus** | Cross-account/cross-region routing |
| **ECS Task** | Start a Fargate/ECS task |
| **Kinesis Stream** | Stream to data pipeline |
| **CodePipeline** | Trigger CI/CD pipeline |

### EventBridge Pipes
**EventBridge Pipes** connect event sources directly to targets with optional filtering and transformation — no Lambda needed for simple routing:
```
Source (SQS, DynamoDB Streams, Kinesis, etc.)
  → Filter (event pattern)
  → Enrichment (Lambda, API Gateway)
  → Target (SQS, SNS, EventBridge Bus, Step Functions, etc.)
```

### EventBridge Scheduler
**EventBridge Scheduler** is a fully managed scheduler that replaces CloudWatch Events cron rules. It supports:
- **Rate-based**: `rate(1 hour)` — every hour
- **Cron-based**: `cron(0 9 * * ? *)` — every day at 9 AM UTC
- **One-time schedules**: trigger at a specific datetime
- **Timezone support**: schedule in any timezone
- **Flexible time window**: trigger within a time window (prevent thundering herd)
- **DLQ and retry**: for failed invocations

### Cross-Account & Cross-Region Event Routing
EventBridge supports routing events across AWS accounts and regions:
- **Event Bus Resource Policy**: allow another account's events to be received
- **Event Bus Target**: send matched events to an event bus in another account/region
Pattern: central event bus in a security/operations account; all applications send events there.

### Schema Registry
EventBridge can automatically discover and catalog the schemas of events. Schemas can be used to:
- Generate **code bindings** (Java, Python, TypeScript) for type-safe event handling
- Enforce event structure in producers
- Browse and search all event schemas in your organization

### Archive and Replay
EventBridge can **archive events** to S3 for retention (configurable retention period). You can **replay** archived events to a bus to:
- Reprocess events after a consumer bug fix
- Test new event consumers against real historical events
- Populate a new service from past events

---

## 2. Interview Questions & Answers

### Q1. What is EventBridge and how is it different from SNS and SQS?
**EventBridge** is a content-based event router with rich pattern matching across many AWS services, SaaS, and custom sources. Events are routed based on their content to multiple targets.  
**SNS** is a push-based pub/sub service for fan-out; subscribers receive all messages or use simple attribute-based filters.  
**SQS** is a pull-based queue for decoupling producers and consumers; messages persist until consumed.

EventBridge is best when you need **event routing logic based on event content** across many AWS services. SNS is best for **simple fan-out**. SQS is best for **reliable async processing with persistence**.

---

### Q2. What is the difference between a custom event bus and the default event bus?
The **default event bus** automatically receives events from **AWS services** (EC2 state changes, S3 operations, RDS snapshots, CodePipeline status changes, etc.).  
**Custom event buses** are for your application's domain events (e.g., `com.shopwave.orders`). Separating application events onto custom buses provides:
- Isolation between teams/services
- Separate access control (resource policies)
- Cleaner rule management
- Cross-account event routing from custom buses

---

### Q3. How does EventBridge rule pattern matching work?
Rules filter events using **JSON pattern matching**. EventBridge checks if the event matches the pattern — only matched events are forwarded to targets. Patterns support:
- Exact string/number matching: `"source": ["com.shopwave.orders"]`
- Prefix matching: `"prefix": "ORDER-"`
- Numeric conditions: `[{"numeric": [">=", 100]}]`
- `anything-but`: match everything except specific values
- `exists`: check if a field is present
- Wildcard: `"ORDER-*"`
Multiple conditions within an object are ANDed; arrays of values within a field are ORed.

---

### Q4. What is EventBridge Scheduler and when would you use it over Lambda cron?
**EventBridge Scheduler** is a fully managed scheduler with millions of concurrent schedules, timezone support, flexible time windows, and built-in retry with DLQ. It replaces the old CloudWatch Events cron rules.  
Use it instead of Lambda cron (CloudWatch Events) when you need:
- Per-timezone scheduling (e.g., 9 AM local time per user)
- High-volume schedules (millions of one-time or recurring schedules)
- Flexible time windows (spread invocations over 30 minutes to avoid thundering herd)
- DLQ + retry for failed invocations

---

### Q5. How do you route events from multiple AWS accounts to a central operations account?
1. In the **central account**, create a custom event bus and add a **resource policy** allowing source accounts to `PutEvents`
2. In each **source account**, create EventBridge rules with a target pointing to the central account's event bus ARN
3. In the **central account**, create rules on the custom bus to route events to targets (Lambda for processing, S3/Firehose for logging)

This is the **hub-and-spoke** EventBridge pattern for centralized event processing.

---

### Q6. What is EventBridge Pipes and when would you use it?
**EventBridge Pipes** provide a point-to-point integration between a source and a target with optional filtering, enrichment, and transformation — without writing Lambda code for simple routing:
- Source: SQS, DynamoDB Streams, Kinesis, Kafka
- Filter: JSON event pattern
- Enrichment: Lambda, API Gateway (enrich the event with additional data)
- Target: SQS, SNS, EventBridge, Step Functions, API Gateway

Use Pipes when you need to connect an event stream to a target with simple transformation, avoiding the boilerplate Lambda code.

---

### Q7. What happens when an EventBridge target fails to process an event?
EventBridge retries delivery to targets for up to **24 hours** using an **exponential backoff** with jitter. The maximum retry count is configurable (1–185 retries).  
For events that exhaust retries, you can configure a **Dead Letter Queue (SQS)** on the rule's target. Always configure a DLQ and a CloudWatch alarm on DLQ depth for production rules.

---

### Q8. How does EventBridge Schema Registry help development?
The Schema Registry stores discovered schemas for all events on your buses. Benefits:
- **Code generation**: Generate Java/Python/TypeScript classes from schemas — no manual event parsing
- **Discoverability**: Browse all events available across your organization
- **Validation**: Ensure producers emit events that match the registered schema
- **IDE integration**: IntelliSense/autocompletion for event types in your IDE

Enable **schema discovery** on your event bus and EventBridge automatically discovers schemas from events as they flow through.

---

### Q9. How do you implement event sourcing with EventBridge Archive and Replay?
1. Enable **Archive** on your event bus with the desired retention period
2. Events are stored in S3 automatically
3. When you deploy a new service or fix a bug, start a **Replay** from the archive:
   - Filter by time range and event pattern
   - Replay events to your bus as if they were live
4. Your consumer processes the replayed events to rebuild its state from history

---

### Q10. How do you test EventBridge rules?
1. Use **EventBridge Sandbox** in the AWS console — paste an event JSON and test against your rule's pattern without sending real events
2. Use **Event Bus test**: send a test event via the console or CLI using `PutEvents`
3. Enable **CloudWatch Logs target** on a rule to log all matched events for debugging
4. Use **AWS SAM or CDK** to write unit tests against the rule pattern (test the JSON matching logic locally)

---

### Q11. What is the event delivery guarantee of EventBridge?
EventBridge provides **at-least-once** delivery — in rare cases an event may be delivered more than once. Targets must be idempotent.  
EventBridge does **not guarantee ordering** of events. If you need ordered processing, use SQS FIFO as the target and process from there.

---

### Q12. How do you monitor EventBridge in production?
- **CloudWatch Metrics**: `MatchedEvents`, `TriggeredRules`, `InvocationAttempts`, `FailedInvocations`, `ThrottledRules`
- **CloudWatch Logs**: Add a CloudWatch Logs target to any rule to capture matched events
- **DLQ Monitoring**: CloudWatch alarm on DLQ `ApproximateNumberOfMessagesVisible > 0`
- **EventBridge Pipe metrics**: `ExecutionsFailed`, `ExecutionsTimedOut`

---

### Q13. What is the difference between EventBridge and CloudWatch Events?
EventBridge **is** CloudWatch Events — it was renamed and enhanced. Additional features in EventBridge:
- **Partner event buses** (SaaS integrations)
- **Custom event buses** (multi-account routing)
- **Schema Registry**
- **Archive and Replay**
- **EventBridge Pipes**
- **EventBridge Scheduler** (migrated from CloudWatch Events)

CloudWatch Events still works and shares the same infrastructure; the console just redirects to EventBridge.

---

### Q14. How do you handle event schema evolution with EventBridge?
**Forward compatibility**: Add new optional fields to events — existing consumers ignore unknown fields (if they use tolerant deserialization).  
**Breaking changes**: Introduce a new `detail-type` (e.g., `Order Placed v2`) while keeping the old one. Migrate consumers gradually, then retire the old event type.  
**Schema versioning**: Include a `schemaVersion` field in the event detail for consumers to handle multiple versions.  
Use the **Schema Registry** to track schema changes over time.

---

### Q15. Design an event-driven architecture for an e-commerce platform using EventBridge.
```
Domain Events Published to Custom Event Bus: com.shopwave

[Lambda: Order Service]
  → PutEvents → [EventBridge Bus: shopwave-events]
                      │
          ┌───────────┼────────────────────────────────────┐
          │           │                                    │
  Rule: Order Placed  Rule: Payment Completed     Rule: Order Shipped
          │           │                                    │
  [SQS: inventory]  [Lambda: email-sender]        [SQS: fulfilment-queue]
  → Deduct stock    → Send confirmation email     → Trigger 3PL API
          │
  [Step Functions: fulfilment-workflow]

  Rule: * (all events)
          │
  [Kinesis Firehose] → [S3: event-archive] → [Athena: analytics]

  EventBridge Scheduler: rate(1 hour)
          │
  [Lambda: abandoned-cart-checker] → SES reminder emails
```

---

## 3. Real-World Use Case: Microservices Event Backbone

### Scenario
ShopWave has 8 microservices that need to react to each other's events without tight coupling. They need audit logging, replay capability, and cross-account routing to a central security account.

### Architecture

```
[Order Service] → PutEvents
[Payment Service] → PutEvents        → [shopwave-events bus]
[Inventory Service] → PutEvents             │
[User Service] → PutEvents                  │
                                            │
                    ┌───────────────────────┤
                    │                       │
        ┌───────────▼──────────┐  ┌────────▼──────────────┐
        │  Application Rules   │  │  Cross-Account Route  │
        │  • Order Placed      │  │  → Security Account   │
        │    → SQS inventory   │  │    event bus          │
        │  • Payment Failed    │  │  (audit all events)   │
        │    → SNS ops-alert   │  └───────────────────────┘
        │  • User Created      │
        │    → Lambda welcome  │
        └──────────────────────┘
                    │
        ┌───────────▼──────────┐
        │  EventBridge Archive │
        │  Retention: 90 days  │
        │  Replay on demand    │
        └──────────────────────┘

EventBridge Scheduler:
  - Daily: rate(1 day) → Lambda(generateDailyReport)
  - Hourly: rate(1 hour) → Lambda(checkAbandonedCarts)
  - One-time: new user sign-up + 48h → Lambda(send-verification-reminder)
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Custom event bus per domain | Isolate domains; separate access control |
| Archive enabled (90 days) | Replay events when adding new consumers or fixing bugs |
| Cross-account routing to security | Central audit trail without coupling services to audit logic |
| DLQ on every rule target | No event lost silently; alert on failures |
| EventBridge Scheduler for 1-time | Dynamic per-user schedules without cron management |

### Interview Narration (Whiteboard Script)
> "EventBridge is our microservices event backbone. Every service publishes domain events to a custom bus — Order Placed, Payment Completed, User Created. Rules route these events to the right consumers without the services knowing about each other. We archive all events for 90 days so we can replay them when we add a new service — it catches up on history without needing a separate sync. We also have a cross-account rule that copies all events to our security account for audit. Scheduling is handled by EventBridge Scheduler — no cron Lambda boilerplate."
