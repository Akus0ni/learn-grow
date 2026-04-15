# MongoDB & RabbitMQ — Basics Q&A

> These are "nice to have" areas per the JD. If asked, give conceptual answers confidently.
> Don't over-claim hands-on experience — say "I've worked with SQL Server extensively, and I understand
> the core concepts of document databases and message brokers."

---

## MongoDB

**Q: What is MongoDB? How does it differ from SQL Server?**

| | MongoDB (NoSQL) | SQL Server (Relational) |
|---|---|---|
| Data model | Documents (BSON/JSON) | Tables with rows and columns |
| Schema | Flexible / schema-less | Fixed schema |
| Relationships | Embedding or references | Foreign keys, JOINs |
| Scaling | Horizontal (sharding) | Vertical (primarily) |
| Transactions | Supported (multi-doc, v4+) | ACID, mature |
| Query language | MQL (MongoDB Query Language) | SQL |
| Best for | Unstructured/semi-structured, high volume, flexible schema | Structured data, complex relationships, reporting |

---

**Q: What is a document in MongoDB?**

A: A document is a JSON-like record (stored as BSON — Binary JSON). Similar to a row in SQL, but can have nested objects and arrays.

```json
{
  "_id": "ObjectId('abc123')",
  "clientId": "C001",
  "fundName": "Growth Fund",
  "transactions": [
    { "date": "2025-01-15", "amount": 10000, "type": "subscription" },
    { "date": "2025-02-10", "amount": -5000, "type": "redemption" }
  ],
  "metadata": {
    "createdAt": "2025-01-01",
    "status": "active"
  }
}
```

---

**Q: What are MongoDB's key concepts?**

| Concept | MongoDB | SQL Server equivalent |
|---|---|---|
| Database | Database | Database |
| Collection | Collection | Table |
| Document | Document | Row |
| Field | Field | Column |
| `_id` | ObjectId (auto-generated) | Primary Key |
| Index | Index | Index |
| Aggregation Pipeline | `$match`, `$group`, `$sort`... | GROUP BY, WHERE, ORDER BY |

---

**Q: When would you choose MongoDB over SQL Server?**

A: Choose MongoDB when:
- **Schema flexibility** needed — data shape varies per record (e.g., fund configurations)
- **High write throughput** — logging, events, IoT data
- **Nested/hierarchical data** — avoid complex JOINs by embedding
- **Horizontal scaling** needed

Choose SQL Server when:
- **Strong relationships** and referential integrity required
- **Complex reporting** and ad-hoc queries
- **ACID transactions** across multiple tables are critical
- Existing enterprise tooling (SSRS, SSAS, etc.)

---

**Q: What is an aggregation pipeline in MongoDB?**

A: A framework for data transformation and analysis, processing documents through a sequence of stages.

```javascript
db.transactions.aggregate([
  { $match: { status: "completed" } },      // filter (like WHERE)
  { $group: {                                // group (like GROUP BY)
      _id: "$clientId",
      totalAmount: { $sum: "$amount" },
      count: { $sum: 1 }
  }},
  { $sort: { totalAmount: -1 } },           // sort (like ORDER BY)
  { $limit: 10 }                            // take top 10
])
```

---

**Q: What is embedding vs referencing in MongoDB?**

A:
- **Embedding** (denormalization) — Store related data inside the same document. Best for data that's always read together, small sub-arrays.
  ```json
  { "orderId": 1, "items": [{ "sku": "A", "qty": 2 }] }
  ```
- **Referencing** (normalization) — Store related data in separate collections, link by ID. Best for large sub-documents, shared data.
  ```json
  { "orderId": 1, "clientId": "C001" }  // client stored in separate collection
  ```

---

## RabbitMQ

**Q: What is RabbitMQ? What problem does it solve?**

A: RabbitMQ is a message broker — middleware that enables applications to communicate asynchronously via messages. It decouples producers (senders) from consumers (receivers).

**Problems it solves:**
- **Async processing** — Don't wait for slow operations (email, report gen)
- **Load leveling** — Buffer bursts of traffic
- **Decoupling** — Producer doesn't need to know about consumers
- **Reliability** — Messages are persisted until consumed

---

**Q: What are the core concepts of RabbitMQ?**

| Concept | Description |
|---|---|
| **Producer** | Application that sends messages |
| **Consumer** | Application that receives and processes messages |
| **Queue** | Buffer that stores messages until consumed |
| **Exchange** | Receives messages from producers and routes to queues |
| **Binding** | Link between an exchange and a queue (with routing key) |
| **Message** | Data payload (headers + body) |
| **Channel** | Lightweight connection multiplexed over TCP connection |
| **Virtual Host (vhost)** | Logical grouping for isolation |

---

**Q: What are the types of exchanges in RabbitMQ?**

| Exchange Type | Routing Logic | Use Case |
|---|---|---|
| **Direct** | Routes to queue with exact matching routing key | Point-to-point, task queues |
| **Fanout** | Broadcasts to all bound queues (ignores routing key) | Pub/sub, notifications |
| **Topic** | Routes by pattern matching (`*.orders.#`) | Event-driven systems |
| **Headers** | Routes based on message headers | Complex routing logic |

---

**Q: How does RabbitMQ ensure message reliability?**

A:
1. **Message persistence** — Messages marked as persistent are saved to disk.
2. **Acknowledgements (ACK)** — Consumer sends ACK after successful processing; if it crashes, message is re-queued.
3. **Publisher confirms** — Broker confirms receipt to producer.
4. **Dead Letter Queues (DLQ)** — Failed/expired messages go to a DLQ for inspection.
5. **Durable queues** — Queues survive broker restart.

---

**Q: What is the difference between RabbitMQ and other messaging systems?**

| | RabbitMQ | Apache Kafka | Azure Service Bus |
|---|---|---|---|
| Model | Push (broker pushes to consumers) | Pull (consumers poll) | Push |
| Message retention | Deleted after consume | Retained for configurable period | Deleted after consume |
| Use case | Task queues, RPC, routing | Event streaming, log aggregation | Enterprise messaging |
| Ordering | Per-queue | Per-partition | Per-session |

---

**Q: How would you use RabbitMQ in a .NET application?**

A: Use the `RabbitMQ.Client` NuGet package or the higher-level `MassTransit` library.

```csharp
// Publishing a message (Producer)
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "order-created", durable: true, ...);

var message = JsonSerializer.Serialize(new OrderCreatedEvent(orderId));
var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(
    exchange: "",
    routingKey: "order-created",
    basicProperties: null,
    body: body);
```

```csharp
// Consuming messages (Consumer)
channel.BasicConsume(
    queue: "order-created",
    autoAck: false,
    consumer: new EventingBasicConsumer(channel)
    {
        Received = (model, ea) =>
        {
            var order = JsonSerializer.Deserialize<OrderCreatedEvent>(ea.Body.Span);
            ProcessOrder(order);
            channel.BasicAck(ea.DeliveryTag, false); // manual ACK
        }
    });
```

---

## Quick Comparison: SQL vs NoSQL

| Aspect | SQL (SQL Server) | NoSQL (MongoDB) |
|---|---|---|
| Schema | Rigid | Flexible |
| Relationships | Enforced (FK) | Application-managed |
| Transactions | Strong ACID | Eventually consistent (or ACID for single doc) |
| Scaling | Scale up | Scale out |
| Query | SQL (declarative) | MQL / Aggregation |
| Best fit | Financial, relational data | Catalogs, logs, user profiles, flexible data |
