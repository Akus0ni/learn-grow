# Capstone Architecture — Full-Stack E-Commerce Platform on AWS

> This document demonstrates a **production-grade, real-world architecture** that combines all 10 AWS services covered in this study folder. Use this as your whiteboard demo for interviews.

---

## 1. Business Requirements

A B2C e-commerce startup called **ShopWave** needs to:
- Handle 5,000 concurrent users normally; scale to 200,000 during Black Friday
- Process 100,000 orders per day
- Serve customers globally (US, EU, APAC)
- Never lose an order (RPO = 0 for orders)
- 99.95% availability SLA
- SOC 2 compliant (audit trail, encryption, access control)
- Team of 8 engineers — cannot spend time managing servers

---

## 2. Architecture Overview

```
┌───────────────────────────────────────────────────────────────────────────────┐
│                              GLOBAL EDGE LAYER                                 │
│                                                                                │
│  Users Worldwide                                                               │
│      │                                                                         │
│  [Route 53] ← Latency-based + Health check routing                           │
│      │                                                                         │
│  [AWS WAF] ← OWASP Top 10, rate limiting, geo-blocking                       │
│      │                                                                         │
│  [CloudFront] ← CDN: static assets, API caching, Lambda@Edge auth            │
│      │                                                                         │
└──────┼────────────────────────────────────────────────────────────────────────┘
       │
┌──────┼────────────────────────────────────────────────────────────────────────┐
│      │                        API LAYER (us-east-1)                            │
│      │                                                                         │
│  [API Gateway REST API]                                                        │
│  ├── /products/*     → Lambda: Product Service                                │
│  ├── /cart/*         → Lambda: Cart Service                                   │
│  ├── /orders/*       → Lambda: Order Service                                  │
│  ├── /users/*        → Lambda: User Service                                   │
│  └── /search/*       → ECS: Search Service (Fargate)                         │
│                                                                                │
│  Auth: Cognito User Pools (mobile/web users)                                  │
│  Throttle: 10,000 RPS account-level; 500 RPS on /orders per usage plan       │
│  Cache: GET /products/* 300s TTL                                              │
│                                                                                │
└──────┬────────────────────────────────────────────────────────────────────────┘
       │
┌──────┼────────────────────────────────────────────────────────────────────────┐
│      │                    COMPUTE LAYER                                        │
│      │                                                                         │
│  Lambda Functions (Fargate-like, serverless):                                 │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐                │
│  │ Product Service │ │  Cart Service   │ │  Order Service  │                │
│  │ 256MB, 10s      │ │ 128MB, 5s       │ │ 512MB, 30s      │                │
│  │ Prov. Concurr.  │ │                 │ │ Prov. Concurr.  │                │
│  └────────┬────────┘ └────────┬────────┘ └────────┬────────┘                │
│           │                   │                   │                           │
│  ECS Fargate Services:                                                        │
│  ┌─────────────────────────────────────────┐                                 │
│  │ Search Service (OpenSearch client)       │                                 │
│  │ 2 vCPU, 4GB, 2-20 tasks, Fargate       │                                 │
│  └─────────────────────────────────────────┘                                 │
│                                                                                │
└──────┬────────────────────────────────────────────────────────────────────────┘
       │
┌──────┼────────────────────────────────────────────────────────────────────────┐
│      │                   ASYNC PROCESSING LAYER                               │
│      │                                                                         │
│  [SQS: order-queue]        [SQS: email-queue]      [SQS: inventory-queue]    │
│       │                         │                          │                  │
│  [Lambda: OrderProcessor]  [Lambda: EmailSender]  [Lambda: InventoryUpdater]│
│  (idempotent, batch 10)    (SES integration)      (DynamoDB conditional)    │
│       │                                                                        │
│  [SQS: order-dlq] ← Failed orders for investigation                         │
│  [CloudWatch Alarm: DLQ depth > 0 → PagerDuty]                              │
│                                                                                │
│  [EventBridge Scheduler]                                                      │
│       ├── Daily: Lambda Report Generator → S3 reports bucket                 │
│       └── Hourly: Lambda Inventory Sync → on-premises ERP (Direct Connect)  │
│                                                                                │
└──────┬────────────────────────────────────────────────────────────────────────┘
       │
┌──────┼────────────────────────────────────────────────────────────────────────┐
│      │                      DATA LAYER                                         │
│                                                                                │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │                    VPC: 10.0.0.0/16                                       │ │
│  │   ┌───────────────────────────────────────────────────────────────────┐  │ │
│  │   │ Private Subnets (AZ-a + AZ-b)                                      │  │ │
│  │   │                                                                     │  │ │
│  │   │  [RDS Proxy] ←──────────────────────────────────────────────────┐  │  │ │
│  │   │       │                                                          │  │  │ │
│  │   │  [Aurora MySQL — Primary (R+W)]   [Aurora — Read Replicas x3]   │  │  │ │
│  │   │   Writer endpoint                  Reader endpoint               │  │  │ │
│  │   │   Multi-AZ (6 copies/3 AZs)        Load balanced reads          │  │  │ │
│  │   │   Global DB → eu-west-1 (< 1s lag)                             │  │  │ │
│  │   │                                                                  │  │  │ │
│  │   │  [DynamoDB] — Sessions, Cart, Product catalog cache            │  │  │ │
│  │   │   Global Tables (us-east-1, eu-west-1)                         │  │  │ │
│  │   │   DAX cluster for sub-ms reads                                  │  │  │ │
│  │   │                                                                  │  │  │ │
│  │   │  [ElastiCache Redis] — API response cache, rate limiting        │  │  │ │
│  │   │   Cluster mode, 2 shards × 2 replicas                          │  │  │ │
│  │   │                                                                  │  │  │ │
│  │   │  VPC Endpoints → S3, DynamoDB, SSM, ECR, Secrets Manager       │  │  │ │
│  │   └───────────────────────────────────────────────────────────────┘  │  │ │
│  └──────────────────────────────────────────────────────────────────────┘ │ │
│                                                                                │
│  [S3 Buckets]                                                                 │
│  ├── product-images/ (Standard → IA after 90d) + CloudFront origin          │
│  ├── order-invoices/ (Standard, Object Lock 7yr, SSE-KMS)                   │
│  ├── reports/        (Standard-IA, 1-year retention)                         │
│  └── logs/           (Standard → Glacier after 30d)                         │
│                                                                                │
└────────────────────────────────────────────────────────────────────────────────┘
┌───────────────────────────────────────────────────────────────────────────────┐
│                        OBSERVABILITY LAYER                                     │
│                                                                                │
│  CloudWatch:                                                                   │
│  ├── Metrics: API GW, Lambda, ECS, RDS, SQS, DynamoDB                       │
│  ├── Custom Metrics (EMF): OrdersPerMin, CheckoutErrors, CartAbandonment     │
│  ├── Alarms: 15 critical alarms → PagerDuty; 30 warnings → Slack            │
│  ├── Composite Alarm: CriticalIncident = ErrorRate AND HighLatency           │
│  ├── Dashboard: NOC view (request rate, error rate, latency, DB health)      │
│  └── Logs: 14-day retention → Kinesis Firehose → S3 (1 year)               │
│                                                                                │
│  X-Ray: Active tracing on all Lambda + API GW + ECS services                 │
│  ├── Service Map: real-time topology                                           │
│  └── Sampling: 5% normal, 100% on errors                                     │
│                                                                                │
│  CloudTrail → Security Account S3 (multi-region, integrity validation)       │
│  Synthetics: Checkout canary every 5 minutes                                 │
│                                                                                │
└───────────────────────────────────────────────────────────────────────────────┘
┌───────────────────────────────────────────────────────────────────────────────┐
│                      SECURITY & IAM LAYER                                      │
│                                                                                │
│  AWS Organizations:                                                            │
│  ├── Management Account (billing)                                             │
│  ├── Security Account (CloudTrail, GuardDuty, Security Hub)                  │
│  ├── Prod Account (ShopWave production — SCP: no IAM users, us-east-1 only) │
│  └── Dev Account (developers, sandboxed)                                     │
│                                                                                │
│  IAM Identity Center + Okta SSO:                                             │
│  ├── Developers: PowerUserAccess on Dev; ReadOnly on Prod                    │
│  └── DevOps: DeployRole on Prod (restricted by Permission Boundary)         │
│                                                                                │
│  GitHub Actions → OIDC → AssumeRoleWithWebIdentity (no stored keys)         │
│                                                                                │
│  Secrets: All in Secrets Manager (auto-rotation every 30 days)               │
│  Encryption: SSE-KMS on all data at rest; TLS 1.2+ in transit               │
│  GuardDuty: Threat detection (crypto mining, unusual API calls)              │
│  Security Hub: Centralized findings from GuardDuty, Inspector, Macie        │
│                                                                                │
└───────────────────────────────────────────────────────────────────────────────┘
┌───────────────────────────────────────────────────────────────────────────────┐
│                     CI/CD & IaC LAYER                                          │
│                                                                                │
│  GitHub → CodePipeline → CodeBuild → CloudFormation (CDK)                   │
│                                                                                │
│  CDK Stacks:                                                                   │
│  ├── NetworkStack: VPC, subnets, SGs, VPC endpoints                          │
│  ├── DataStack: Aurora, DynamoDB, ElastiCache, S3 buckets                   │
│  ├── ComputeStack: Lambda functions, ECS cluster, ECR                        │
│  ├── APIStack: API Gateway, Cognito, WAF, CloudFront                         │
│  └── ObservabilityStack: CloudWatch alarms, dashboards, X-Ray                │
│                                                                                │
│  Deployment: Blue/Green via CodeDeploy                                        │
│  ├── Lambda: weighted aliases (10% → canary → 100%)                         │
│  └── ECS: CodeDeploy blue/green with ALB listener shifting                  │
│                                                                                │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. How All 10 Services Work Together

### Order Flow (Happy Path) — Step by Step

```
1. Customer clicks "Buy Now" on product page
   CloudFront: serves cached product page (static JS/HTML from S3)

2. Mobile app calls POST /orders (with Cognito JWT in Authorization header)
   Route 53: resolves api.shopwave.com → API Gateway
   WAF: checks OWASP rules, rate limits (allow)
   API Gateway: validates Cognito JWT token
   Throttling: allows (< 500 RPS per usage plan)

3. API Gateway invokes Order Lambda (synchronous, Provisioned Concurrency)
   Lambda (Order Service):
   → Validates order payload
   → Reads inventory from DynamoDB (via DAX — sub-ms)
   → Writes order to Aurora (via RDS Proxy — uses connection pool)
   → Publishes message to SQS order-queue
   → Publishes to SQS email-queue
   → Returns 202 Accepted + orderId

4. SQS → Lambda (OrderProcessor) — async, batch=10
   → Updates order status in Aurora
   → Decrements inventory in DynamoDB (conditional write — prevents oversell)
   → Stores invoice PDF in S3 (order-invoices/ with Object Lock)
   → Publishes order.confirmed event to EventBridge

5. EventBridge routes order.confirmed:
   → Email Lambda: SES sends confirmation email (with S3 pre-signed URL for invoice)
   → Analytics Lambda: Writes to Kinesis Firehose → S3 data lake

6. CloudWatch monitors the entire flow:
   X-Ray trace: API GW → Order Lambda → Aurora → SQS → OrderProcessor → DynamoDB
   If OrderProcessor fails → DLQ → CloudWatch Alarm → PagerDuty
   Synthetics canary: every 5 minutes checks the checkout endpoint
```

### Black Friday Scaling Behavior

| Time | Normal Load | Black Friday | How AWS Handles It |
|------|------------|--------------|-------------------|
| T-0: Sale starts | 1,000 RPS | 20,000 RPS | API Gateway scales automatically up to account quotas (quota increases available) |
| T+0: Order spike | 50/min | 5,000/min | Lambda scales from 100 → 3,000 concurrent (Provisioned Concurrency handles first wave) |
| T+5: DB pressure | 200 queries/s | 8,000/s | RDS Proxy absorbs connection spike; Read Replicas serve all GET queries |
| T+10: Queue buildup | SQS queue ~0 | SQS depth 200,000 | Lambda (OrderProcessor) scales to 1,000 concurrent; processes backlog |
| T+60: Settled | Back to normal | 3,000 RPS | Auto Scaling scales Lambda/ECS down; DynamoDB On-Demand reduces consumed capacity |

---

## 4. Cost Optimization Strategies

| Service | Strategy | Monthly Savings |
|---------|----------|----------------|
| EC2 (search service) | Reserved Instances for baseline; Spot for batch | ~50% |
| Lambda | Arm64 (Graviton2) instead of x86 | ~20% |
| Aurora | Serverless v2 for dev/staging (scales to 0.5 ACU at night) | ~70% on non-prod |
| S3 | Intelligent-Tiering for product images (access pattern unknown) | ~30% |
| DynamoDB | On-Demand for variable order traffic; Reserved for sessions | ~25% |
| CloudFront | Cache product pages 5 min — 80% cache hit rate | Reduces API GW requests by 80% |

---

## 5. Disaster Recovery Plan

| Scenario | Detection | Response | RTO | RPO |
|----------|-----------|----------|-----|-----|
| Lambda function fails | X-Ray + CW Alarm | Auto retry (3x), SQS DLQ, alert | < 1 min | 0 (SQS preserved) |
| AZ failure | CW health checks | ALB routes to healthy AZ; RDS Multi-AZ failover | < 2 min | 0 (sync replication) |
| Region failure (us-east-1) | Route 53 health check | Fail over to eu-west-1 (Aurora Global DB promote) | < 10 min | < 1 sec |
| Accidental data deletion | CloudTrail audit, DynamoDB point-in-time | Restore Aurora to point-in-time; DynamoDB PITR | < 1 hour | 5 min |
| Ransomware/breach | GuardDuty alert, Security Hub | Isolate accounts via SCP; restore from S3 versioned/locked backup | 4-8 hours | 1 day |

---

## 6. Compliance Checklist (SOC 2 + PCI-DSS)

| Control | Implementation |
|---------|---------------|
| Encryption at rest | SSE-KMS on S3, Aurora, DynamoDB, ElastiCache |
| Encryption in transit | TLS 1.2+ enforced on ALB, API GW, RDS |
| Access control | IAM Identity Center + Okta; no IAM users in prod (SCP enforced) |
| Least privilege | IAM roles with Permission Boundaries; IRSA for K8s |
| MFA | Enforced via IAM Identity Center + Okta |
| Audit logging | CloudTrail multi-region → Security Account (tamper-proof) |
| Vulnerability management | Amazon Inspector on ECR images + EC2 |
| DLP | Amazon Macie on S3 buckets containing PII |
| Network segmentation | Private subnets for all data; VPC endpoints; NACLs |
| Incident response | GuardDuty → Security Hub → EventBridge → PagerDuty runbooks |
| Data retention | S3 Object Lock on invoices (7-year Compliance mode) |

---

## 7. Whiteboard Narration Script (15-Minute Interview Demo)

### Opening (1 min)
> "I'll walk you through an e-commerce platform built on AWS using the core services. The key constraints are: high availability (99.95%), global scale, SOC 2 compliance, and a small engineering team that can't spend time managing servers."

### Edge Layer (2 min)
> "Traffic comes in through Route 53 using latency-based routing — users in Europe hit our EU CloudFront distribution, users in APAC hit our APAC edge. WAF sits in front of CloudFront, filtering OWASP Top 10 attacks and rate-limiting. CloudFront caches our static product pages and images from S3, so 80% of read traffic never hits the origin. For dynamic API calls, CloudFront forwards to API Gateway."

### API & Compute (3 min)
> "API Gateway handles all our API routes with Cognito JWT authentication — no custom auth Lambda needed for our user-facing APIs. We use REST API type because we need caching for product reads and usage plans for partner APIs. The backend is mostly Lambda — small, focused functions per service domain: products, cart, orders, users. Lambda's Provisioned Concurrency is enabled on the order service and cart service so there are zero cold starts during checkout. ECS Fargate runs our search service because it has a longer runtime and maintains an OpenSearch connection pool."

### Async Processing (2 min)
> "Order processing is intentionally async. The Order Lambda validates the request, writes to Aurora, and puts a message on SQS — returning 202 Accepted immediately. This means our API response time is consistent even if downstream processing is slow. The OrderProcessor Lambda polls SQS in batches. It's idempotent — it checks DynamoDB before processing to handle SQS's at-least-once delivery. Failed messages go to a DLQ with an alarm."

### Data Layer (3 min)
> "Data sits in a private VPC with no internet route. Aurora MySQL Global Database is the source of truth — the primary in us-east-1, a replica in eu-west-1 with ~1 second lag for DR. RDS Proxy sits in front of Aurora to handle the Lambda connection spike — without it, thousands of concurrent Lambda instances would exhaust Aurora's connection limit. DynamoDB stores sessions, cart, and product cache with Global Tables replicated to both regions. ElastiCache Redis caches API responses and implements rate limiting. S3 stores product images, order invoices, and reports — invoices use Object Lock in Compliance mode for 7 years."

### Observability (2 min)
> "CloudWatch is the observability hub. Lambda emits business metrics via EMF — orders per minute, checkout errors — alongside the default AWS metrics. We have 15 critical alarms routing to PagerDuty and 30 warning alarms to Slack. Composite alarms reduce false positives: we only page the on-call when BOTH error rate AND latency are elevated simultaneously. X-Ray traces every request end-to-end — the Service Map shows which of our 50 services is the bottleneck at any moment."

### Security & IaC (2 min)
> "Security is layered. AWS Organizations with SCPs — no IAM users in production, no resources outside us-east-1. Engineers log in via IAM Identity Center connected to Okta. GitHub Actions uses OIDC to get temporary credentials — no stored access keys anywhere. Our CDK codebase defines all infrastructure as TypeScript — every change is a PR, reviewed, tested, and deployed through CodePipeline with blue/green deployments and automatic rollback."

### Closing (1 min)
> "To summarize: the architecture is fully serverless for the API and processing tier, event-driven for order processing, globally distributed with Aurora Global DB and DynamoDB Global Tables, and secure by default with no public data tier, KMS encryption everywhere, and tamper-proof audit logs. The entire stack is defined in CDK, so spinning up a dev environment or a new region takes minutes."
