# AWS SQS — Simple Queue Service

## 1. Core Concepts

### What is SQS?
SQS is a **fully managed, serverless message queuing service** that enables decoupling of distributed application components. Producers send messages to a queue; consumers poll and process them independently. SQS guarantees **at-least-once delivery**.

### Queue Types
| Feature | Standard Queue | FIFO Queue |
|---------|---------------|-----------|
| **Throughput** | Unlimited (nearly) | 3,000 msg/s with batching; 300 msg/s without |
| **Ordering** | Best-effort (not guaranteed) | Strict FIFO per `MessageGroupId` |
| **Delivery** | At-least-once (duplicates possible) | Exactly-once processing |
| **Deduplication** | None | 5-minute deduplication window |
| **Name suffix** | `MyQueue` | `MyQueue.fifo` |
| **Use case** | High-throughput, order-insensitive | Orders, financial transactions, sequencing |

### Key Concepts
| Term | Meaning |
|------|---------|
| **Visibility Timeout** | Time a message is invisible to other consumers after being received (default 30 s, max 12 hr) |
| **Message Retention** | How long messages stay in queue if not processed (1 min – 14 days, default 4 days) |
| **Delivery Delay** | Delay before message is visible to consumers (0 – 15 min) |
| **Long Polling** | Consumer waits up to 20 s for messages instead of returning empty — reduces API calls and cost |
| **Short Polling** | Returns immediately even if queue is empty — more expensive |
| **Dead Letter Queue (DLQ)** | Receives messages that fail processing after `maxReceiveCount` attempts |
| **Message Group ID** | FIFO: groups messages processed in order; multiple groups processed in parallel |
| **Deduplication ID** | FIFO: unique ID per message; prevents duplicate delivery within 5-min window |

### Message Lifecycle
```
Producer → SendMessage → [Queue]
                              ↓
                    Consumer → ReceiveMessage
                              ↓
                    Message becomes invisible (Visibility Timeout)
                              ↓
             Process Success: Consumer → DeleteMessage → removed from queue
             Process Failure: Timeout expires → message reappears → retry
             Max retries hit → message moved to DLQ
```

### Dead Letter Queues (DLQ)
A DLQ is a separate queue that receives messages after they fail `maxReceiveCount` processing attempts. Use DLQs to:
- Isolate and analyze failed messages
- Trigger alerts on DLQ depth (CloudWatch Alarm → SNS)
- Replay messages after fixing the consumer bug

> DLQ type must match source queue (Standard→Standard, FIFO→FIFO).

### SQS vs SNS vs EventBridge
| | SQS | SNS | EventBridge |
|--|-----|-----|------------|
| Model | Pull (consumer polls) | Push (fan-out) | Event-driven rules |
| Consumers | Single consumer group | Multiple subscribers | Multiple targets |
| Message persistence | Yes (up to 14 days) | No (fire-and-forget) | No |
| Filtering | Visibility timeout based | Message attribute filters | Advanced pattern matching |
| Use case | Work queues, buffering | Notification fan-out | Event routing, SaaS integration |

### Scaling Patterns
- **Lambda trigger**: Lambda polls SQS and processes in batches (batch size 1–10,000); scales to 1,000 concurrent instances
- **EC2/ECS**: Consumer scales based on `ApproximateNumberOfMessagesVisible` CloudWatch metric → ASG or ECS Service Auto Scaling
- **Batch window**: Lambda waits up to N seconds to accumulate messages before invoking (reduces Lambda invocations)

### Security
- **Encryption at rest**: AWS managed KMS key (SSE-SQS) or customer managed KMS key (SSE-KMS)
- **Encryption in transit**: TLS enforced by default
- **Access control**: IAM policies + SQS resource policy (cross-account access)
- **VPC Endpoint**: Private access from VPC without internet gateway

---

## 2. Interview Questions & Answers

### Q1. What is the difference between Standard and FIFO queues?
**Standard** queues offer nearly unlimited throughput and at-least-once delivery — messages may arrive out of order or be duplicated. Best for high-volume, order-insensitive workloads (image processing, log ingestion).  
**FIFO** queues preserve exact order and provide exactly-once processing (deduplication). Throughput is limited to 3,000 msg/s with batching. Use for financial transactions, order processing, or any scenario where sequence matters.

---

### Q2. What is the visibility timeout and why is it important?
The **visibility timeout** is the period during which a received message is hidden from other consumers. If the consumer processes and deletes the message within this window, no other consumer sees it. If the consumer fails or times out, the message reappears for retry.  
Setting it correctly prevents both **double processing** (too short) and **long delays** (too long). Best practice: set it to at least 6× your average processing time, and call `ChangeMessageVisibility` to extend it dynamically for long-running tasks.

---

### Q3. What is long polling and why should you use it?
**Short polling** returns immediately even if the queue is empty, wasting API calls and incurring cost. **Long polling** waits up to 20 seconds for a message to arrive before returning an empty response.  
Long polling reduces the number of empty `ReceiveMessage` calls, lowers SQS costs (fewer API requests), and reduces CPU/network overhead in consumers. Always use `WaitTimeSeconds=20` unless you have a specific reason for short polling.

---

### Q4. What is a Dead Letter Queue and when should you use it?
A DLQ captures messages that repeatedly fail processing. After `maxReceiveCount` receive attempts, SQS moves the message to the DLQ instead of letting it loop forever.  
Always configure a DLQ for production queues. Set a CloudWatch alarm on `ApproximateNumberOfMessagesVisible` for the DLQ to alert on failures. Use DLQ replay (SQS DLQ Redrive) to reprocess messages after the bug is fixed.

---

### Q5. How does SQS integrate with Lambda?
Lambda has a native **event source mapping** for SQS. Lambda polls the queue and invokes your function with a batch of records. Key settings:
- **Batch size**: 1–10,000 messages per invocation
- **Batch window**: 0–300 s to accumulate messages before invoking
- **Concurrency**: Lambda scales from 0 to 1,000 concurrent executions based on queue depth
- **Error handling**: On failure, the batch is returned to the queue (or moved to DLQ if configured). Use `ReportBatchItemFailures` to mark only failed items for retry instead of the whole batch.

---

### Q6. How does SQS Fan-Out work with SNS?
Fan-out is an architecture where one SNS topic is subscribed to by multiple SQS queues:
```
Producer → SNS Topic → SQS Queue A (Service A)
                    → SQS Queue B (Service B)
                    → SQS Queue C (Service C)
```
Benefits:
- Each service processes messages independently at its own pace
- Queues buffer messages if a service is slow
- A service failure doesn't affect others
- Filters on SNS subscriptions route only relevant messages to each queue

---

### Q7. What are FIFO message groups?
In FIFO queues, **MessageGroupId** identifies a logical group. Messages in the same group are processed strictly in order. Messages in **different groups** can be processed in parallel.  
Example: `MessageGroupId = "user-123"` ensures all events for user 123 are ordered, while events for different users are processed concurrently.

---

### Q8. How do you scale consumers based on queue depth?
Create a **CloudWatch metric** on `ApproximateNumberOfMessagesVisible`:
1. Create an Auto Scaling policy on the consumer ASG (or ECS service) that scales out when messages exceed a threshold
2. Use **Target Tracking** on `ApproximateNumberOfMessagesVisible / running instances` to maintain a constant messages-per-instance ratio
3. Set a scale-in cooldown to avoid thrashing
For Lambda consumers, scaling is automatic — Lambda scales to match queue depth.

---

### Q9. What is at-least-once delivery and how do you handle duplicate messages?
SQS Standard guarantees **at-least-once delivery** — in rare cases a message may be delivered more than once. Consumers must be **idempotent**:
- Track processed message IDs in DynamoDB with a conditional write (fail if already exists)
- Use database UPSERT instead of INSERT for updates
- Design operations that are safe to repeat (e.g., setting a flag is idempotent; incrementing a counter is not)
FIFO queues provide exactly-once processing within the deduplication window.

---

### Q10. How does SQS encryption work?
**SSE-SQS**: SQS manages the encryption key using an AWS managed CMK. No additional cost. Transparent to consumers.  
**SSE-KMS**: Use a customer managed KMS key (CMK) for additional control — key rotation policies, cross-account access, CloudTrail audit. Adds KMS API call cost.  
In-transit: TLS is always enforced for SQS API calls.  
For high-security workloads, enforce KMS and use a VPC endpoint to avoid internet exposure.

---

### Q11. What is the maximum message size in SQS and how do you handle large payloads?
SQS maximum message size is **256 KB**. For larger payloads:
1. Use the **Java/Python SQS Extended Client Library** — it stores the payload in S3 and puts an S3 reference in the SQS message
2. Store the payload in S3 manually and include the S3 key in the SQS message
3. Consumer reads S3 key from message, fetches full payload from S3

This pattern is common for large binary files, images, or JSON documents.

---

### Q12. What is message delay and when would you use it?
**Delivery delay** (0–900 s) prevents consumers from seeing a message until the delay expires. Use cases:
- Allow a database write to propagate before a consumer reads it (eventual consistency buffer)
- Implement retry backoff — increasing delay on failure: `ChangeMessageVisibility` or re-enqueue with delay
- Schedule future processing (e.g., send reminder email 10 minutes after cart abandonment)

---

### Q13. How do you implement a priority queue with SQS?
SQS doesn't natively support priority. Common approaches:
1. **Multiple queues**: Create separate queues for high, medium, low priority. Consumer always drains the high-priority queue first before checking lower-priority queues.
2. **Weighted polling**: Consumer polls the high-priority queue N times more frequently than low-priority queues.
3. **Visibility timeout manipulation**: For urgent re-processing, use `ChangeMessageVisibility` with `VisibilityTimeout=0` to make a message immediately visible.

---

### Q14. What are the cost components of SQS?
| Component | Cost |
|-----------|------|
| First 1M requests/month | Free |
| Standard queue requests | $0.40 per million |
| FIFO queue requests | $0.50 per million |
| Data transfer (inter-region) | Standard AWS data transfer rates |
| KMS API calls (if SSE-KMS) | $0.03 per 10,000 API calls |

Each API call is 64 KB of data — a 256 KB message counts as 4 API calls.

---

### Q15. Design an order processing system using SQS.
```
[Customer] → POST /orders → API Gateway → Lambda (Order Validator)
                                                ↓
                                    [SQS FIFO Queue: orders.fifo]
                                    MessageGroupId = customerId
                                                ↓
                                    Lambda (Order Processor) × N
                                    ├── Write order to DynamoDB
                                    ├── Deduct inventory
                                    └── Send confirmation → SNS → SES (email)
                                                ↓ (on failure)
                                    [DLQ: orders-dlq.fifo]
                                    CloudWatch Alarm → PagerDuty
```
FIFO ensures orders per customer are processed in sequence. DLQ captures failures for investigation without losing orders.

---

## 3. Real-World Use Case: Order Processing & Inventory Decoupling

### Scenario
ShopWave needs to process 100,000 orders/day with:
- Guaranteed ordering per customer (no race conditions)
- Retry on payment failures without losing orders
- Decoupled inventory, fulfilment, and notification services

### Architecture

```
[API Gateway] → [Order Lambda]
                     ↓ Validate + enqueue
         [SQS FIFO: orders.fifo]  (MessageGroupId=customerId)
                     ↓
         [Order Processor Lambda] (batch=10, window=5s)
         ├── [DynamoDB: orders table]
         ├── → [SQS Standard: inventory-updates]
         │           ↓
         │   [Inventory Lambda] → [DynamoDB: inventory]
         │
         ├── → [SQS Standard: fulfilment-jobs]
         │           ↓
         │   [Fulfilment ECS Service]
         │
         └── → [SNS: order-events]
                     ↓
              [SES: confirmation email]

[DLQ: orders-dlq.fifo]
  └── CloudWatch Alarm (depth > 0) → SNS → PagerDuty
  └── EventBridge Scheduler: DLQ Redrive after hotfix deploy
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| FIFO queue for orders | Per-customer ordering; exactly-once processing |
| Standard queues for downstream | High throughput for inventory/fulfilment; ordering not required |
| DLQ on all queues | No order ever lost; failures isolated and replayable |
| CloudWatch alarm on DLQ depth | Alert within 60 s of first failure |
| Lambda batch with `ReportBatchItemFailures` | Only failed messages go back to queue; successful ones aren't retried |

### Interview Narration (Whiteboard Script)
> "Orders go into a FIFO queue grouped by customer ID — this ensures we never process two orders from the same customer concurrently, avoiding race conditions on inventory. Lambda processes batches of 10 with ReportBatchItemFailures so only genuinely failed items get retried. Downstream services — inventory, fulfilment, email — each have their own Standard queues so they scale independently. A DLQ on every queue captures failures, and a CloudWatch alarm fires the moment any failed message arrives. This architecture handles 100,000 orders/day and can scale to 10× with zero config changes."
