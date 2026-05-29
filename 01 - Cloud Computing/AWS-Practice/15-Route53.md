# AWS Route 53 — DNS and Traffic Management

## 1. Core Concepts

### What is Route 53?
Route 53 is AWS's **scalable, highly available DNS service** and **domain registrar**. It routes end-user requests to applications running in AWS or on-premises. The name "53" refers to port 53 (the standard DNS port).

### Key Features
- **Domain registration** — register or transfer domains
- **DNS hosting** — manage public and private hosted zones
- **Health checks** — monitor endpoints and trigger DNS failover
- **Traffic routing policies** — flexible routing for latency, geo, weighted, failover scenarios
- **Resolver** — hybrid DNS between on-premises and VPC

### Hosted Zones
| Type | Scope | Use Case |
|------|-------|----------|
| **Public Hosted Zone** | Internet-accessible | Resolve `api.shopwave.com` from anywhere |
| **Private Hosted Zone** | VPC-internal only | Resolve `db.internal` inside VPC (split-horizon DNS) |

Cost: $0.50/month per hosted zone (first 25 zones); $0.10/month after.

### Record Types
| Type | Purpose | Example |
|------|---------|---------|
| **A** | IPv4 address | `shopwave.com → 1.2.3.4` |
| **AAAA** | IPv6 address | `shopwave.com → 2001:db8::1` |
| **CNAME** | Alias for another hostname | `www → shopwave.com` (cannot use at zone apex) |
| **Alias** | AWS-specific alias (free queries, works at apex) | `shopwave.com → ALB` |
| **MX** | Mail exchange | `shopwave.com → mail.shopwave.com priority 10` |
| **TXT** | Text records | SPF, DKIM, domain verification |
| **NS** | Name servers | Delegation |
| **SOA** | Start of authority | Zone metadata |
| **SRV** | Service location | Kubernetes, gaming |
| **PTR** | Reverse DNS | IP → hostname |

> **Alias vs CNAME**: Alias records are Route 53-specific, resolve natively to AWS resource endpoints (ALB, CloudFront, S3 website, API Gateway), have **no extra charge** for queries, and **work at the zone apex** (e.g., `shopwave.com`). CNAME cannot be used at the apex.

### Routing Policies
| Policy | Behavior | Use Case |
|--------|----------|----------|
| **Simple** | Returns one or more values; client picks randomly | Single resource, no health checks |
| **Weighted** | Split traffic by percentage weights | A/B testing, canary releases |
| **Latency** | Route to region with lowest latency for user | Multi-region active-active |
| **Geolocation** | Route based on user's geographic location | Regulatory, language-specific content |
| **Geoproximity** | Route based on location + optional bias | Fine-grained geographic shifting |
| **Failover** | Active-passive: primary unless unhealthy | Disaster recovery |
| **Multi-Value Answer** | Return up to 8 healthy records randomly | Simple load balancing without ELB |
| **IP-Based** | Route based on client CIDR block | Known network topology, ISP routing |

### Health Checks
Route 53 health checks monitor endpoints by sending HTTP/HTTPS/TCP requests from multiple AWS regions. Types:
| Type | Monitors |
|------|---------|
| **Endpoint** | HTTP(S)/TCP to a specific IP or hostname |
| **Calculated** | AND/OR logic combining child health checks |
| **CloudWatch Alarm** | Healthy only when alarm is in OK state |

Failure threshold: 3 consecutive failures (each check every 30 s default, or 10 s for fast checks at extra cost).  
Health checks integrate with DNS routing policies to enable **automatic failover**.

### Route 53 Resolver
| Feature | Description |
|---------|-------------|
| **Resolver Inbound Endpoint** | Allow on-premises DNS servers to resolve AWS private hosted zones |
| **Resolver Outbound Endpoint** | Allow VPC instances to forward DNS queries to on-premises DNS |
| **Resolver Rules** | Forward specific domains (e.g., `corp.internal`) to specified DNS servers |
| **DNS Firewall** | Block or allow domain name queries from VPCs (prevent data exfiltration via DNS) |

### DNSSEC
Route 53 supports **DNSSEC signing** for public hosted zones to protect against DNS spoofing and cache poisoning. A cryptographic signature is added to DNS records; resolvers can verify authenticity.

---

## 2. Interview Questions & Answers

### Q1. What is the difference between an Alias record and a CNAME in Route 53?
- **CNAME** is a standard DNS record that maps one hostname to another. It cannot be used at the **zone apex** (e.g., `shopwave.com`) — only on subdomains. Route 53 charges for CNAME queries.
- **Alias** is Route 53-specific. It maps a hostname directly to an AWS resource (ALB, CloudFront, S3 website, API Gateway, Elastic Beanstalk). It **works at the zone apex**, has **no query charge**, and Route 53 automatically handles the underlying IP changes as the resource changes.

Rule of thumb: always use Alias for AWS resources, CNAME only for non-AWS third-party hostnames.

---

### Q2. Explain the difference between latency-based and geolocation routing.
- **Latency-based**: Routes to the AWS region with the **lowest network latency** for the user's IP, regardless of geography. Best for performance — a user in Japan near a US West Coast region may be routed there if it's faster than ap-northeast-1.
- **Geolocation**: Routes based on the **country or continent** of the user's IP. Best for regulatory requirements, language-specific content, or business logic (e.g., EU users must be served from EU region for GDPR).

For global apps that want both, you'd typically use **latency** for performance and override with **geolocation** for compliance.

---

### Q3. How does Route 53 failover routing work?
1. Configure a **primary** and **secondary** record with `Failover` routing policy
2. Attach a **health check** to the primary record
3. If the health check fails (3 consecutive failures by default), Route 53 returns the secondary record
4. When the primary recovers, Route 53 automatically routes back to primary (configurable)

This is used for **active-passive** disaster recovery. For active-active, use latency or weighted routing with health checks on all endpoints.

---

### Q4. What is a weighted routing policy and how would you use it for a canary release?
Weighted routing assigns a relative **weight** to each record set. Route 53 distributes queries proportionally.  
For a canary release:
1. Create record for `v1` with weight `90` and `v2` with weight `10`
2. Traffic splits 90/10 between versions
3. Gradually increase `v2` weight as confidence grows
4. Set `v1` weight to `0` to drain traffic (no queries go there without removing the record)

A weight of `0` removes the record from rotation without deleting it.

---

### Q5. How do Route 53 health checks integrate with DNS?
Health checks run independently from DNS. When a routing policy has health checks associated:
- Route 53 only returns records whose health check is **Healthy**
- If all records are unhealthy, Route 53 returns all records (to avoid complete outage)
- **Calculated health checks** combine child health checks with AND/OR logic (e.g., "healthy if at least 2 of 3 endpoints are healthy")

---

### Q6. What is the difference between a public and a private hosted zone?
- **Public Hosted Zone**: Accessible from the internet. Records resolve from any DNS resolver globally. Used for customer-facing domains.
- **Private Hosted Zone**: Resolves only within **associated VPCs**. Used for internal services (e.g., `payments-service.internal → 10.0.1.50`). Multiple VPCs (same or different accounts) can be associated with a single private hosted zone.

---

### Q7. How does Route 53 Resolver enable hybrid DNS?
**Outbound Endpoints** are ENIs in your VPC that forward specified domains to on-premises DNS servers via **Resolver Rules**. For example, queries for `corp.internal` go to your on-premises DNS at `192.168.1.1`.  
**Inbound Endpoints** are ENIs that receive DNS queries from on-premises and resolve them against Route 53 (public + private hosted zones). Combined, this enables seamless DNS resolution across VPN or Direct Connect connections.

---

### Q8. What is Route 53 DNS Firewall?
DNS Firewall lets you filter outbound DNS queries from VPCs. You create **rule groups** with domain lists that are allowed or blocked. Use cases:
- Block malware command-and-control domains
- Prevent **DNS data exfiltration** (attackers encode data in DNS queries to external resolvers)
- Enforce an allow-list for approved external services

DNS Firewall is evaluated before Route 53 Resolver forwards queries.

---

### Q9. How do you migrate a domain to Route 53 with zero downtime?
1. **Create a public hosted zone** in Route 53 and duplicate all DNS records from the current provider
2. **Lower the TTL** on all records at the current provider (to 60 s) — wait for TTL to expire so changes propagate quickly
3. **Verify** the hosted zone NS records are working by testing against Route 53's name servers directly
4. **Update the domain registrar** NS records to point to Route 53's name servers (4 NS records given by Route 53)
5. Wait for NS TTL (48–72 hr for most registries) — old records are still served during this window
6. Once propagated, raise TTL back to 300–3,600 s

---

### Q10. What are TTL best practices in Route 53?
| Scenario | Recommended TTL |
|----------|----------------|
| Static content, rarely changes | 86,400 s (24 hr) |
| Production APIs with ALB | 300 s |
| Disaster recovery active records | 60 s (fast failover) |
| Pre-migration (lowering before change) | 60 s |
| Health-checked failover records | 60 s |

Lower TTL = faster propagation but more DNS queries (cost). Higher TTL = better cache performance but slower failover.

---

### Q11. Can Route 53 route traffic to on-premises servers?
Yes. You can create A records pointing to on-premises IP addresses. Combined with **failover routing** and health checks, you can route to on-premises when AWS is unavailable (or vice versa for cloud burst scenarios).  
For hybrid setups, use **Resolver Outbound Endpoints** to resolve on-premises hostnames from VPCs.

---

### Q12. What is Route 53 Traffic Flow?
**Traffic Flow** is a visual policy editor that lets you build complex routing policies using a **traffic policy** (a tree of rules). You can combine multiple routing types (e.g., geolocation → latency → failover) and apply the same policy across multiple domain names.  
Traffic policies are versioned, enabling rollbacks. Useful when you have many regions and complex routing logic.

---

### Q13. How does Route 53 handle multi-region active-active deployments?
1. Use **latency-based routing** to send users to the nearest healthy region
2. Attach **health checks** to each regional endpoint (ALB or NLB)
3. Route 53 automatically removes unhealthy regions from DNS responses
4. Use **Alias records** pointing to regional ALBs
5. Optionally layer **weighted routing** on top of latency for regional traffic shifting

---

### Q14. What is DNSSEC and when would you enable it on Route 53?
DNSSEC (DNS Security Extensions) adds cryptographic signatures to DNS records, allowing resolvers to verify authenticity and detect tampering (DNS spoofing/cache poisoning).  
Enable DNSSEC for:
- High-security domains (financial services, government)
- Compliance requirements (FedRAMP, NIST)
- Any domain where DNS hijacking would be catastrophic

Route 53 manages key signing automatically. You need to register the **DS record** with your domain registrar to chain trust.

---

### Q15. Design a multi-region, highly available DNS architecture for a global e-commerce site.
```
shopwave.com (Route 53 Public Hosted Zone)
  │
  ├── Latency Policy — us-east-1 ALB (weight: active, health check: /health)
  ├── Latency Policy — eu-west-1 ALB (weight: active, health check: /health)
  └── Latency Policy — ap-southeast-1 ALB (weight: active, health check: /health)

Each regional ALB → ECS/Lambda → RDS Aurora Global DB

Failover:
  - Health check fails in us-east-1 → Route 53 stops returning that record
  - Users in us-east-1 routed to eu-west-1 or ap-southeast-1
  - Aurora Global DB promotes secondary cluster in target region
  - RTO: ~1 min (DNS failover) + ~1 min (Aurora failover) = ~2 min total
```

---

## 3. Real-World Use Case: Multi-Region Active-Active Global DNS

### Scenario
ShopWave serves customers in US, EU, and APAC. They need:
- < 200 ms latency for all users globally
- Automatic failover if any region goes down (RTO < 5 min)
- Regulatory requirement: EU customers' data must stay in eu-west-1

### Architecture

```
shopwave.com (Route 53)
     │
     ├─ [Geolocation: Europe] → Latency → eu-west-1 CloudFront → ALB
     │                                    (GDPR: EU data stays in eu-west-1)
     │
     ├─ [Geolocation: Asia] → Latency → ap-southeast-1 CloudFront → ALB
     │
     └─ [Default] → Latency → us-east-1 CloudFront → ALB

Health checks on each regional ALB:
  - Threshold: 3 failures → mark unhealthy
  - Frequency: every 10 s (fast health check)
  - CloudWatch alarm: AlertOps on health check failure

Private Hosted Zone (internal routing):
  - payments.internal → 10.0.1.50 (internal payments service)
  - db-primary.internal → RDS Aurora primary writer
  - db-replica.internal → RDS Aurora read replica
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Geolocation + Latency combination | Geo ensures EU data residency; latency optimizes within each geo |
| 10-second health check frequency | Faster failure detection (extra ~$1/month per health check) |
| Private hosted zone for internal services | Decouple internal service discovery from public DNS |
| Alias records for all AWS resources | No CNAME limitations, no query charges, auto-updates with ALB IP changes |
| DNSSEC enabled | Financial platform — DNS hijacking would redirect users to fraud sites |

### Interview Narration (Whiteboard Script)
> "I'd use Route 53 with a combination of geolocation and latency routing. Geolocation rules first — EU traffic goes to eu-west-1 for GDPR compliance, APAC to ap-southeast-1. Within those geos, latency routing picks the nearest healthy endpoint. Health checks run every 10 seconds; if a region fails three consecutive checks, Route 53 removes it from DNS automatically. All records are Alias records pointing to regional CloudFront distributions, so there's no charge per query and IP changes are handled transparently. For internal service discovery, I'd use private hosted zones in the VPC — much cleaner than maintaining /etc/hosts files."
