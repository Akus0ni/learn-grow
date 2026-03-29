# AWS Practice — Top 10 Services Interview Prep

> **Goal:** Land a great cloud/AWS job in one week using $100 in AWS credits.  
> Each file covers a single service with deep-dive concepts, cracking interview Q&A, and a real-world architecture you can demo.

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
| 11 | [Capstone Architecture](./11-Real-World-Capstone-Architecture.md) | All 10 Services | Full end-to-end e-commerce platform using all 10 services — architecture diagram + narration |
| 12 | [7-Day Study Plan](./12-7-Day-Study-Plan.md) | Study Guide | Daily schedule, lab exercises mapped to your $100 credit budget, mock interview checklist |

---

## How to Use These Files

1. **Read** the Concepts section for each service first.  
2. **Answer** the interview questions out loud before reading the sample answers.  
3. **Build** the hands-on lab in your AWS account (see the study plan for credit budgets).  
4. **Architect** the real-world solution and practice explaining it on a whiteboard or in a Miro board.  
5. **Repeat** — revisit weak areas on Day 7.

---

> 💡 **Tip:** AWS Free Tier covers the basics. The $100 credits let you experiment with multi-AZ RDS, EKS, and larger EC2 instances without worry. Terminate resources after each lab session to stay within budget.
