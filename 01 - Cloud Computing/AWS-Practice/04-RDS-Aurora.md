# AWS RDS & Aurora — Managed Relational Databases

## 1. Core Concepts

### What is RDS?
Amazon RDS is a **managed relational database service** that automates provisioning, patching, backups, Multi-AZ failover, and read replica creation. You pick the engine; AWS manages the undifferentiated heavy lifting.

### Supported Engines
| Engine | Notes |
|--------|-------|
| **MySQL** | Most popular open-source RDBMS |
| **PostgreSQL** | Advanced features, JSONB, extensions |
| **MariaDB** | MySQL fork — community-driven |
| **Oracle** | BYOL or License Included |
| **SQL Server** | BYOL or License Included |
| **Aurora MySQL** | AWS-optimized MySQL — 5× faster |
| **Aurora PostgreSQL** | AWS-optimized PostgreSQL — 3× faster |

### RDS Multi-AZ
- Synchronous replication to a **standby** instance in another AZ
- Automatic failover in 60–120 seconds (DNS TTL flip)
- Standby is **NOT readable** — pure HA, not for reads
- Enable for all production databases

### RDS Read Replicas
- Asynchronous replication (slight lag)
- **Readable** — offload reporting/analytics queries
- Up to 15 read replicas per primary (Aurora); 5 for MySQL/PostgreSQL/MariaDB
- Can be in the **same region, different region, or different account**
- Can be promoted to standalone primary (for migrations or DR)
- Cross-region read replicas: useful for local reads, DR

### Multi-AZ vs Read Replicas
| | Multi-AZ | Read Replicas |
|--|----------|---------------|
| Purpose | High Availability | Read Scalability |
| Replication | Synchronous | Asynchronous |
| Readable? | No (standby) | Yes |
| Failover | Automatic (DNS) | Manual promotion |
| Cross-region? | No (same region) | Yes |
| Cost | ~2× storage cost | Additional instance cost |

### Automated Backups & Snapshots
- **Automated Backups**: Daily snapshot + transaction logs. Retention: 0–35 days. Point-in-time recovery to any second within retention window.
- **Manual Snapshots**: Persist until explicitly deleted. Use for long-term retention beyond 35 days.
- **Snapshots don't expire** — they persist even after you delete the DB instance.

### RDS Storage
| Type | Throughput | IOPS | Use Case |
|------|-----------|------|----------|
| **gp2** | 3 IOPS/GB | Max 16,000 | General purpose (legacy) |
| **gp3** | Configurable | Max 64,000 | General purpose (recommended) |
| **io1/io2** | Configurable | Max 256,000 | IOPS-intensive DBs |
| **Magnetic** | Low | Low | Legacy, avoid |

**Storage Auto Scaling**: Automatically increases storage when free space is low.

### RDS Proxy
- Fully managed connection pooler between Lambda/ECS and RDS
- Reduces connection overhead — Lambda can open 1,000s of connections; RDS Proxy maintains a small pool to the DB
- Improves failover time (~66% faster than direct connection on Multi-AZ failover)
- Enforces IAM authentication to the database

### Security
- **Encryption at rest**: KMS (must enable at creation time — can't enable post-creation without snapshot restore)
- **Encryption in transit**: SSL/TLS; enforce with DB parameter groups
- **IAM authentication**: Token-based short-lived credentials (MySQL, PostgreSQL, Aurora)
- **Security Groups**: Control network access at VPC level
- **No public IP**: Always deploy in private subnets; connect via bastion host or SSM Session Manager

---

## 2. Amazon Aurora

### What Makes Aurora Different?
Aurora uses a **distributed, fault-tolerant shared storage layer** (6 copies across 3 AZs) that's separate from the compute layer. The storage auto-scales in 10 GB increments up to 128 TB.

### Aurora Architecture
```
         ┌─────────────────────────────────────────┐
         │            Aurora Storage Layer           │
         │  6 copies of data across 3 AZs (2 per AZ)│
         │  Write quorum: 4/6 | Read quorum: 3/6     │
         └──────────────┬──────────────────────────┘
                        │
        ┌───────────────┼──────────────────┐
        ▼               ▼                  ▼
  [Primary Instance]  [Read Replica 1]  [Read Replica 2]
  Read + Write        Read-only          Read-only
  (Writer endpoint)   └────────────────────────────────┘
                             (Reader endpoint — auto load-balanced)
```

### Aurora vs RDS MySQL
| Feature | Aurora MySQL | RDS MySQL |
|---------|-------------|-----------|
| Performance | 5× faster | Baseline |
| Read Replicas | Up to 15 | Up to 5 |
| Replication lag | ~10ms (shared storage) | Seconds |
| Failover | ~30 seconds | 60–120 seconds |
| Storage Auto-scale | Yes (to 128 TB) | Manual resize |
| Storage copies | 6 across 3 AZs | 2 (primary + standby) |
| Serverless | Aurora Serverless v2 | No |
| Cost | ~20% more | Less |

### Aurora Serverless v2
- Automatically scales DB capacity from 0.5 ACU to 128 ACU in fine-grained increments
- Scales in seconds based on actual load
- Scales to zero (v1 behavior) — add minimum ACU > 0 for instant response
- Pay per ACU-hour consumed
- Ideal for: variable workloads, dev/test, multi-tenant SaaS

### Aurora Global Database
- Replicates to up to 5 secondary regions with ~1 second lag
- Primary region: Read + Write
- Secondary regions: Read-only (can promote to primary in <1 minute for DR)
- Use case: Global apps needing local reads, cross-region DR with near-zero RPO

### Aurora Multi-Master
- All instances can read AND write
- Conflict resolution at storage layer
- Use case: Multi-region write capability (limited support — check current AWS docs)

---

## 3. Interview Questions & Answers

### Q1. What is the difference between RDS Multi-AZ and Read Replicas?
**Multi-AZ**: Provides **high availability**. Synchronous replication to a standby in another AZ. Standby is not accessible — failover is automatic and takes 60–120 seconds. Handles AZ failures and maintenance.

**Read Replicas**: Provide **read scalability**. Asynchronous replication — slight lag. Replicas are accessible for read queries. Reduce load on the primary. Can be promoted to primary (manual).

**Key distinction**: Multi-AZ = HA; Read Replicas = Performance. You can have both simultaneously.

---

### Q2. What is Aurora and how does it differ from RDS MySQL?
Aurora is AWS's cloud-native relational DB engine compatible with MySQL and PostgreSQL. Key differences:
- **Storage**: Shared distributed storage (6 copies/3 AZs) instead of replication; storage automatically scales to 128 TB
- **Performance**: 5× faster than MySQL, 3× faster than PostgreSQL for the same workload
- **Replication lag**: ~10ms (shared storage, no traditional replication) vs seconds for RDS
- **Read replicas**: Up to 15 (Aurora) vs 5 (MySQL)
- **Failover**: ~30s vs 60–120s for RDS Multi-AZ

Cost is ~20% higher than RDS but often justified by performance and HA improvements.

---

### Q3. What is RDS Proxy and why is it critical for serverless architectures?
RDS Proxy is a fully managed connection pooler that sits between your application and RDS/Aurora.

**Problem**: Lambda can scale to thousands of concurrent instances, each opening a DB connection. RDS has connection limits (e.g., db.t3.medium: ~70 connections). Without Proxy, you get "too many connections" errors.

**Solution**: RDS Proxy maintains a pool of persistent connections to the DB and multiplexes thousands of application connections onto them. Benefits:
- Handles Lambda spikes without exhausting DB connections
- 66% faster failover (Proxy reconnects to new primary transparently)
- Enforces IAM auth to the database

---

### Q4. How does point-in-time recovery work in RDS?
RDS continuously backs up transaction logs to S3. With **automated backups** enabled (retention 1–35 days), you can restore to any second within the retention period. The process:
1. RDS restores the latest automated backup snapshot
2. Applies transaction logs up to the specified point in time
3. Creates a **new DB instance** (doesn't overwrite existing)

After restore, update your application connection string to point to the new instance.

---

### Q5. Can you encrypt an existing unencrypted RDS database?
Not directly. You can't enable encryption on an existing unencrypted instance. The workaround:
1. Create a snapshot of the unencrypted instance
2. Copy the snapshot with encryption enabled (specifying a KMS key)
3. Restore a new DB instance from the encrypted snapshot
4. Update application connection string
5. Delete the old unencrypted instance

---

### Q6. What is Aurora Serverless v2 and when should you use it?
Aurora Serverless v2 automatically scales database capacity based on application demand, measured in **Aurora Capacity Units (ACUs)**. It scales up/down in increments of 0.5 ACU (1 ACU = ~2 GB memory).

Best for:
- **Variable workloads** — dev/test, staging environments
- **Unpredictable traffic** — new applications where load is unknown
- **Multi-tenant SaaS** — many small tenants, pay only for what you use

Not ideal for: Predictable, steady-state production workloads where Reserved instances are more cost-effective.

---

### Q7. What is an Aurora Global Database?
Aurora Global Database spans multiple AWS regions. One primary region (read/write) replicates to up to 5 secondary regions (read-only) with < 1 second lag using dedicated replication infrastructure (not Aurora's standard replication).

Use cases:
- Global applications needing local reads in multiple regions (low latency)
- Cross-region DR: Promote a secondary to primary in < 1 minute (RPO ~1s, RTO ~1min)

---

### Q8. How do you handle database schema migrations with zero downtime?
1. **Expand-contract pattern**: Add new columns as nullable, backfill data, then switch app to use new column, finally drop old column — no locking.
2. **Blue/Green deployments**: RDS Blue/Green Deployments creates a staging environment (green), apply schema changes and test, then switch with minimal downtime (<1 min).
3. **Tools**: Flyway, Liquibase, or AWS Schema Conversion Tool (SCT) for cross-engine migrations.
4. For read-heavy tables, create the new schema on a read replica, promote it, redirect traffic.

---

### Q9. What are RDS Performance Insights?
Performance Insights is a DB performance monitoring feature that shows database load as a chart of **Average Active Sessions** broken down by SQL query, wait type, user, host, or application. This helps you identify:
- Top SQL queries consuming the most DB time
- Wait events causing slowdowns (lock waits, I/O wait, CPU)
- Whether the bottleneck is the DB or the application

Free 7-day retention; paid for longer history.

---

### Q10. What is the difference between Aurora Replica and RDS Read Replica?
| | Aurora Replica | RDS Read Replica |
|--|---------------|-----------------|
| Replication | Shared storage (no replication lag) | Async (seconds of lag) |
| Failover | Automatic, ~30s, no data loss | Manual promotion, potential data loss |
| Max replicas | 15 | 5 (MySQL/PostgreSQL) |
| Cross-region? | Via Global Database | Yes, natively |
| In-cluster? | Yes — same storage cluster | No — separate storage |

---

### Q11. What is the Aurora storage layer and why is it significant?
Aurora uses a **distributed, shared storage layer** across 6 storage nodes in 3 AZs. Write quorum requires 4/6 acknowledgments; read quorum requires 3/6. This means:
- Storage can survive 2 AZ failures and still have a read quorum
- Storage can survive 1 AZ + 1 node failure and still have a write quorum
- No data loss on compute instance failure (storage is separate)
- Storage auto-scales — no need to pre-provision disk

---

### Q12. How do you migrate from MySQL to Aurora MySQL?
Options:
1. **DMS (Database Migration Service)**: Minimal downtime — continuous replication, then cutover
2. **Create Aurora Read Replica from MySQL RDS**: Promote when replication lag is zero — simplest if already on RDS
3. **mysqldump + import**: Simplest but requires downtime
4. **AWS Schema Conversion Tool (SCT)**: For cross-engine migrations (Oracle → Aurora PostgreSQL)

For production: DMS with ongoing replication is preferred (near-zero downtime).

---

### Q13. What happens during an RDS Multi-AZ failover?
1. Primary instance becomes unavailable (AZ failure, maintenance, hardware issue)
2. RDS detects failure and updates the DNS CNAME (endpoint) to point to the standby
3. Standby promotes to primary
4. Clients reconnect — DNS TTL is 5 seconds for RDS
5. Total time: 60–120 seconds

**Application impact**: Connection errors during DNS propagation. Mitigate with retry logic, connection pools, and RDS Proxy (which handles reconnection transparently).

---

### Q14. Explain RDS IAM Database Authentication.
Instead of username/password, your application gets a **temporary auth token** from IAM (valid 15 minutes), generated using `generate-db-auth-token`. The DB validates the token using the IAM policy.

Benefits:
- No password stored in code or config
- Short-lived credentials — stolen token expires in 15 minutes
- Audit trail in CloudTrail for every connection attempt
- Rotate credentials by IAM policy change, not DB password change

Supported: MySQL, PostgreSQL, Aurora MySQL, Aurora PostgreSQL.

---

### Q15. What is the maximum storage for RDS and Aurora?
| Engine | Max Storage |
|--------|------------|
| MySQL, MariaDB | 64 TB (io1/gp3) |
| PostgreSQL | 64 TB |
| Oracle | 64 TB |
| SQL Server | 16 TB |
| **Aurora** | **128 TB** (auto-scales in 10 GB increments) |

Aurora's advantage: Storage grows automatically; you don't need to predict and pre-provision.

---

## 4. Real-World Use Case: Fintech Transactional Database

### Scenario
A fintech company needs a database tier that:
- Processes 50,000 transactions/second at peak (end-of-quarter)
- Must never lose committed transactions (RPO = 0)
- Global users need low-latency reads for account balances (US, EU, APAC)
- Must meet SOC 2 and PCI-DSS compliance requirements
- DR: RTO < 1 minute, RPO < 1 second

### Architecture

```
                    ┌──────────────────────────────────┐
                    │       AWS Global Accelerator      │
                    │      (static IPs, edge routing)   │
                    └───────────────┬──────────────────┘
              ┌───────────────────┬─┘
              ▼                   ▼
    ┌─────────────────┐   ┌──────────────────┐
    │  us-east-1      │   │  eu-west-1       │
    │  PRIMARY        │   │  SECONDARY       │
    │                 │   │                  │
    │ Aurora Cluster  │──►│ Aurora Global DB │
    │  Primary (R+W)  │   │  Read-only       │
    │  3 Replicas (R) │   │  < 1s lag        │
    │                 │   │                  │
    │  Writer: 1      │   │  3 Read Replicas │
    │  Endpoint       │   │  Endpoint        │
    └────────┬────────┘   └──────────────────┘
             │
    ┌────────▼────────┐
    │   RDS Proxy     │  ← Connection pooling
    │  (per AZ)       │
    └────────┬────────┘
             │
    ┌────────▼────────────────────────────────────┐
    │              Application Tier                │
    │  ECS Fargate containers (auto-scaling)       │
    │  Read ops → Reader endpoint (load balanced) │
    │  Write ops → Writer endpoint                │
    └─────────────────────────────────────────────┘

    CloudWatch Performance Insights → SNS alarm → PagerDuty
    AWS Backup → Cross-region backup vault (compliance)
    AWS CloudTrail → Audit all DB API calls
```

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| Aurora Global Database | RPO ~1s, RTO ~1min for cross-region DR; local reads in EU reduce latency by 60% |
| RDS Proxy | ECS containers scale independently of DB; Proxy prevents connection exhaustion |
| Aurora Serverless v2 for dev/test | Non-prod environments scale to 0.5 ACU overnight — saves 80% on dev DB costs |
| Encryption with KMS CMK | Compliance requires customer-managed keys; CloudTrail shows every decrypt operation |
| IAM auth tokens | Eliminate DB passwords from app config; short-lived tokens reduce credential risk |
| Performance Insights (1-year retention) | Compliance audit requirement for query history |
| AWS Backup cross-region | PCI-DSS requires backups in geographically separate location |
| Aurora parameter group tuning | `innodb_flush_log_at_trx_commit=1` ensures ACID compliance for financial transactions |

### Interview Narration (White-board Script)
> "For a fintech transactional system the top requirements are zero data loss and global availability. I'd use Aurora Global Database — the primary cluster in us-east-1 handles all writes, and read replicas in eu-west-1 and ap-southeast-1 serve local balance lookups with sub-second replication lag. If the primary region goes down, I can promote the EU secondary in under a minute — that's RPO of ~1 second and RTO of ~1 minute. I separate reads and writes at the application layer using Aurora's reader endpoint. RDS Proxy sits in between to multiplex the ECS container connections — without it, thousands of containers would overwhelm Aurora's connection limit. Everything is encrypted with a customer-managed KMS key so we have full audit trails for PCI-DSS."
