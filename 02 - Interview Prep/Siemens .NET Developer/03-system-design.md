# System Design & Architecture — Siemens Interview

## Q1. Design a Distributed Time Series Database (Prepfully verified)

**Relevant to Siemens:** Industrial IoT sensors generate massive time-series data.

- **Data model:** `timestamp + device_id + metric_name + value`
- **Write-optimized storage:** append-only logs, LSM trees
- **Partitioning:** by time range (hot/cold data) + device_id hash
- **Replication:** leader-follower for reads, quorum writes for consistency
- **Compression:** delta encoding for timestamps, gorilla compression for values
- **Retention policies:** auto-downsample old data (1s → 1min → 1hr)
- **Query patterns:** range queries, aggregations, latest-value lookups
- **Tech choices:** InfluxDB, TimescaleDB, or Kafka + Cassandra

## Q2. Design a Distributed Lock Service (Prepfully verified)

- **Purpose:** coordinate access to shared resources across microservices
- **Options:** Redis (Redlock), ZooKeeper, etcd
- **Requirements:** mutual exclusion, deadlock freedom, fault tolerance
- **Redis implementation:** `SET resource_key unique_value NX PX 30000`
- **Fencing tokens:** monotonically increasing to prevent stale locks
- **Trade-offs:** CP vs AP — prefer CP for locks (CAP theorem)

## Q3. Design a Real-Time Customer Feedback System (Prepfully verified)

- **Ingestion:** Kafka / Event Hub for real-time stream
- **Processing:** Apache Flink / Azure Stream Analytics for NLP
- **Storage:** Elasticsearch (search) + PostgreSQL (structured)
- **ML Pipeline:** sentiment analysis, topic extraction
- **Dashboard:** real-time metrics via WebSocket / SignalR
- **Scale:** partition by region, horizontal scaling of consumers

## Q4. Design a Payment System

- **Idempotency:** unique payment ID to prevent double charges
- **State machine:** PENDING → PROCESSING → COMPLETED / FAILED
- **Event sourcing:** immutable log of all payment events
- **Saga pattern:** distributed transactions across services
- **Security:** PCI DSS, tokenization, encryption at rest/transit
- **Reconciliation:** batch job to match records with payment gateway

## Q5. Microservices Architecture (Frequently asked)

**Key topics to cover:**
- Monolith vs Microservices trade-offs
- API Gateway pattern (routing, auth, rate limiting)
- Service discovery (Consul, Eureka)
- Database per service vs shared database
- Event-driven communication (RabbitMQ, Kafka, Azure Service Bus)
- CI/CD pipelines (asked at Siemens specifically)

**Circuit Breaker with Polly (.NET):**
```csharp
var policy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

**Saga pattern:** orchestrated (central coordinator) vs choreographed (event-based). Use for operations spanning multiple services where traditional ACID transactions aren't possible.

## Q6. Design Patterns (Technical Round 2)

**Singleton (thread-safe):**
```csharp
public sealed class DeviceManager
{
    private static readonly Lazy<DeviceManager> _instance = new(() => new DeviceManager());
    public static DeviceManager Instance => _instance.Value;
    private DeviceManager() { }
}
```

**Factory:**
```csharp
public interface ISensor { double Read(); }
public class SensorFactory
{
    public static ISensor Create(string type) => type switch
    {
        "temperature" => new TemperatureSensor(),
        "pressure" => new PressureSensor(),
        _ => throw new ArgumentException($"Unknown sensor: {type}")
    };
}
```

**Observer (built into C# via events):**
```csharp
public class SensorMonitor
{
    public event Action<double> ThresholdExceeded;
    public void CheckReading(double value)
    {
        if (value > 100) ThresholdExceeded?.Invoke(value);
    }
}
```

**Repository pattern:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```
