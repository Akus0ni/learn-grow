# AWS Practice — Interview Prep: Core Services + Supporting Architecture Services

> **Goal:** Land a great cloud/AWS job in one week using $100 in AWS credits.  
> Each file covers a single service with deep-dive concepts, cracking interview Q&A, and a real-world architecture you can demo.
>
> **Structure:** Files 01–10 are the **10 core services** covered in the daily study plan. Files 13–21 are the **supporting services** used in the capstone architecture (CloudFront, WAF, Route 53, SQS, DynamoDB, EventBridge, ElastiCache, CloudTrail, SES) — each with the same full depth: concepts, 15 interview Q&As, and a real-world architecture.

---

## Study Plan

| Day | Services to Cover | Hands-On Lab |
|-----|-------------------|--------------|
| 1 | EC2 + IAM | Launch EC2, configure IAM roles & policies |
| 2 | S3 + CloudWatch | Create buckets, lifecycle policies, set up dashboards |
| 3 | VPC + Networking | Build a multi-tier VPC from scratch |
| 4 | RDS / Aurora | Deploy RDS, enable Multi-AZ & Read Replicas |
| 5 | Lambda + API Gateway | Build a serverless REST API end-to-end |
| 6 | ECS / EKS | Run Docker containers via Fargate |
| 7 | CloudFormation/CDK | Deploy the capstone architecture as IaC |

---

## File Index

### Core Services (Days 1–7)

| # | File | Service | What's Inside |
|---|------|---------|---------------|
| 1 | [EC2](./01-EC2.md) | Elastic Compute Cloud | Instance types, Auto Scaling, pricing, 15 interview Q&As, e-commerce web tier architecture |
| 2 | [S3](./02-S3.md) | Simple Storage Service | Storage classes, bucket policies, 15 interview Q&As, data lake architecture |
| 3 | [Lambda](./03-Lambda.md) | Serverless Functions | Cold starts, concurrency, triggers, 15 interview Q&As, event-driven order processor |
| 4 | [RDS & Aurora](./04-RDS-Aurora.md) | Managed Databases | Multi-AZ, Read Replicas, Aurora global, 15 interview Q&As, fintech transactional DB design |
| 5 | [VPC & Networking](./05-VPC-Networking.md) | Virtual Private Cloud | Subnets, NAT, security groups, NACLs, 15 interview Q&As, secure 3-tier network architecture |
| 6 | [IAM & Security](./06-IAM-Security.md) | Identity & Access Management | Policies, roles, STS, least privilege, 15 interview Q&As, cross-account access architecture |
| 7 | [ECS & EKS](./07-ECS-EKS.md) | Container Orchestration | Fargate, task definitions, K8s concepts, 15 interview Q&As, microservices platform |
| 8 | [API Gateway](./08-API-Gateway.md) | API Management | REST vs HTTP APIs, throttling, auth, 15 interview Q&As, serverless API architecture |
| 9 | [CloudFormation & CDK](./09-CloudFormation-CDK.md) | Infrastructure as Code | Templates, stacks, CDK constructs, 15 interview Q&As, full IaC pipeline |
| 10 | [CloudWatch & Observability](./10-CloudWatch-Observability.md) | Monitoring & Logging | Metrics, alarms, logs, X-Ray, 15 interview Q&As, observability stack |
| 11 | [Capstone Architecture](./11-Real-World-Capstone-Architecture.md) | All Services | Full end-to-end e-commerce platform — architecture diagram + narration |
| 12 | [7-Day Study Plan](./12-7-Day-Study-Plan.md) | Study Guide | Daily schedule, lab exercises mapped to your $100 credit budget, mock interview checklist |

### Supporting Services (used in the Capstone Architecture)

> These services appear in the capstone and real-world architectures throughout the study kit. Each file has the same format as the core service files: concepts, 15 interview Q&As, and a real-world architecture.

| # | File | Service | What's Inside |
|---|------|---------|---------------|
| 13 | [CloudFront](./13-CloudFront.md) | Content Delivery Network | Edge locations, cache behaviors, OAC, Lambda@Edge, 15 interview Q&As, global SPA delivery |
| 14 | [WAF](./14-WAF.md) | Web Application Firewall | Rule groups, rate limiting, bot control, ATP, 15 interview Q&As, multi-layer e-commerce protection |
| 15 | [Route 53](./15-Route53.md) | DNS & Traffic Management | Routing policies, health checks, Resolver, DNSSEC, 15 interview Q&As, multi-region active-active DNS |
| 16 | [SQS](./16-SQS.md) | Simple Queue Service | Standard vs FIFO, DLQ, visibility timeout, 15 interview Q&As, order processing decoupling |
| 17 | [DynamoDB](./17-DynamoDB.md) | Managed NoSQL Database | Single-table design, GSI, streams, DAX, transactions, 15 interview Q&As, e-commerce data model |
| 18 | [EventBridge](./18-EventBridge.md) | Serverless Event Bus | Event patterns, pipes, scheduler, archive/replay, 15 interview Q&As, microservices event backbone |
| 19 | [ElastiCache](./19-ElastiCache.md) | In-Memory Caching | Redis vs Memcached, cluster modes, caching strategies, 15 interview Q&As, session & API cache |
| 20 | [CloudTrail](./20-CloudTrail.md) | Audit & Governance | Management events, Insights, log protection, Athena, 15 interview Q&As, PCI-DSS compliance trail |
| 21 | [SES](./21-SES.md) | Simple Email Service | Deliverability, DKIM/SPF/DMARC, bounce handling, templates, 15 interview Q&As, transactional email |

---

## How to Use These Files

1. **Read** the Concepts section for each service first.  
2. **Answer** the interview questions out loud before reading the sample answers.  
3. **Build** the hands-on lab in your AWS account (see the study plan for credit budgets).  
4. **Architect** the real-world solution and practice explaining it on a whiteboard or in a Miro board.  
5. **Repeat** — revisit weak areas on Day 7.

---

> 💡 **Tip:** AWS Free Tier covers the basics. The $100 credits let you experiment with multi-AZ RDS, EKS, and larger EC2 instances without worry. Terminate resources after each lab session to stay within budget.
