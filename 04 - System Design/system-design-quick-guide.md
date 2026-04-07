# System Design - 1-Page Quick Guide

> System design is the language of scale, and every engineer should speak it fluently.

---

## 1. Core Concepts

| Concept | What It Means |
|---|---|
| **CAP Theorem** | A distributed system can guarantee only 2 of 3: **Consistency**, **Availability**, **Partition Tolerance**. Pick based on your use case. |
| **Vertical Scaling** | Add more power (CPU, RAM) to a single machine. Simple but has a ceiling. |
| **Horizontal Scaling** | Add more machines. Complex but virtually unlimited scale. |
| **Latency vs Throughput** | Latency = time per request. Throughput = requests per unit time. Optimizing one can hurt the other. |
| **Consistency Models** | Strong (latest data always), Eventual (data converges over time), Causal (respects order of operations). |
| **Partitioning / Sharding** | Split data across nodes by a shard key to distribute load and storage. |
| **Replication** | Copy data to multiple nodes for fault tolerance and read scaling (leader-follower, multi-leader, leaderless). |

---

## 2. Key Components & Tools

| Component | Purpose |
|---|---|
| **Load Balancer** | Distributes traffic across servers (Round Robin, Least Connections, IP Hash). Prevents single points of failure. |
| **CDN** | Caches static content at edge locations close to users. Reduces latency globally. |
| **API Gateway** | Single entry point for clients. Handles routing, auth, rate limiting, request aggregation. |
| **Message Queue** | Decouples producers from consumers for async processing (Kafka, RabbitMQ, SQS). |
| **Cache** | Store frequently accessed data in memory (Redis, Memcached). Read-through, write-through, write-behind strategies. |
| **Databases** | SQL (ACID, joins, structured) vs NoSQL (flexible schema, horizontal scale). Choose based on access patterns. |
| **Object Store** | Store unstructured data like images, videos, backups (S3, Azure Blob). |
| **Containers & Orchestration** | Package apps in containers (Docker), orchestrate with Kubernetes for scaling, self-healing, rolling deploys. |
| **Service Mesh** | Manages service-to-service communication (Istio, Linkerd). Handles mTLS, retries, observability. |

---

## 3. Reliability & Fault Tolerance

| Practice | Description |
|---|---|
| **Health Checks** | Periodically verify that services are alive and responsive. Liveness vs readiness probes. |
| **Retries with Backoff** | Retry failed requests with exponential backoff + jitter to avoid thundering herd. |
| **Failover** | Automatically switch to a standby system when the primary fails (active-passive, active-active). |
| **Chaos Testing** | Intentionally inject failures (kill pods, add latency) to validate resilience. Netflix Chaos Monkey approach. |
| **Redundancy** | No single point of failure. Replicate critical components across zones/regions. |
| **Graceful Degradation** | Serve partial results or fallback responses when a dependency is down. |
| **Idempotency** | Design operations so repeating them produces the same result. Critical for retries and exactly-once semantics. |

---

## 4. Monitoring & Observability

| Pillar | What It Captures |
|---|---|
| **Metrics** | Numeric measurements over time -- CPU, memory, request rate, error rate. Tools: Prometheus, Datadog. |
| **Logging** | Structured event records for debugging. Centralize with ELK stack or Loki. Use correlation IDs. |
| **Tracing** | Follow a request across services end-to-end. Tools: Jaeger, Zipkin, OpenTelemetry. |
| **Dashboards** | Visualize system health at a glance. Grafana, Datadog. Set up alerts on SLO breaches. |
| **SLIs / SLOs / SLAs** | SLI = what you measure, SLO = target you set, SLA = contract with users. Drive reliability decisions. |

---

## 5. Design Patterns

| Pattern | When to Use |
|---|---|
| **Circuit Breaker** | Stop calling a failing service. Open -> Half-Open -> Closed states. Prevents cascade failures. |
| **Bulkhead** | Isolate components so one failure doesn't sink the whole system. Separate thread pools/resources per service. |
| **BFF (Backend for Frontend)** | Dedicated backend per client type (web, mobile, IoT). Tailors API responses to client needs. |
| **Event-Driven Architecture** | Services react to events rather than direct calls. Enables loose coupling and async processing. |
| **CQRS** | Separate read and write models. Optimize each independently. Often paired with event sourcing. |
| **Saga Pattern** | Manage distributed transactions as a sequence of local transactions with compensating actions on failure. |
| **Sidecar Pattern** | Attach helper processes alongside main service (logging agent, proxy). Core of service mesh architecture. |
| **Strangler Fig** | Incrementally migrate a legacy system by routing traffic to new services piece by piece. |

---

## Bonus: Foundational Terms to Know

| Term | Quick Definition |
|---|---|
| **Rate Limiting** | Cap the number of requests a client can make in a time window. Protects against abuse and overload. |
| **Cold Start** | Initial delay when a new instance spins up (especially serverless). Mitigate with warm pools or provisioned concurrency. |
| **Session Stickiness** | Route a user's requests to the same server. Useful for stateful apps but hurts load distribution. |
| **Service Discovery** | Automatically detect and connect to available service instances (Consul, Eureka, DNS-based). |
| **Auto-Scaling** | Dynamically add/remove instances based on metrics (CPU, queue depth). Scale out on demand, scale in to save cost. |
| **Back Pressure** | When a system is overwhelmed, push back on producers to slow down input rate. Prevents resource exhaustion. |
| **Blue-Green Deployment** | Run two identical environments. Switch traffic from blue (old) to green (new) for zero-downtime deploys. |
| **Canary Release** | Roll out changes to a small % of users first. Monitor, then gradually increase traffic. |

---

## How to Approach a System Design Interview

```
1. CLARIFY       -> Ask requirements, constraints, scale (users, data, QPS)
2. ESTIMATE      -> Back-of-envelope math (storage, bandwidth, servers)
3. HIGH-LEVEL    -> Draw core components and data flow
4. DEEP DIVE     -> Detail 1-2 critical components (DB schema, API design, caching)
5. TRADE-OFFS    -> Justify choices (SQL vs NoSQL, push vs pull, sync vs async)
6. BOTTLENECKS   -> Identify and address single points of failure, hotspots
```

---

*Keep this guide handy. System design is not about memorizing -- it's about understanding trade-offs and knowing which tool fits which problem.*
