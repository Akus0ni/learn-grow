# AWS ElastiCache — In-Memory Caching

## 1. Core Concepts

### What is ElastiCache?
ElastiCache is a **fully managed in-memory caching service** that supports **Redis** and **Memcached**. It delivers sub-millisecond read latency for frequently accessed data, offloading your databases and reducing application response times.

### Redis vs Memcached
| Feature | ElastiCache for Redis | ElastiCache for Memcached |
|---------|----------------------|--------------------------|
| **Data structures** | Strings, lists, sets, sorted sets, hashes, bitmaps, HyperLogLog, streams | Strings only |
| **Persistence** | Yes (RDB snapshots, AOF) | No |
| **Replication** | Yes (read replicas) | No |
| **Multi-AZ / Failover** | Yes (automatic failover) | No |
| **Pub/Sub** | Yes | No |
| **Lua scripting** | Yes | No |
| **Cluster mode** | Yes (horizontal sharding) | Yes (auto-discovery) |
| **Transactions** | Yes (MULTI/EXEC) | No |
| **Geospatial** | Yes | No |
| **Use case** | Sessions, leaderboards, queues, real-time analytics | Simple caching, scale-out read cache |

> **Rule of thumb**: Use **Redis** for almost everything. Use **Memcached** only if you need simple object caching with maximum multi-threaded performance and don't need persistence, replication, or advanced data structures.

### ElastiCache for Redis Architecture
```
[Application]
     │
 [Redis Primary Node]  ← Reads + Writes
     ├── [Redis Replica 1]  ← Read-only (scale reads)
     └── [Redis Replica 2]  ← Read-only + failover candidate

On primary failure:
  → Automatic failover: replica promoted to primary
  → DNS endpoint updated automatically
  → Downtime: typically 10–30 seconds
```

### Cluster Modes
| Mode | Description | Scaling |
|------|-------------|---------|
| **Cluster Mode Disabled** | Single shard; 1 primary + up to 5 replicas | Vertical (change node type) |
| **Cluster Mode Enabled** | Up to 500 shards; data partitioned across shards | Horizontal (add shards) |

Cluster Mode Enabled is required for very large datasets that don't fit in a single node. Each shard has its own primary and replicas.

### Node Types
| Family | Optimized For | Example |
|--------|--------------|---------|
| **r7g** (Graviton) | Memory-optimized | r7g.large (13.07 GB RAM) |
| **r6g** | General memory | r6g.xlarge (26.32 GB RAM) |
| **m7g** | Balanced | m7g.large (6.38 GB RAM) |
| **t4g** | Burstable / dev-test | t4g.micro (0.5 GB RAM) |

### Caching Strategies
| Strategy | How It Works | Best For |
|----------|-------------|---------|
| **Lazy Loading (Cache-Aside)** | Read from cache; on miss, read DB and populate cache | Read-heavy, tolerant of stale data |
| **Write-Through** | Write to DB + cache simultaneously | Low-latency reads, fresh data |
| **Write-Behind (Write-Back)** | Write to cache first; async flush to DB | Write-heavy, eventual consistency OK |
| **TTL (Expiration)** | Items expire after TTL; prevents stale data | All strategies — combine with above |

### Common Use Cases
| Use Case | Data Structure | TTL |
|----------|---------------|-----|
| Session storage | Hash (userId → session data) | 30 min |
| API response cache | String (cache-key → JSON) | 60 s |
| Rate limiting | String with INCR | 1 s rolling window |
| Leaderboard | Sorted Set (score → userId) | No TTL |
| Pub/Sub messaging | Pub/Sub channels | N/A |
| Distributed lock | String with SET NX EX | Lock duration |
| Queue | List (LPUSH / BRPOP) | N/A |

### Security
- **VPC**: Deploy in private subnets; never expose publicly
- **Encryption at rest**: AES-256 (Redis with encryption enabled)
- **Encryption in transit**: TLS 1.2+
- **Redis AUTH**: Password-based authentication (or IAM auth for Redis 7.0+)
- **Security Groups**: Restrict access to port 6379 from application security groups only

### ElastiCache Serverless (Redis)
**ElastiCache Serverless** (launched 2023) automatically scales capacity in seconds with no cluster management. Pay per GB of data stored and per ECPUs consumed. Ideal for variable workloads. Limitations: higher cost per unit at sustained high load vs provisioned.

---

## 2. Interview Questions & Answers

### Q1. What is the difference between ElastiCache for Redis and Memcached?
**Redis** supports rich data structures (lists, sets, sorted sets, hashes), persistence (snapshots and AOF), replication with automatic failover, pub/sub, and Lua scripting. It's the right choice for most production use cases.  
**Memcached** is a simple key-value string cache with multi-threaded architecture. It supports no persistence, no replication, and no data structures beyond strings. Use it only when you need maximum multi-threaded performance for simple object caching and don't need any Redis features.

---

### Q2. What is cache-aside (lazy loading) and what are its trade-offs?
In **cache-aside**, the application:
1. Checks the cache for the key
2. On a **hit**: returns cached value
3. On a **miss**: reads from the DB, writes to cache, returns the value

**Advantages**: Cache only contains data that's actually requested; resilient to cache failure (falls through to DB).  
**Disadvantages**: First request after a miss is slow (double read); cache may contain stale data until TTL expires; can lead to a "thundering herd" on cache warm-up.

---

### Q3. What is a write-through caching strategy?
In **write-through**, every write to the database also writes to the cache simultaneously:
1. Application writes to DB
2. Application writes same data to cache

**Advantages**: Cache is always fresh; reads are always fast.  
**Disadvantages**: Write latency doubles; many cached items may never be read (cache pollution). Combine with TTL to expire unused items.

---

### Q4. How does Redis sorted sets work and what's a real use case?
A **sorted set** stores unique members, each with a floating-point score. Members are automatically sorted by score. Operations:
- `ZADD leaderboard 1500 "user123"` — add/update member
- `ZRANGE leaderboard 0 9 REV WITHSCORES` — top 10 by score
- `ZRANK leaderboard "user123"` — get rank of a user

Real use case: **Game leaderboard** — update player scores in real time; retrieve top-N players in O(log N) time. This would be expensive with SQL `ORDER BY` + `LIMIT` at scale.

---

### Q5. How does ElastiCache help with session management?
Instead of storing sessions in the database (slow, hard to scale) or on the EC2 instance (breaks with horizontal scaling), store sessions in Redis:
1. Generate session ID on login; store user data in Redis hash: `HSET session:{id} userId 123 email user@example.com`
2. Set TTL: `EXPIRE session:{id} 1800` (30 min)
3. On each request: `HGETALL session:{id}` (sub-ms)
4. On logout: `DEL session:{id}`

Benefits: Any instance can serve any request (no sticky sessions); automatic expiry; sub-ms reads.

---

### Q6. What happens when an ElastiCache Redis primary node fails?
ElastiCache with Multi-AZ enabled performs **automatic failover**:
1. ElastiCache detects the primary is unavailable (typically within 60 s)
2. A replica is promoted to primary
3. The DNS CNAME for the primary endpoint is updated to point to the new primary
4. Application reconnects to the same endpoint — no code change required

Downtime is typically 10–60 seconds. Existing connections must be retried.

---

### Q7. What is Cluster Mode and when do you enable it?
**Cluster Mode Enabled** shards data across multiple Redis nodes (up to 500 shards), allowing datasets larger than a single node's memory. Data is distributed by hash slot (16,384 total slots).  
Enable it when:
- Your dataset exceeds the largest available instance type (~400 GB)
- You need horizontal write scaling (writes distributed across shards)

Limitation: Multi-key commands must operate on keys in the same shard (use hash tags `{tag}` to force co-location: `{user123}:session`).

---

### Q8. How do you implement a distributed lock with Redis?
Use the `SET NX EX` pattern (**Redlock** algorithm for multi-node):
```
SET lock:resource123 random-token NX EX 30
```
- `NX`: Only set if key doesn't exist (atomically acquires the lock)
- `EX 30`: Auto-expire the lock in 30 seconds (prevents deadlock if holder crashes)

Release:
```lua
-- Lua script to atomically check and delete:
if redis.call("GET", KEYS[1]) == ARGV[1] then
    return redis.call("DEL", KEYS[1])
end
```
Only release if the token matches (prevents releasing another process's lock).

---

### Q9. What is the TTL strategy for preventing cache stampede (thundering herd)?
**Cache stampede** occurs when many requests hit the cache simultaneously for an expired key — all miss and hammer the DB.  
Mitigations:
1. **Jitter on TTL**: Add randomness to TTL — `TTL = base_ttl + random(0, 10)` — so keys expire at different times
2. **Probabilistic early expiration**: Before TTL expires, probabilistically refresh the cache ahead of time
3. **Mutex / distributed lock**: Only one request rebuilds the cache; others wait
4. **Background refresh**: A separate process refreshes cache before TTL expires

---

### Q10. How does Redis Pub/Sub work?
Publishers send messages to **channels**; subscribers receive all messages on subscribed channels in real time.
```
SUBSCRIBE chat:room123       # Subscribe to a channel
PUBLISH chat:room123 "Hello" # Publisher sends message
```
Messages are **not persistent** — if no subscriber is listening, the message is lost. Use for real-time notifications, live dashboards, or chat. For durable messaging, use **Redis Streams** (persistent, consumer groups, replayed on reconnect).

---

### Q11. When would you NOT use ElastiCache?
- **Data that rarely changes**: Caching provides no benefit; adds complexity
- **Data that must be 100% fresh**: Cache-aside introduces staleness; use strongly consistent DB reads instead
- **Very large, infrequently accessed datasets**: Cost of ElastiCache exceeds the benefit
- **Write-heavy workloads with no read benefit**: Cache won't help if every read is unique
- **Simple single-instance apps**: Overhead not justified; application-level caching (e.g., in-process LRU) is simpler

---

### Q12. What is ElastiCache's backup and recovery capability?
**Redis** supports:
- **Automatic daily snapshots** (RDB): Retained for 1–35 days. Creates a point-in-time snapshot.
- **Manual snapshots**: On-demand; stored in S3 indefinitely until deleted.
- **Restore**: Restore from snapshot to a new cluster.

**Memcached**: No persistence or backup. All data is lost on node replacement.

---

### Q13. How do you monitor ElastiCache performance?
Key CloudWatch metrics:
| Metric | What it Indicates |
|--------|------------------|
| `CacheHits` / `CacheMisses` | Cache effectiveness; aim for hit ratio > 90% |
| `Evictions` | Memory pressure; increase node size or add shards |
| `CurrConnections` | Connection count; watch for connection exhaustion |
| `EngineCPUUtilization` | Redis CPU (single-threaded); > 80% → scale up |
| `BytesUsedForCache` | Memory usage; add capacity before hitting limit |
| `ReplicationLag` | Replica lag; if high, replica is behind primary |

---

### Q14. What is the difference between TTL and eviction in Redis?
**TTL**: You explicitly set an expiry time on a key (`EXPIRE key 3600`). After expiry, Redis deletes the key.  
**Eviction**: When Redis runs out of memory, it evicts keys based on the **eviction policy**:
| Policy | Behavior |
|--------|---------|
| `allkeys-lru` | Evict least recently used key |
| `allkeys-lfu` | Evict least frequently used key |
| `volatile-lru` | Evict LRU key among keys with TTL set |
| `noeviction` | Return error on write when memory full (default) |
| `allkeys-random` | Evict random key |

Set `allkeys-lru` for a general-purpose cache; `noeviction` for session stores (you want OOM errors, not silent eviction).

---

### Q15. Design a caching architecture for a high-traffic product catalog.
```
[React App] → API Gateway → Lambda (Product Service)
                                    ↓
                    ┌──── [ElastiCache Redis]
                    │      Key: product:{id}
                    │      Value: JSON product data
                    │      TTL: 300 s
                    │
                    │ Cache Hit → return immediately (<1 ms)
                    │
                    └ Cache Miss → [RDS Aurora] → populate cache → return
                                                    ↓
                                        if miss rate > 10%:
                                        [Lambda: cache-warmer]
                                        pre-load top 1000 products
                                        scheduled via EventBridge

ElastiCache Setup:
  - Cluster Mode Disabled (single shard; ~50 GB catalog)
  - 1 primary (r7g.xlarge, 26 GB) + 2 replicas (separate AZs)
  - Multi-AZ with automatic failover
  - TTL jitter: 270-330 s (avoid stampede)
  - Eviction: allkeys-lru (cache as an LRU buffer)

Rate Limiting (separate Redis key space):
  - INCR rate:{userId}:{minute}
  - EXPIRE rate:{userId}:{minute} 60
  - Block if count > 100
```

---

## 3. Real-World Use Case: Session & API Response Caching

### Scenario
ShopWave has 50,000 concurrent users at peak. Database CPU spikes to 90% during sale events. 80% of product page reads are for the same 500 products.

### Architecture

```
[ALB] → [ECS: ShopWave API] × N
              │
    ┌─────────┼──────────────────┐
    │         │                  │
    ▼         ▼                  ▼
[Redis]   [Redis]             [Redis]
Sessions  API Cache           Rate Limiter
(Hash)    (String, TTL=60s)   (String, TTL=1s)
               │
               │ cache miss
               ▼
         [RDS Aurora MySQL]
         (primary read)

Redis Cluster:
  Primary: r7g.xlarge (26 GB) — us-east-1a
  Replica 1: r7g.xlarge — us-east-1b (auto-failover)
  Replica 2: r7g.xlarge — us-east-1c (additional read replica)
  → Read replica endpoint for API cache reads (scale reads)
  → Primary endpoint for session writes + rate limit writes
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Redis (not Memcached) | Sessions require persistence; rate limiting requires INCR; need Multi-AZ failover |
| Separate primary + read replica endpoints | Reads for product cache go to replicas; writes (sessions) go to primary |
| TTL jitter on product cache | Prevents stampede when all 500 hot products expire simultaneously |
| `allkeys-lru` eviction | Redis self-manages hot/cold items; memory is always used for most-requested data |
| Multi-AZ with auto failover | Session data survives AZ failure; < 30 s failover |

### Interview Narration (Whiteboard Script)
> "We put Redis in front of RDS to handle the 80% of reads that hit the same 500 products. API response cache with 60-second TTL cuts our RDS read load by 85% during peak. Sessions live in Redis with 30-minute TTL — any server can handle any request without sticky sessions. Rate limiting uses Redis INCR with 1-second TTL keys — atomic, fast, and self-expiring. The Redis cluster has two replicas in separate AZs; if the primary fails, we get automatic failover in under 30 seconds with the same connection endpoint."
