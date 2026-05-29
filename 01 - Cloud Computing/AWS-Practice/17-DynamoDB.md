# AWS DynamoDB — Managed NoSQL Database

## 1. Core Concepts

### What is DynamoDB?
DynamoDB is a **fully managed, serverless NoSQL key-value and document database** that delivers single-digit millisecond performance at any scale. There are no servers to provision, no schema migrations, and it automatically replicates data across multiple Availability Zones.

### Data Model
| Concept | Description |
|---------|-------------|
| **Table** | Top-level container for items (similar to a table in SQL) |
| **Item** | A record in the table (similar to a row); up to 400 KB |
| **Attribute** | A key-value field on an item (similar to a column) — schemaless except for keys |
| **Partition Key (PK)** | Required; determines which partition holds the item |
| **Sort Key (SK)** | Optional; enables range queries and ordering within a partition |
| **Primary Key** | PK alone (simple), or PK + SK (composite) |

### Key Selection (Critical for Performance)
The partition key must distribute data evenly across partitions. Poor key choice creates **hot partitions** (one partition handles most I/O → throttling):

| ❌ Bad Partition Key | ✅ Good Partition Key |
|---------------------|----------------------|
| `status` (few values) | `userId` (high cardinality) |
| `date` (daily spike) | `orderId` (UUID) |
| `country` (skewed traffic) | `productId` + random suffix |

Use a **composite key** (`PK + SK`) to model one-to-many relationships in a single table:
```
PK=USER#123, SK=PROFILE → user profile
PK=USER#123, SK=ORDER#001 → order 1
PK=USER#123, SK=ORDER#002 → order 2
```

### Capacity Modes
| Mode | Description | Best For |
|------|-------------|----------|
| **Provisioned** | Specify Read/Write Capacity Units (RCU/WCU); can use Auto Scaling | Predictable, steady workloads |
| **On-Demand** | Pay per request; scales instantly | Unpredictable or spiky workloads |

**RCU/WCU Definitions**:
- 1 RCU = 1 strongly consistent read per second for items up to 4 KB (or 2 eventually consistent reads)
- 1 WCU = 1 write per second for items up to 1 KB

### Consistency Models
| Model | Description | Cost |
|-------|-------------|------|
| **Eventually Consistent Reads** | Data may be up to ~1 s stale; higher throughput | 0.5 RCU |
| **Strongly Consistent Reads** | Always returns latest data | 1 RCU |
| **Transactional Reads** (TransactGet) | Atomic multi-item reads | 2 RCU |
| **Transactional Writes** (TransactWrite) | All-or-nothing multi-item writes | 2 WCU |

### Secondary Indexes
| Type | Description | Consistency |
|------|-------------|------------|
| **GSI** (Global Secondary Index) | Different PK + optional SK; spans entire table; separate throughput | Eventually consistent only |
| **LSI** (Local Secondary Index) | Same PK as table, different SK; must be created at table creation | Strongly or eventually consistent |

Use **GSI** to query by any attribute. DynamoDB can have up to 20 GSIs per table.

### DynamoDB Streams
Captures a time-ordered sequence of item-level changes (INSERT, MODIFY, REMOVE) for up to 24 hours. Consumers:
- Lambda triggers for real-time change processing
- Kinesis Data Streams for extended retention + analytics
- Cross-region replication (Global Tables use streams internally)

### Global Tables
DynamoDB Global Tables provide **multi-region, multi-active** replication. All regions can accept reads and writes. Conflict resolution is **last-writer-wins** (by timestamp). Ideal for:
- Global low-latency reads and writes
- Cross-region disaster recovery (RPO near 0, RTO seconds)

### DynamoDB Accelerator (DAX)
DAX is a **fully managed, in-memory cache** for DynamoDB with microsecond read latency. It's a drop-in replacement (same API). Best for:
- Read-heavy workloads with repeated queries for the same items
- Applications requiring < 1 ms read latency

Not suitable for: strongly consistent reads, write-heavy workloads, applications that rarely re-read the same data.

### Additional Features
| Feature | Description |
|---------|-------------|
| **TTL (Time to Live)** | Automatically delete expired items (no WCU cost) — ideal for sessions, cache |
| **Point-in-Time Recovery (PITR)** | Restore table to any second in the last 35 days |
| **On-Demand Backup** | Full table backup to S3 without performance impact |
| **Conditional Writes** | Write only if a condition is met (optimistic locking) |
| **PartiQL** | SQL-like query language for DynamoDB |

---

## 2. Interview Questions & Answers

### Q1. How is DynamoDB different from a relational database?
| | DynamoDB | RDBMS |
|--|---------|-------|
| Schema | Schemaless (flexible attributes) | Fixed schema, migrations required |
| Query language | Key-based + index scans | Full SQL with joins |
| Joins | No native joins (denormalize instead) | Full JOIN support |
| ACID | Item-level + multi-item transactions | Full ACID |
| Scaling | Horizontal (unlimited) | Vertical (limited) |
| Latency | Single-digit ms at any scale | Varies; degrades at scale |
| Use case | High-scale, flexible data; sessions, catalogs | Complex queries, reporting, strict ACID |

---

### Q2. What is the difference between a partition key and a sort key?
The **partition key** (hash key) determines which physical partition stores the item. DynamoDB applies a hash function to distribute items evenly.  
The **sort key** (range key) allows multiple items with the same partition key, stored sorted. This enables range queries (`BETWEEN`, `BEGINS_WITH`, `>`, `<`) on the sort key.  
A composite key (PK + SK) models hierarchical data in one table — e.g., all orders for a user (`PK=USER#123, SK=ORDER#*`).

---

### Q3. What are GSIs and when should you use them?
A **Global Secondary Index (GSI)** projects selected attributes from the table into a separate index with a different partition key (and optional sort key). You can query the GSI just like the base table.  
Use GSIs when you need to query by attributes other than the primary key. Example: query orders by `status` and `createdAt` — create a GSI with PK=`status`, SK=`createdAt`.  
GSIs have their own read/write capacity. Writes that touch a GSI attribute consume WCUs from both the table and the GSI.

---

### Q4. What is the difference between strongly consistent and eventually consistent reads?
**Eventually consistent reads** return data that may be up to ~1 second stale (from a replica that hasn't received the latest write). They cost 0.5 RCU.  
**Strongly consistent reads** always return the latest data (routed to the primary replica). They cost 1 RCU and have slightly higher latency. Not available for GSIs.  
For most use cases (display, caching), eventually consistent is fine and cheaper. Use strongly consistent for cases where stale data would cause incorrect behavior (inventory checks, financial balances).

---

### Q5. What are DynamoDB Transactions and when do you use them?
`TransactWrite` and `TransactGet` provide **ACID transactions across multiple items and tables** within the same account and region.  
Use transactions for:
- **Order + inventory deduction** — both succeed or both fail
- **Conditional updates** — "update item only if field X = Y AND field Z > 0"
- **Multi-table operations** — write to orders AND inventory tables atomically
Cost: 2× the normal RCU/WCU.

---

### Q6. How does DynamoDB auto-scaling work?
DynamoDB Auto Scaling uses **Application Auto Scaling** to adjust provisioned RCU/WCU based on the `ConsumedReadCapacityUnits` and `ConsumedWriteCapacityUnits` metrics:
1. You set a **target utilization** (e.g., 70% of provisioned capacity)
2. Auto Scaling adjusts capacity up or down (within your min/max bounds) to maintain the target
3. There's a brief lag (minutes) for capacity to adjust — for sudden spikes, use **On-Demand mode** or pre-warm capacity

---

### Q7. What is the Single-Table Design pattern?
Instead of one DynamoDB table per entity type (like in SQL), **single-table design** puts all entities into one table using generic `PK` and `SK` attributes. Different entity types use different key prefixes:
```
PK=USER#123, SK=PROFILE → user record
PK=USER#123, SK=ORDER#456 → order record
PK=ORDER#456, SK=ITEM#789 → order line item
PK=PRODUCT#999, SK=METADATA → product record
```
Benefits: fewer tables, better query efficiency (fetch user + all orders in one query), no hot-partition issues from cross-table joins.  
Trade-off: complex access patterns must be designed upfront.

---

### Q8. How do DynamoDB Streams work?
DynamoDB Streams capture item-level changes in near-real time (within milliseconds) for up to 24 hours. Each stream record contains the before/after image of the item.  
Common uses:
- **Lambda trigger**: process changes in real time (e.g., update an Elasticsearch index when a product changes)
- **Replication**: sync data to a secondary data store (S3, Redshift)
- **Audit trail**: log all changes to CloudWatch Logs or a history table

---

### Q9. What is DAX and when would you use it?
**DynamoDB Accelerator (DAX)** is an in-memory, write-through cache for DynamoDB. It has microsecond latency vs DynamoDB's millisecond latency.  
Use DAX when:
- Read-heavy workloads with repeated queries for the same items (product catalog, user profiles)
- You need < 1 ms latency
- You want to reduce DynamoDB read costs at high volume

Don't use DAX for: strongly consistent reads (DAX serves eventually consistent reads only), write-heavy workloads, or rarely-repeated queries.

---

### Q10. How does DynamoDB handle hot partitions?
A **hot partition** occurs when too many requests are directed at the same partition key. DynamoDB's partition throughput limit is 3,000 RCU or 1,000 WCU per partition.  
Mitigation strategies:
- **Write sharding**: Append a random suffix (1–10) to the partition key (`status#3`), then query all shards and aggregate
- **Use On-Demand mode**: Absorbs spikes without needing to predict capacity
- **Caching with DAX**: Cache hot items to reduce partition read pressure
- **Better key design**: Choose high-cardinality keys that naturally distribute load

---

### Q11. What is TTL in DynamoDB and how does it work?
**Time to Live (TTL)** is an attribute you define (e.g., `expiresAt`) that contains a Unix timestamp. DynamoDB automatically deletes items after the timestamp passes (typically within 48 hours, but usually much sooner). Deletion is **free** — no WCU consumed.  
Use TTL for: session data, caches, temporary tokens, audit logs with a retention period, leases.

---

### Q12. How do you implement optimistic locking in DynamoDB?
Use a **version attribute** and a conditional expression:
1. Read item; note `version = 5`
2. Write: `UpdateItem ... SET version = 6 CONDITION version = 5`
3. If another process updated it since your read, `version` is now `6` and your condition fails → `ConditionalCheckFailedException`
4. Retry: re-read and re-attempt

This prevents lost updates in concurrent scenarios without pessimistic locks.

---

### Q13. What are the limits of DynamoDB transactions?
- Maximum **100 operations** per `TransactWrite` or `TransactGet` call
- Maximum **4 MB** total data size per transaction
- Transactions are within a single **AWS account and region**
- Not available across Global Table replicas (each region's transaction is local)
- Cost is **2× normal RCU/WCU**

---

### Q14. What is Point-in-Time Recovery (PITR) and when would you enable it?
PITR continuously backs up your table and lets you restore it to any second within the last **35 days**. It protects against accidental writes, deletes, or bad migrations.  
Enable PITR for all production tables. Cost is 0.20/GB/month of table storage (additional charge). Restore creates a **new table** — you then redirect your application to the new table.

---

### Q15. Design a DynamoDB schema for an e-commerce application.
**Access Patterns**: Get user profile, get all orders for a user, get order details, get products by category.

**Single-Table Design**:
```
PK                SK               Data
-------------------------------------------------
USER#123          PROFILE          {name, email, address}
USER#123          ORDER#2024-001   {status, total, items}
USER#123          ORDER#2024-002   {status, total, items}
ORDER#2024-001    ITEM#1           {productId, qty, price}
ORDER#2024-001    ITEM#2           {productId, qty, price}
PRODUCT#999       METADATA         {name, description, price, category}

GSI1: PK=status, SK=createdAt → query orders by status
GSI2: PK=category, SK=productId → browse products by category
```

**Query examples**:
- `PK=USER#123, SK begins_with ORDER#` → all orders for a user
- `GSI1: PK=PENDING, SK > 2024-01-01` → all pending orders after a date

---

## 3. Real-World Use Case: E-Commerce Product Catalog & Order Store

### Scenario
ShopWave needs to store user profiles, order history, and product catalog with:
- Single-digit ms reads for product browsing
- Atomic order + inventory deduction
- Query orders by status for fulfilment team
- Automatic session expiry

### Architecture

```
[React App] → [API Gateway] → [Lambda]
                                  ↓
                          [DynamoDB Table: shopwave]
                          ├── USER#* / PROFILE    → User data
                          ├── USER#* / ORDER#*    → Order per user
                          ├── ORDER#* / ITEM#*    → Line items
                          └── PRODUCT#* / META    → Product catalog

                          GSI: status-index
                          PK=status, SK=createdAt
                          → Fulfilment team queries pending/shipped orders

[DynamoDB Streams] → [Lambda: search-indexer]
                          → [OpenSearch] → product search

[DAX Cluster]
  → Cache hot product reads (microsecond latency)
  → Cache user session tokens (TTL = 3600s)

TransactWrite for orders:
  - Write ORDER record
  - Decrement INVENTORY count (condition: count >= qty)
  All-or-nothing: if inventory check fails, entire order rejected
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Single-table design | Fetch user + orders in one query (`PK=USER#123, SK begins_with ORDER#`) |
| GSI on status | Fulfilment dashboard queries without full table scan |
| TTL on sessions | Automatic cleanup without cron jobs or extra cost |
| DAX for product reads | Browse catalog gets < 1 ms latency; reduces DynamoDB RCU cost by 80% |
| TransactWrite for orders | Atomic deduction prevents overselling without an external lock |
| PITR enabled | Instant recovery from accidental deletes in 35-day window |

### Interview Narration (Whiteboard Script)
> "I'd use a single-table design where user profiles, orders, and line items all coexist in one table using PK and SK prefixes. This lets me fetch a user and all their orders in a single query. Product browsing sits behind DAX — the same DynamoDB API but with microsecond reads from the in-memory cache. For the order flow, I use TransactWrite to deduct inventory and create the order record atomically — if the product is out of stock, the entire transaction fails and the customer sees an error immediately. Orders feed into DynamoDB Streams, which triggers a Lambda to keep our OpenSearch product index current."
