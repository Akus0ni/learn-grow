# Cloud Computing Basics

## What is Cloud Computing?

Cloud computing is the **on-demand delivery** of computing resources — servers, storage, databases, networking, software, analytics, and intelligence — over the internet ("the cloud") with **pay-as-you-go pricing**. Instead of owning and maintaining physical data centers and servers, you rent access to technology services from a cloud provider.

---

## Why Cloud Computing?

| Benefit | Description |
|---|---|
| **Cost Efficiency** | No upfront capital expense; pay only for what you use |
| **Scalability** | Scale up or down instantly based on demand |
| **Global Reach** | Deploy applications worldwide in minutes |
| **Speed & Agility** | Provision resources in minutes, not weeks |
| **Reliability** | Data is mirrored across redundant sites for disaster recovery |
| **Security** | Major providers invest billions in security infrastructure |
| **Performance** | Worldwide network of secure data centers, regularly upgraded |

---

## Cloud Service Models

| Model | What You Manage | What Provider Manages | Example |
|---|---|---|---|
| **IaaS** (Infrastructure as a Service) | OS, Runtime, Apps, Data | Servers, Storage, Networking, Virtualization | AWS EC2, Azure VMs, GCP Compute Engine |
| **PaaS** (Platform as a Service) | Apps, Data | OS, Runtime, Servers, Storage, Networking | AWS Elastic Beanstalk, Azure App Service, GCP App Engine |
| **SaaS** (Software as a Service) | Nothing (just use it) | Everything | Gmail, Salesforce, Microsoft 365 |
| **FaaS / Serverless** | Code (functions) | Everything else | AWS Lambda, Azure Functions, GCP Cloud Functions |

---

## Cloud Deployment Models

| Model | Description | Best For |
|---|---|---|
| **Public Cloud** | Resources shared across multiple organizations on provider infrastructure | Startups, web apps, dev/test environments |
| **Private Cloud** | Dedicated infrastructure for a single organization | Banking, healthcare, government (strict compliance) |
| **Hybrid Cloud** | Combination of public and private clouds | Enterprises migrating gradually, burst workloads |
| **Multi-Cloud** | Using multiple public cloud providers simultaneously | Avoiding vendor lock-in, best-of-breed services |

---

## The Big Three: AWS vs Azure vs GCP

### Overview

| Aspect | AWS | Azure | GCP |
|---|---|---|---|
| **Launched** | 2006 | 2010 | 2008 |
| **Market Share (approx.)** | ~31% | ~25% | ~11% |
| **Regions** | 33+ | 60+ | 40+ |
| **Availability Zones** | 105+ | 300+ (including edge) | 121+ |
| **Strengths** | Broadest service catalog, mature ecosystem | Enterprise integration (Microsoft stack), hybrid cloud | Data analytics, ML/AI, Kubernetes |
| **Best For** | Startups, enterprises, any workload | Microsoft-centric shops, enterprise hybrid | Data-heavy workloads, ML/AI, containerized apps |
| **Pricing Model** | Pay-as-you-go, reserved, spot | Pay-as-you-go, reserved, spot, hybrid benefit | Pay-as-you-go, committed use, sustained use discounts |
| **Free Tier** | 12 months + always free | 12 months + always free | 90 days $300 credit + always free |

---

### When to Use Which Provider?

| Use Case | Recommended Provider | Why |
|---|---|---|
| **Startup / General Purpose** | AWS | Largest ecosystem, most tutorials/community support, broadest services |
| **Enterprise with Microsoft Stack** | Azure | Seamless integration with Active Directory, Office 365, .NET, SQL Server |
| **Big Data & Analytics** | GCP | BigQuery is best-in-class; strong Dataflow, Dataproc offerings |
| **Machine Learning / AI** | GCP or AWS | GCP has TensorFlow/Vertex AI; AWS has SageMaker with broadest ML ecosystem |
| **Kubernetes / Containers** | GCP | Google created Kubernetes; GKE is the most mature managed K8s |
| **Hybrid Cloud** | Azure | Azure Arc and Azure Stack provide strongest hybrid story |
| **Serverless** | AWS | Lambda pioneered serverless; deepest integration with event sources |
| **Gaming** | AWS or Azure | AWS GameLift; Azure PlayFab and Xbox integration |
| **IoT** | AWS | AWS IoT Core has the broadest IoT service suite |
| **Government / Compliance** | AWS or Azure | Both have dedicated GovCloud regions; Azure has strong government contracts |
| **Cost-Sensitive Workloads** | GCP | Sustained use discounts, per-second billing, preemptible VMs are cheapest |
| **CDN / Content Delivery** | AWS | CloudFront has 400+ edge locations and deep integration |
| **DevOps / CI-CD** | Azure or AWS | Azure DevOps is excellent; AWS CodePipeline is deeply integrated |

---

### Core Services Comparison

| Category | AWS | Azure | GCP |
|---|---|---|---|
| **Compute (VMs)** | EC2 | Virtual Machines | Compute Engine |
| **Serverless Compute** | Lambda | Functions | Cloud Functions |
| **Container Orchestration** | EKS / ECS | AKS | GKE |
| **App Hosting (PaaS)** | Elastic Beanstalk | App Service | App Engine |
| **Object Storage** | S3 | Blob Storage | Cloud Storage |
| **Block Storage** | EBS | Managed Disks | Persistent Disks |
| **File Storage** | EFS | Azure Files | Filestore |
| **Relational DB** | RDS / Aurora | Azure SQL / SQL Database | Cloud SQL / AlloyDB |
| **NoSQL DB** | DynamoDB | Cosmos DB | Firestore / Bigtable |
| **In-Memory Cache** | ElastiCache | Azure Cache for Redis | Memorystore |
| **Data Warehouse** | Redshift | Synapse Analytics | BigQuery |
| **VPC / Networking** | VPC | VNet | VPC |
| **Load Balancer** | ELB / ALB / NLB | Azure Load Balancer | Cloud Load Balancing |
| **DNS** | Route 53 | Azure DNS | Cloud DNS |
| **CDN** | CloudFront | Azure CDN / Front Door | Cloud CDN |
| **IAM** | IAM | Azure AD / Entra ID | Cloud IAM |
| **Monitoring** | CloudWatch | Azure Monitor | Cloud Monitoring (Ops Suite) |
| **ML Platform** | SageMaker | Azure ML | Vertex AI |
| **Message Queue** | SQS / SNS | Service Bus | Pub/Sub |
| **CI/CD** | CodePipeline / CodeBuild | Azure DevOps / Pipelines | Cloud Build |
| **IaC** | CloudFormation | ARM / Bicep Templates | Deployment Manager / Terraform |

---

### Certification Paths

| Provider | Entry Level | Associate | Professional/Expert |
|---|---|---|---|
| **AWS** | Cloud Practitioner | Solutions Architect Associate, Developer Associate, SysOps Associate | Solutions Architect Professional, DevOps Professional, Specialty certs |
| **Azure** | AZ-900 Fundamentals | AZ-104 Admin, AZ-204 Developer, AZ-500 Security | AZ-305 Solutions Architect Expert, AZ-400 DevOps Expert |
| **GCP** | Cloud Digital Leader | Associate Cloud Engineer | Professional Cloud Architect, Data Engineer, ML Engineer, etc. |

---

## Key Concepts to Remember

1. **Shared Responsibility Model** — The cloud provider secures the infrastructure; you secure your data, access, and configurations.
2. **Regions & Availability Zones** — Regions are geographic locations; AZs are isolated data centers within a region for fault tolerance.
3. **Elasticity vs Scalability** — Elasticity is automatic scaling; scalability is the ability to handle growth (may be manual).
4. **High Availability** — Designing systems to minimize downtime using multi-AZ/region deployments.
5. **Disaster Recovery** — Strategies: Backup & Restore → Pilot Light → Warm Standby → Multi-Site Active-Active.
6. **Well-Architected Framework** — All three providers have one: focuses on operational excellence, security, reliability, performance, cost optimization, and sustainability.
