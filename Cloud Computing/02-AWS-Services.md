# AWS (Amazon Web Services) — Services Quick Reference

> Comprehensive table of AWS services for interview preparation and quick revision.

---

## Compute Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **EC2** (Elastic Compute Cloud) | Virtual servers in the cloud | Instance types (general, compute, memory, GPU optimized), Auto Scaling, Spot/Reserved/On-Demand pricing, AMIs | Web servers, app hosting, batch processing, any workload needing full OS control | Know instance families: T (burstable), M (general), C (compute), R (memory), P/G (GPU) |
| **Lambda** | Serverless compute — run code without provisioning servers | Event-driven, auto-scales to zero, pay per invocation (ms billing), supports Python/Node/Java/Go/.NET, 15-min max timeout | Event-driven tasks, APIs (with API Gateway), file processing, cron jobs, microservices | Max 15-min execution, 10 GB memory, 512 MB–10 GB ephemeral storage |
| **ECS** (Elastic Container Service) | Docker container orchestration on AWS | Launch types: Fargate (serverless) or EC2, integrates with ALB, deep AWS integration | Running Docker containers without managing K8s | Fargate = serverless containers; EC2 launch type = you manage the instances |
| **EKS** (Elastic Kubernetes Service) | Managed Kubernetes | Runs upstream K8s, supports Fargate and EC2 nodes, integrates with IAM/VPC | Teams already using Kubernetes, multi-cloud K8s strategy | GKE is considered more mature, but EKS has deepest AWS integration |
| **Elastic Beanstalk** | PaaS — deploy apps without managing infra | Supports Java, .NET, Node, Python, Ruby, Go, Docker; auto-provisions EC2, ELB, ASG | Quick deployments, developers who don't want to manage infrastructure | It uses EC2, ELB, ASG under the hood — not serverless |
| **Lightsail** | Simple virtual private servers | Fixed monthly pricing, includes compute/storage/networking, pre-configured apps | Simple websites, blogs, small applications, dev/test | Think of it as "simplified EC2" for small projects |
| **Batch** | Managed batch computing | Dynamically provisions optimal compute resources, job queues and scheduling | Large-scale batch jobs, HPC, ML training data processing | Automatically manages compute environment based on job requirements |
| **Outposts** | AWS infrastructure on-premises | Fully managed, same AWS APIs/tools on-prem | Low-latency on-prem needs, data residency requirements | Brings AWS to your data center, not the other way around |
| **App Runner** | Fully managed container app service | Auto-builds/deploys from source or container image, auto-scales | Web apps and APIs that need quick deployment | Simpler than ECS/EKS for straightforward web services |

---

## Storage Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **S3** (Simple Storage Service) | Object storage | Unlimited storage, 99.999999999% (11 9s) durability, versioning, lifecycle policies, storage classes (Standard, IA, Glacier, Deep Archive) | Static website hosting, backups, data lakes, media storage | Know storage classes: Standard → IA → One Zone-IA → Glacier Instant → Glacier Flexible → Deep Archive |
| **EBS** (Elastic Block Store) | Block storage volumes for EC2 | SSD (gp3, io2) and HDD (st1, sc1) types, snapshots, encryption, multi-attach (io2) | OS volumes, databases, high-performance apps | Tied to a single AZ; use snapshots to move across AZs/regions |
| **EFS** (Elastic File System) | Managed NFS file system | Multi-AZ, auto-scaling, POSIX-compliant, supports thousands of concurrent connections | Shared file storage across multiple EC2 instances, CMS, ML training data | Linux only; for Windows use FSx for Windows |
| **S3 Glacier** | Archival storage | Glacier Instant (ms retrieval), Flexible (mins-hours), Deep Archive (12-48 hrs) | Long-term backups, compliance archives, rarely accessed data | Deep Archive is cheapest storage in AWS (~$1/TB/month) |
| **FSx** | Managed third-party file systems | FSx for Windows (SMB), FSx for Lustre (HPC), FSx for NetApp ONTAP, FSx for OpenZFS | Windows file shares, HPC workloads, lift-and-shift | FSx for Lustre integrates with S3 for HPC data processing |
| **Storage Gateway** | Hybrid cloud storage | File Gateway, Volume Gateway, Tape Gateway | Connecting on-prem storage to AWS cloud | Bridge between on-prem and cloud; caches frequently accessed data locally |
| **Snow Family** | Physical data transfer devices | Snowcone (8-14 TB), Snowball Edge (80 TB), Snowmobile (100 PB) | Migrating petabytes of data, edge computing in remote locations | When network transfer would take weeks/months |

---

## Database Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **RDS** (Relational Database Service) | Managed relational databases | Supports MySQL, PostgreSQL, MariaDB, Oracle, SQL Server; Multi-AZ, Read Replicas, automated backups | Traditional relational workloads, OLTP | Multi-AZ = high availability (sync replication); Read Replicas = performance (async replication) |
| **Aurora** | AWS-optimized relational DB | MySQL/PostgreSQL compatible, 5x faster than MySQL, 3x faster than PostgreSQL, auto-scales storage to 128 TB, 6 copies across 3 AZs | High-performance relational workloads, replacing commercial DBs | Aurora Serverless v2 scales to zero; Aurora Global Database for cross-region |
| **DynamoDB** | Managed NoSQL (key-value + document) | Single-digit ms latency, auto-scaling, global tables, DAX (in-memory cache), TTL, streams | Serverless backends, gaming leaderboards, IoT, session management | Know partition key vs sort key; DynamoDB Streams for change data capture |
| **ElastiCache** | Managed in-memory caching | Redis and Memcached engines, sub-ms latency, cluster mode | Session stores, caching DB queries, real-time analytics, leaderboards | Redis = persistence + complex data types; Memcached = simple, multi-threaded |
| **Redshift** | Data warehouse | Columnar storage, massive parallel processing (MPP), Redshift Spectrum (query S3), Serverless option | Business intelligence, analytics on structured/semi-structured data | OLAP not OLTP; Spectrum queries S3 data without loading it |
| **DocumentDB** | Managed document DB (MongoDB compatible) | MongoDB API compatible, auto-scales, multi-AZ | MongoDB workloads on AWS | Not actual MongoDB — it's AWS-built with MongoDB API compatibility |
| **Neptune** | Managed graph database | Supports Gremlin and SPARQL, fast graph queries | Social networks, knowledge graphs, fraud detection, recommendation engines | Graph DB for relationship-heavy queries |
| **Keyspaces** | Managed Apache Cassandra | Cassandra-compatible, serverless, auto-scales | Cassandra workloads, high-throughput time-series | Serverless Cassandra without managing clusters |
| **Timestream** | Time-series database | Purpose-built for IoT and operational data, auto-scales | IoT sensor data, DevOps metrics, application monitoring | 1000x faster and 1/10th cost vs relational DBs for time-series |
| **QLDB** | Quantum Ledger Database | Immutable, cryptographically verifiable transaction log | Audit trails, supply chain, financial ledgers | Immutable history — think "blockchain without decentralization" |
| **MemoryDB** | Redis-compatible durable database | Ultra-fast with multi-AZ durability, microsecond reads | Primary database for Redis workloads needing durability | Unlike ElastiCache, MemoryDB provides durability as a primary DB |

---

## Networking & Content Delivery

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **VPC** (Virtual Private Cloud) | Isolated virtual network | Subnets (public/private), route tables, internet/NAT gateways, security groups, NACLs, VPC peering, VPC endpoints | Foundation for all AWS networking | Security Groups = stateful; NACLs = stateless |
| **Route 53** | DNS and domain registration | DNS routing (simple, weighted, latency, failover, geolocation, multivalue), health checks, domain registration | DNS management, traffic routing, domain purchases | Named after port 53 (DNS); supports alias records for AWS resources |
| **CloudFront** | CDN (Content Delivery Network) | 400+ edge locations, Lambda@Edge, signed URLs/cookies, origin access control, WebSocket support | Static/dynamic content delivery, streaming, API acceleration | Lambda@Edge runs code at edge locations; OAC restricts S3 access |
| **ELB** (Elastic Load Balancing) | Load balancer | ALB (Layer 7/HTTP), NLB (Layer 4/TCP/UDP), GLB (Layer 3/network appliances), CLB (legacy) | Distributing traffic across targets | ALB = path/host routing; NLB = ultra-low latency, static IP; GLB = third-party appliances |
| **API Gateway** | Managed API service | REST/HTTP/WebSocket APIs, throttling, caching, auth, usage plans | Building RESTful APIs, serverless backends (with Lambda) | Two types: REST API (more features) vs HTTP API (cheaper, simpler) |
| **Direct Connect** | Dedicated network connection to AWS | 1/10/100 Gbps, consistent latency, private connectivity | Hybrid cloud, large data transfers, regulatory compliance | Takes weeks to set up; use VPN as backup or during provisioning |
| **Global Accelerator** | Network layer traffic optimizer | Anycast IPs, routes traffic over AWS global network | Multi-region apps needing consistent global performance | Unlike CloudFront, it's for non-HTTP use cases (TCP/UDP) and doesn't cache |
| **Transit Gateway** | Central hub for connecting VPCs and on-prem | Simplifies network topology, supports thousands of VPCs, multicast | Complex multi-VPC architectures, hub-and-spoke networks | Replaces complex VPC peering meshes |
| **PrivateLink** | Private connectivity to services | Access AWS/third-party services without traversing the internet | Secure SaaS connectivity, keeping traffic on AWS network | Creates ENIs in your VPC for private access |

---

## Security, Identity & Compliance

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **IAM** (Identity and Access Management) | Access control service | Users, groups, roles, policies (JSON), MFA, cross-account access, identity federation | Every AWS deployment — it's the foundation of security | Principle of least privilege; policies have Effect/Action/Resource/Condition |
| **Cognito** | User identity for apps | User Pools (authentication), Identity Pools (authorization/federation), social sign-in, MFA | Adding user signup/login to web/mobile apps | User Pools = auth; Identity Pools = temporary AWS credentials |
| **KMS** (Key Management Service) | Managed encryption keys | Create/manage symmetric and asymmetric keys, automatic rotation, integrated with 100+ AWS services | Encrypting data at rest and in transit | KMS keys can't be exported; CloudHSM for FIPS 140-2 Level 3 |
| **Secrets Manager** | Secret storage and rotation | Auto-rotates secrets, integrates with RDS/Redshift/DocumentDB, version control | Storing DB passwords, API keys, tokens | Auto-rotation is the key differentiator vs Parameter Store |
| **WAF** (Web Application Firewall) | Layer 7 firewall | SQL injection, XSS protection, rate limiting, bot control, managed rules | Protecting web apps and APIs from common exploits | Deploys on CloudFront, ALB, API Gateway, AppSync |
| **Shield** | DDoS protection | Standard (free, auto-enabled), Advanced ($3K/month, 24/7 DRT, cost protection) | Protection against DDoS attacks | Shield Standard is free and always on; Advanced for mission-critical apps |
| **GuardDuty** | Threat detection | ML-based, analyzes CloudTrail/VPC Flow Logs/DNS logs, finds malicious activity | Continuous threat monitoring across accounts | Enable it — it's a single click and works automatically |
| **Inspector** | Vulnerability assessment | Scans EC2/ECR/Lambda for software vulnerabilities and network exposure | Automated vulnerability management | Agentless scanning for Lambda and ECR |
| **Certificate Manager (ACM)** | SSL/TLS certificate management | Free public certs, auto-renewal, integrates with ELB/CloudFront/API Gateway | HTTPS for your applications | Free for public certs on AWS services; can't export private keys |
| **Security Hub** | Central security dashboard | Aggregates findings from GuardDuty/Inspector/Macie/etc., compliance checks | Single pane of glass for security posture | Integrates with 65+ AWS and third-party services |
| **Macie** | Data privacy/PII detection | ML-powered, scans S3 for sensitive data (PII, PHI, financial) | Data privacy compliance, GDPR/HIPAA discovery | Specifically designed for S3 data classification |

---

## Application Integration

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **SQS** (Simple Queue Service) | Managed message queue | Standard (at-least-once, best-effort ordering) and FIFO (exactly-once, ordered), DLQ, long polling | Decoupling microservices, buffering requests, async processing | Standard = unlimited throughput; FIFO = 300 msg/s (3000 with batching) |
| **SNS** (Simple Notification Service) | Pub/sub messaging | Topics, fan-out to multiple subscribers (SQS, Lambda, HTTP, email, SMS), message filtering | Event notifications, fan-out patterns, alerts | SNS + SQS fan-out is a classic architecture pattern |
| **EventBridge** | Serverless event bus | Rules, event patterns, schema registry, 35+ AWS sources, SaaS integrations, scheduled events | Event-driven architectures, SaaS integration, cron-like scheduling | Successor to CloudWatch Events; more features and SaaS sources |
| **Step Functions** | Workflow orchestration | Visual workflows, state machines, error handling, parallel execution, integrates with 220+ services | Orchestrating microservices, ETL pipelines, approval workflows | Standard (long-running, exactly-once) vs Express (high-volume, at-least-once) |
| **SWF** (Simple Workflow) | Legacy workflow service | Task-oriented, decider programs | Legacy apps — use Step Functions for new work | Effectively replaced by Step Functions |
| **MQ** | Managed message broker | Apache ActiveMQ and RabbitMQ, supports JMS/AMQP/MQTT/STOMP | Migrating existing message broker workloads to cloud | Use for lift-and-shift of existing broker apps; SQS/SNS for cloud-native |

---

## Management & Monitoring

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **CloudWatch** | Monitoring and observability | Metrics, alarms, logs, dashboards, custom metrics, anomaly detection, Logs Insights | Monitoring any AWS resource, application logging, alerting | Custom metrics for app-level data; Logs Insights for querying logs |
| **CloudTrail** | API activity logging | Records every API call, management and data events, multi-region, organization trails | Auditing, compliance, security analysis, troubleshooting | Enabled by default for 90 days; create a trail for permanent storage |
| **CloudFormation** | Infrastructure as Code | JSON/YAML templates, stack sets, drift detection, change sets, nested stacks | Automating infrastructure provisioning, repeatable deployments | AWS-native IaC; Terraform is the multi-cloud alternative |
| **Systems Manager** | Operations management | Session Manager (SSH without port 22), Parameter Store, Patch Manager, Run Command, Automation | Managing EC2 fleets, storing config, patching, remote access | Parameter Store is free for standard params; Session Manager replaces bastion hosts |
| **Config** | Resource configuration tracking | Configuration history, compliance rules, conformance packs, auto-remediation | Compliance auditing, tracking configuration changes | Records WHO changed WHAT and WHEN for every resource |
| **Trusted Advisor** | Best practice recommendations | Cost optimization, security, fault tolerance, performance, service limits | Identifying cost savings, security gaps, performance improvements | Basic checks free; full checks require Business/Enterprise support |
| **Organizations** | Multi-account management | Consolidated billing, SCPs (Service Control Policies), OUs | Managing multiple AWS accounts, enterprise governance | SCPs restrict what accounts CAN do (deny lists or allow lists) |
| **Control Tower** | Multi-account setup/governance | Landing zone, guardrails, account factory | Setting up a well-architected multi-account AWS environment | Builds on Organizations with best-practice guardrails |

---

## Analytics & Big Data

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Athena** | Serverless query service | SQL on S3 data, pay per query, supports CSV/JSON/Parquet/ORC, integrates with Glue Catalog | Ad-hoc querying of S3 data lakes, log analysis | Serverless Presto/Trino under the hood; $5 per TB scanned |
| **EMR** (Elastic MapReduce) | Managed Hadoop/Spark | Spark, Hive, Presto, HBase, Flink; on EC2 or EKS or Serverless | Large-scale data processing, ETL, ML training | EMR Serverless for intermittent workloads |
| **Kinesis** | Real-time data streaming | Data Streams (custom processing), Firehose (load to destinations), Analytics (SQL on streams), Video Streams | Real-time analytics, log aggregation, IoT data ingestion | Data Streams = custom; Firehose = delivery to S3/Redshift/etc. |
| **Glue** | Serverless ETL | Crawlers (schema discovery), ETL jobs (Spark), Data Catalog, DataBrew (visual ETL) | Data preparation, cataloging, ETL pipelines | Glue Data Catalog is the central metadata repository |
| **QuickSight** | Business intelligence | ML insights, embedded dashboards, SPICE in-memory engine, pay-per-session | Dashboards, reports, embedded analytics | Serverless BI; pay-per-session pricing is unique |
| **Lake Formation** | Data lake builder | Centralized permissions, data sharing, governed tables | Building and managing secure data lakes | Fine-grained access control on top of S3/Glue |
| **MSK** (Managed Streaming for Kafka) | Managed Apache Kafka | Fully compatible, MSK Connect, MSK Serverless | Kafka workloads, event streaming | Use MSK for Kafka migrations; Kinesis for AWS-native streaming |
| **OpenSearch Service** | Search and analytics | Elasticsearch/OpenSearch compatible, dashboards, alerting, anomaly detection | Log analytics, full-text search, application monitoring | Formerly Amazon Elasticsearch Service |

---

## Machine Learning & AI

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **SageMaker** | Full ML platform | Build, train, deploy models; notebooks, experiments, pipelines, model registry, endpoints | End-to-end ML workflows | Most comprehensive ML platform on any cloud |
| **Bedrock** | Managed generative AI | Foundation models (Claude, Llama, Titan, etc.), RAG, fine-tuning, agents | Building GenAI applications without managing infrastructure | Access to multiple LLMs via a single API |
| **Rekognition** | Image and video analysis | Face detection, object detection, text in images, content moderation, custom labels | Image moderation, facial analysis, visual search | Pre-trained — no ML expertise needed |
| **Comprehend** | NLP service | Sentiment analysis, entity extraction, topic modeling, language detection | Text analysis, customer feedback analysis, document classification | Comprehend Medical for healthcare text |
| **Polly** | Text-to-speech | 60+ languages, neural voices, SSML support, real-time streaming | Voice-enabled apps, accessibility, content creation | Neural TTS for human-like speech |
| **Transcribe** | Speech-to-text | Real-time and batch, custom vocabulary, speaker identification, auto-language detection | Call center analytics, subtitling, meeting transcription | Transcribe Medical for clinical documentation |
| **Translate** | Machine translation | 75+ languages, real-time and batch, custom terminology, active custom translation | Multilingual applications, content localization | Can customize with parallel data for domain-specific terms |
| **Lex** | Conversational AI | Powers Alexa, NLU, ASR, multi-turn conversations | Chatbots, virtual assistants, IVR systems | Same tech behind Alexa |
| **Textract** | Document OCR/extraction | Tables, forms, handwriting, queries, identity documents | Invoice processing, form extraction, ID verification | Goes beyond OCR — understands document structure |
| **Personalize** | Recommendation engine | Real-time personalization, user segmentation, similar items | Product recommendations, content personalization | Same tech behind Amazon.com recommendations |

---

## Developer Tools

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **CodeCommit** | Managed Git repos | Private Git hosting, integrates with IAM, pull requests | Source control within AWS ecosystem | Being phased out — consider GitHub/GitLab |
| **CodeBuild** | Managed build service | Compiles, tests, produces packages; pay per build minute, custom build environments | CI builds, testing, artifact generation | Serverless — no build servers to manage |
| **CodeDeploy** | Deployment automation | EC2, Lambda, ECS deployments; blue/green, rolling, canary strategies | Automated application deployments | Works with on-prem servers too |
| **CodePipeline** | CI/CD pipeline | Orchestrates CodeCommit → CodeBuild → CodeDeploy, third-party integrations | Full CI/CD automation | The orchestrator; individual tools do the actual work |
| **CDK** (Cloud Development Kit) | IaC using programming languages | TypeScript, Python, Java, C#, Go; synthesizes to CloudFormation | Developers who prefer code over YAML/JSON for IaC | Generates CloudFormation under the hood |
| **X-Ray** | Distributed tracing | Service map, trace analysis, sampling, integrates with Lambda/ECS/EC2 | Debugging distributed applications, finding bottlenecks | Adds trace headers to requests flowing through services |
| **CloudShell** | Browser-based shell | Pre-authenticated CLI, 1 GB persistent storage, pre-installed tools | Quick CLI tasks without local setup | Free; persists home directory between sessions |

---

## Migration & Transfer

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **DMS** (Database Migration Service) | Database migration | Supports homogeneous and heterogeneous migrations, continuous replication, Schema Conversion Tool | Migrating databases to AWS with minimal downtime | SCT converts schemas; DMS migrates data |
| **Migration Hub** | Migration tracking | Central dashboard for tracking migrations, integrates with partner tools | Coordinating large migration projects | Single pane of glass for migration progress |
| **Application Discovery Service** | On-prem discovery | Agent-based or agentless, discovers servers, dependencies, utilization | Planning migrations — understanding your current estate | Run this first before planning a migration |
| **DataSync** | Data transfer service | Automated, encrypted, up to 10x faster than open-source tools, S3/EFS/FSx destinations | One-time or recurring data transfers to AWS | Faster and simpler than custom scripts |
| **Transfer Family** | Managed SFTP/FTPS/FTP | S3 or EFS backend, integrates with AD/LDAP | Migrating file transfer workflows to AWS | Replaces on-prem SFTP servers |
