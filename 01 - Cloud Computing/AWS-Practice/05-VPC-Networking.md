# AWS VPC & Networking

## 1. Core Concepts

### What is a VPC?
A **Virtual Private Cloud (VPC)** is your own isolated, logically separate section of the AWS cloud. You define the IP address range (CIDR block), create subnets, set up route tables, and control traffic flow with security groups and NACLs.

### VPC Fundamentals
- Each AWS region gets a **default VPC** (CIDR: 172.31.0.0/16) with public subnets in every AZ — avoid using this for production
- **Custom VPC**: Define your own CIDR (e.g., 10.0.0.0/16); up to 5 VPCs per region (soft limit)
- VPCs span all AZs in a region
- VPC **CIDR cannot overlap** with VPCs you want to peer with

### Subnets
- **Public Subnet**: Route table has a route to an Internet Gateway (`0.0.0.0/0 → igw-xxx`). Resources can have public IPs.
- **Private Subnet**: No direct route to internet. Resources use NAT Gateway for outbound-only internet access.
- **Isolated Subnet**: No route to internet at all (DB-only tier)
- One subnet exists in **one AZ** (many subnets can be in the same AZ)

### Internet Gateway (IGW)
- Allows resources in **public subnets** to communicate with the internet
- Horizontally scaled, redundant, HA — no bandwidth constraints
- One IGW per VPC

### NAT Gateway
- Allows resources in **private subnets** to initiate outbound internet connections (software updates, API calls)
- Blocks inbound connections initiated from internet
- **Managed by AWS**: Highly available within an AZ; deploy one per AZ for HA
- Charges: $0.045/hr + $0.045/GB data processing
- Alternative: NAT Instance (EC2-based, cheaper but manual HA, not recommended for production)

### Route Tables
- Each subnet is associated with one route table
- Route table entries: `destination CIDR → target`
- Local route (`10.0.0.0/16 → local`) always present and can't be removed
- Most specific route wins (longest prefix match)

### Security Groups
- **Stateful** firewall attached to ENIs (Elastic Network Interfaces)
- Only **ALLOW** rules — no deny rules
- Return traffic automatically allowed (stateful)
- Can reference other security groups as sources (e.g., "allow traffic from the app-sg")
- Changes take effect immediately

### Network ACLs (NACLs)
- **Stateless** firewall attached to **subnets**
- Supports **ALLOW** and **DENY** rules
- Rules evaluated in **ascending numeric order** — first match wins
- Must explicitly allow **both inbound and outbound** (stateless)
- Processed before packets reach Security Groups

### Security Group vs NACL Comparison
| Feature | Security Group | NACL |
|---------|---------------|------|
| State | Stateful | Stateless |
| Scope | Instance (ENI) | Subnet |
| Rules | Allow only | Allow + Deny |
| Evaluation | All rules | In numeric order |
| Return traffic | Auto-allowed | Must be explicitly allowed |
| Use case | Per-resource firewall | Subnet-level perimeter |

### VPC Endpoints
Allow private connectivity to AWS services **without internet, NAT, VPN, or Direct Connect**:

| Type | How | Use Case |
|------|-----|----------|
| **Gateway Endpoint** | Route table entry | S3 and DynamoDB only — free |
| **Interface Endpoint** (PrivateLink) | ENI in your subnet | Most AWS services (SSM, ECR, Secrets Manager, etc.) — charged per hour + data |

### VPC Peering
- One-to-one connection between two VPCs (same or different regions/accounts)
- **Non-transitive**: If A↔B and B↔C, A cannot reach C through B (must create A↔C directly)
- CIDRs must not overlap
- Route tables in both VPCs must be updated manually

### AWS Transit Gateway
- Hub-and-spoke network to connect thousands of VPCs, VPNs, and Direct Connect
- **Transitive routing**: A can reach C through TGW if A↔TGW and C↔TGW
- Supports inter-region peering (TGW to TGW)
- Network Manager provides global topology view

### VPN Connections
| Type | Description | Bandwidth |
|------|-------------|-----------|
| **Site-to-Site VPN** | IPSec tunnel between on-premises and VPC | Up to 1.25 Gbps per tunnel (4 Gbps with ECMP + TGW) |
| **Client VPN** | OpenVPN-based for individual users | Per-user |

### AWS Direct Connect
- Dedicated private network connection from on-premises to AWS (not over internet)
- 1 Gbps, 10 Gbps, or 100 Gbps ports at AWS Direct Connect Locations
- Lower latency, consistent bandwidth, more predictable than VPN
- **Direct Connect + VPN**: Encrypt Direct Connect traffic with IPSec

### Elastic IP (EIP)
- Static public IPv4 address allocated to your account
- Moves between instances on failover (Elastic Load Balancers don't need EIPs — they get DNS names)
- Charged when **not associated** with a running instance

### Bastion Host / Jump Box
EC2 instance in a public subnet used to SSH/RDP into private instances. Replace with **AWS Systems Manager Session Manager** for auditable, key-free shell access to private instances — no bastion needed.

---

## 2. Interview Questions & Answers

### Q1. What is the difference between a Security Group and a NACL?
**Security Groups**: Stateful, instance-level (ENI), allow-only rules. Return traffic is automatically permitted. Changes are immediate. Best for per-instance access control.

**NACLs**: Stateless, subnet-level, allow and deny rules evaluated in order. Must explicitly allow return traffic. Best for adding an extra layer of subnet perimeter control (e.g., blocking specific IPs).

**Defense in depth**: Use both — NACLs as the coarse subnet perimeter, Security Groups as the fine-grained per-instance firewall.

---

### Q2. What is the difference between a VPC Gateway Endpoint and Interface Endpoint?
**Gateway Endpoint**: Adds a route to S3 or DynamoDB via the route table. Traffic stays in AWS backbone. **Free**. Only for S3 and DynamoDB.

**Interface Endpoint (PrivateLink)**: Creates an ENI in your subnet with a private IP. Traffic to the service uses this IP — stays within the VPC, no internet. Supports 100+ AWS services. Charged per endpoint-hour and per GB.

Use Gateway Endpoints for S3/DynamoDB (free, same performance). Use Interface Endpoints for SSM, ECR, Secrets Manager, etc. in private-only environments (no internet access).

---

### Q3. What is VPC Peering and what are its limitations?
VPC Peering connects two VPCs so they can communicate using private IPs. Limitations:
1. **Non-transitive**: VPC A → B → C doesn't mean A → C. You need explicit peering for every pair.
2. **No overlapping CIDRs**: Must plan IP ranges in advance
3. **No edge-to-edge routing**: Cannot route internet traffic, VPN traffic, or Direct Connect through a peered VPC

For many-to-many VPC connectivity, use **AWS Transit Gateway** instead.

---

### Q4. What is AWS Transit Gateway and when should you use it instead of VPC Peering?
Transit Gateway is a hub-and-spoke model for connecting VPCs, VPNs, and Direct Connect at scale. Routing is **transitive** through the TGW.

Use TGW when:
- Connecting more than ~10 VPCs (peering mesh becomes complex)
- Need transitive routing between VPCs
- Connecting VPCs across multiple regions (inter-region TGW peering)
- Centralizing security inspection (route all traffic through a security VPC via TGW)

Use VPC Peering when:
- Simple 1-to-1 or few VPC connectivity
- Cost-sensitive (TGW has per-attachment and per-GB charges)

---

### Q5. What is the difference between Site-to-Site VPN and Direct Connect?
| | Site-to-Site VPN | Direct Connect |
|--|-----------------|----------------|
| Connection | IPSec over public internet | Dedicated private fiber |
| Latency | Variable (internet) | Consistent, low |
| Bandwidth | Up to ~4 Gbps with ECMP | 1, 10, or 100 Gbps |
| Setup time | Minutes | Weeks to months |
| Cost | Low ($0.05/hr per connection) | High (port fees + partner fees) |
| Encryption | Yes (IPSec) | Not by default (add VPN for encryption) |
| Use case | Quick DR, remote access, cost-sensitive | Production workloads, high throughput, compliance |

Best practice: **Direct Connect + VPN** — private connection with IPSec encryption for both performance and security.

---

### Q6. How do you design a VPC for a 3-tier web application?
```
CIDR: 10.0.0.0/16

Public Subnets (ALB, NAT GW, Bastion): 
  10.0.0.0/24 (AZ-a), 10.0.1.0/24 (AZ-b)
  Route: 0.0.0.0/0 → IGW

App Subnets (EC2, ECS, Lambda in VPC):
  10.0.10.0/24 (AZ-a), 10.0.11.0/24 (AZ-b)
  Route: 0.0.0.0/0 → NAT GW

DB Subnets (RDS, ElastiCache):
  10.0.20.0/24 (AZ-a), 10.0.21.0/24 (AZ-b)
  Route: local only (no internet route)
```

Security Groups:
- `alb-sg`: 0.0.0.0/0 port 443 → ALB
- `app-sg`: `alb-sg` port 8080 → App instances
- `db-sg`: `app-sg` port 3306 → RDS

---

### Q7. What is a NAT Gateway and how does it differ from a NAT Instance?
Both allow private subnet resources to access the internet outbound-only.

**NAT Gateway (managed)**:
- AWS manages availability, scaling, and redundancy within an AZ
- No security group — managed by AWS
- Up to 45 Gbps throughput
- $0.045/hr + $0.045/GB

**NAT Instance (EC2-based)**:
- Self-managed — you handle HA, scaling, patching
- Can use Security Groups
- Cheaper (can use a t3.nano) but more operational overhead
- Limited throughput by instance size

Best practice: Use NAT Gateway for production; deploy one per AZ for high availability.

---

### Q8. What is a VPC Flow Log and what does it capture?
VPC Flow Logs capture metadata about IP traffic to/from network interfaces in your VPC. Published to CloudWatch Logs or S3.

Fields include: srcaddr, dstaddr, srcport, dstport, protocol, packets, bytes, action (ACCEPT/REJECT), log-status.

Use cases:
- Security analysis (identify port scans, unexpected traffic)
- Troubleshooting connectivity issues (is traffic being rejected by a SG/NACL?)
- Compliance auditing of network access

Note: Flow Logs capture **metadata**, not packet contents.

---

### Q9. How does an Elastic Network Interface (ENI) work?
An ENI is a virtual NIC in a VPC. Every EC2 instance has at least one primary ENI (eth0). You can:
- Attach multiple ENIs to one instance (for multiple subnets/IPs/SGs)
- Move an ENI (and its private IP, EIP, and MAC address) between instances — useful for failover appliances
- ENIs have: private IPs, public IP (optional), MAC address, Security Groups

---

### Q10. What is PrivateLink?
AWS PrivateLink lets you access AWS services or your own services (in another VPC/account) via a private endpoint in your VPC. Traffic never traverses the public internet.

Components:
- **Service provider**: Creates a Network Load Balancer in front of their service; registers it with PrivateLink
- **Consumer**: Creates an Interface VPC Endpoint in their VPC; gets a private IP for the service

Use case: SaaS provider exposes their API to customers' VPCs without the customer needing VPC peering or internet access.

---

### Q11. Explain the NACL rule evaluation order with an example.
NACLs evaluate rules in **ascending numeric order**, and the first match is applied.

Example (inbound):
```
Rule 100: ALLOW TCP 0.0.0.0/0 port 443
Rule 200: DENY  TCP 10.0.5.0/24 port 443
Rule *:   DENY  all traffic (default)
```
If a request comes from 10.0.5.1 port 443:
- Rule 100 matches first → **ALLOW** (Rule 200 never evaluated)

To block that subnet, put the DENY rule at a lower number than the ALLOW:
```
Rule 90: DENY TCP 10.0.5.0/24 port 443
Rule 100: ALLOW TCP 0.0.0.0/0 port 443
```

---

### Q12. What is AWS Global Accelerator and how does it differ from CloudFront?
| | AWS Global Accelerator | CloudFront |
|--|----------------------|------------|
| Protocol | Any TCP/UDP | HTTP/HTTPS |
| Caching | No | Yes |
| Static IPs | Yes (2 Anycast IPs) | No |
| Use case | Non-HTTP: gaming, IoT, VoIP; consistent IP needed | HTTP content delivery, CDN |
| Edge processing | No | Lambda@Edge, CloudFront Functions |

Global Accelerator routes traffic to the closest AWS edge location and then over AWS backbone to your endpoint — reduces latency and improves availability for non-HTTP workloads.

---

### Q13. What is the difference between an Internet Gateway and a NAT Gateway?
**Internet Gateway (IGW)**: Bidirectional — allows resources in public subnets to **receive inbound connections** from the internet AND make outbound connections.

**NAT Gateway**: Outbound-only for private subnet resources. Translates private IPs to the NAT Gateway's public IP for outbound traffic. Blocks inbound connections initiated from the internet.

---

### Q14. How do you connect multiple AWS accounts' VPCs in an enterprise?
Options:
1. **AWS Transit Gateway** (recommended): Each account connects their VPCs to a shared TGW (shared via Resource Access Manager). Centralized routing, transitive.
2. **VPC Peering**: Point-to-point, non-transitive — complex for many accounts.
3. **AWS PrivateLink**: For service-to-service access without full network peering.
4. **AWS Network Firewall + TGW**: Centralized inspection for all cross-account traffic.

Enterprise pattern: **AWS Organizations + TGW sharing via RAM + centralized inspection VPC** (hub-and-spoke with security).

---

### Q15. What happens at the networking level when you launch an EC2 instance in a public subnet?
1. EC2 is assigned a **primary ENI** with a private IP from the subnet CIDR
2. If "Auto-assign public IP" is enabled, AWS assigns a public IP (from AWS's pool) to the ENI
3. The subnet's route table sends `0.0.0.0/0` traffic to the **Internet Gateway**
4. The IGW maintains a 1-to-1 NAT mapping between the instance's private IP and public IP
5. The Security Group applied to the ENI controls which traffic is allowed inbound/outbound
6. A NACL on the subnet processes traffic before it reaches the Security Group

---

## 3. Real-World Use Case: Secure 3-Tier Network Architecture

### Scenario
A healthcare startup needs:
- Patient-facing web application (HIPAA compliance)
- Private API and application servers — no public IPs
- Database tier completely isolated from internet
- Secure connectivity to on-premises EMR system
- Full network traffic audit capability
- DDoS protection

### Architecture

```
                   Internet
                      │
              [AWS Shield Standard]
                      │
           [AWS WAF] (OWASP, rate limit)
                      │
                [CloudFront CDN]
                      │
                [ALB] (public subnets, AZ-a + AZ-b)
                      │
        ┌─────────────┼─────────────────┐
        │             │                 │
   Public Subnet AZ-a │          Public Subnet AZ-b
  ┌───────────┐        │         ┌───────────┐
  │ NAT GW-a  │        │         │ NAT GW-b  │
  └───────────┘        │         └───────────┘
                       │
        ┌──────────────┴─────────────────┐
        │                                │
  App Subnet AZ-a                 App Subnet AZ-b
  ┌──────────────┐              ┌──────────────┐
  │ ECS Fargate  │              │ ECS Fargate  │  ← No public IP
  │ API service  │              │ API service  │
  └──────┬───────┘              └──────┬───────┘
         │                            │
  ┌──────┴────────────────────────────┴──────┐
  │              DB Subnet (private)          │
  │    Aurora MySQL Multi-AZ                  │
  │    ElastiCache Redis                      │
  │    (isolated — no internet route)         │
  └───────────────────────────────────────────┘
         │
  [VPC Endpoints] → S3, SSM, ECR, Secrets Manager (no internet)
         │
  [Direct Connect] → On-premises EMR system
         │
  [VPC Flow Logs] → CloudWatch Logs → CloudWatch Insights

Security Groups:
  alb-sg:   0.0.0.0/0:443 → ALB
  app-sg:   alb-sg:8080 → ECS tasks
  db-sg:    app-sg:3306/6379 → Aurora + Redis

NACLs (subnet-level deny list):
  NACL-app: DENY known malicious IPs (updated via Lambda from threat intel feed)
  NACL-db:  DENY all except 10.0.10.0/24 and 10.0.11.0/24 (app subnets only)
```

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| No public IP on app/DB instances | Attack surface reduction — HIPAA best practice |
| NAT Gateway per AZ | If one AZ's NAT GW fails, other AZ's instances still have outbound access |
| VPC Interface Endpoints | ECR, SSM, Secrets Manager accessed privately — no internet traffic for critical ops |
| Direct Connect to on-prem EMR | Consistent, private, encrypted access to patient record system |
| VPC Flow Logs to CloudWatch | HIPAA requires audit trail of network access; Insights queries detect anomalies |
| AWS WAF + Shield Standard | OWASP Top 10 protection + DDoS mitigation — healthcare sites are frequent targets |
| Separate DB Subnet NACL | Explicit subnet-level deny ensures DB subnets unreachable from public or app subnets not on allow-list |

### Interview Narration (White-board Script)
> "For the healthcare platform I'd design three subnet tiers: public for the load balancer and NAT gateways, private for the application layer, and isolated for the database with no internet route at all. The app tier gets outbound internet through NAT Gateways — one per AZ for HA — but has no public IPs. All AWS service calls — to ECR, SSM, and Secrets Manager — go through VPC Interface Endpoints so that traffic never touches the internet, which is important for HIPAA. The on-premises EMR connection uses Direct Connect for consistent, private, auditable access. VPC Flow Logs capture all network metadata and go to CloudWatch where I set up queries to detect unusual access patterns."
