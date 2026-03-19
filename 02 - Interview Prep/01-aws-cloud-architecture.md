# AWS Cloud Architecture — Interview Prep

---

## Your Real AWS Experience (eGain, 2021–2025)

Always anchor answers to this. It's your credibility.

**What you did:**
- Migrated analytics product from SSAS (SQL Server Analysis Services) to **AWS Redshift** — a full data warehouse migration
- Automated workflows using **Lambda** (serverless compute), **EC2** (virtual machines), and **Secrets Manager** (credentials/secrets)
- Automated deployment of key infrastructure, reducing deployment time
- Contributed to cloud transformation initiative improving data processing speed

**How to narrate it:**
> "At eGain, I was part of a cloud transformation initiative where we migrated our analytics product from on-premises SSAS to AWS Redshift. My role involved designing and implementing Lambda functions to automate data workflows, managing secrets securely using Secrets Manager, and working with EC2 for infrastructure. The migration significantly improved data processing speeds and accessibility for our analytics platform."

---

## Core AWS Services You Should Know

### Compute

| Service | What It Is | Your Angle |
|---|---|---|
| **EC2** | Virtual machines in the cloud | Used at eGain for infrastructure |
| **Lambda** | Serverless functions, event-driven | Used at eGain for workflow automation |
| **ECS** | Container orchestration (Docker) | Linked to containerization JD requirement |
| **EKS** | Managed Kubernetes | Enterprise container orchestration |
| **Elastic Beanstalk** | PaaS — deploy apps without managing infra | Good for ASP.NET deployments |

### Storage & Database

| Service | What It Is | Your Angle |
|---|---|---|
| **S3** | Object storage | Standard — data lake, file storage |
| **Redshift** | Data warehouse (columnar, petabyte-scale) | Your migration story |
| **RDS** | Managed relational DB (SQL Server, Postgres, MySQL) | Familiar territory for .NET dev |
| **DynamoDB** | NoSQL key-value store | Fast, serverless-compatible |

### Security & Identity

| Service | What It Is | Your Angle |
|---|---|---|
| **Secrets Manager** | Store/rotate credentials securely | Used at eGain |
| **IAM** | Roles, policies, permissions | Core to any AWS architecture |
| **KMS** | Key Management Service — encrypt data | Works alongside Secrets Manager |
| **VPC** | Virtual Private Cloud — network isolation | Foundational for secure architecture |

### Integration & Messaging

| Service | What It Is |
|---|---|
| **SQS** | Message queue — decouples services |
| **SNS** | Pub/sub notifications |
| **EventBridge** | Event bus — connect AWS services/apps |
| **API Gateway** | Managed REST/WebSocket API layer |

### DevOps

| Service | What It Is | Your Angle |
|---|---|---|
| **CodePipeline** | CI/CD pipeline | Mirrors your CI/CD experience |
| **CodeBuild** | Build service | Equivalent to GitHub Actions/Jenkins |
| **CloudFormation** | Infrastructure as Code (YAML/JSON) | IaC = DevOps best practice |
| **CloudWatch** | Monitoring, logging, alerting | Essential for production ops |

---

## AWS Architecture Patterns You Should Know

### Serverless Architecture
```
Client → API Gateway → Lambda → DynamoDB/RDS
                              ↘ S3
```
- Stateless, event-driven
- Lambda scales automatically
- Cost-efficient for variable workloads
- **Your experience:** Lambda + workflow automation at eGain

### Microservices on ECS/EKS
```
Client → Load Balancer → ECS Cluster (containers)
                              ↓
                        RDS / Redshift / S3
```
- Each service independently deployable
- Container-based isolation

### Data Warehouse Pattern (Your Migration Story)
```
Source Systems → ETL (Lambda/Glue) → S3 (raw) → Redshift → BI/Analytics
```
- **SSAS → Redshift** is exactly this pattern
- Redshift: columnar storage, massively parallel processing (MPP)

---

## Redshift Deep Dive (Your Migration Story)

**Why Redshift over SSAS?**
- SSAS is on-premises, requires SQL Server licensing + hardware
- Redshift: fully managed, auto-scaling, pay-per-query options
- Redshift integrates natively with S3, Lambda, Glue
- Better for large-scale analytics workloads

**Redshift key concepts:**
- **Columnar storage** — reads only columns needed (faster analytics)
- **Distribution styles** — EVEN, KEY, ALL (how data spreads across nodes)
- **Sort keys** — optimize query performance
- **Spectrum** — query S3 data directly from Redshift

**Sample answer:**
> "We chose Redshift because our on-prem SSAS solution had scaling limitations and high maintenance overhead. Redshift gave us columnar storage with MPP, native S3 integration, and a managed experience. After migration, our data processing speeds improved significantly and the team spent less time on infrastructure."

---

## Common AWS Interview Questions

**Q: How does Lambda pricing work?**
> Billed per request + duration (GB-seconds). First 1M requests/month free. Ideal for sporadic, short-duration workloads.

**Q: When would you use EC2 vs Lambda?**
> Lambda: stateless, event-driven, short-running tasks (<15 min), variable load. EC2: long-running processes, stateful apps, need OS control, specific hardware requirements.

**Q: How do you secure secrets in AWS?**
> AWS Secrets Manager — stores encrypted secrets, supports automatic rotation, integrates with IAM, Lambda, RDS. Avoid hardcoding credentials (used this at eGain).

**Q: What is IAM and how do you use it?**
> Identity and Access Management. Define roles + policies (least privilege). Attach roles to Lambda/EC2 instead of embedding access keys. Use resource-based and identity-based policies.

**Q: What's the difference between SQS and SNS?**
> SQS: queue — one consumer pulls messages, point-to-point. SNS: pub/sub — one message fans out to multiple subscribers. Often combined: SNS → SQS for fan-out with retry.
