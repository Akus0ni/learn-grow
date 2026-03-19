# Cloud Services Summary — What, Why & Where to Use

> A detailed cross-cloud comparison by use case. For each scenario: what the service does, why you'd choose a specific provider, and where it fits best.

---

## Compute

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Virtual Machines** | On-demand servers with full OS control | EC2 | Virtual Machines | Compute Engine | **AWS**: Broadest instance types, Graviton (ARM). **Azure**: Hybrid Benefit saves 85% with Windows licenses. **GCP**: Custom machine types, sustained use discounts, live migration. |
| **Serverless Functions** | Run code without managing servers, event-driven | Lambda | Functions | Cloud Functions | **AWS**: Largest event source ecosystem, mature. **Azure**: Durable Functions for workflows, PowerShell support. **GCP**: 2nd gen built on Cloud Run, 60-min timeout. |
| **Container Orchestration (Kubernetes)** | Managed Kubernetes clusters | EKS | AKS | GKE | **GCP (GKE)**: Google invented K8s; most mature, Autopilot mode. **Azure (AKS)**: Free control plane. **AWS (EKS)**: Deepest AWS integration. |
| **Serverless Containers** | Run containers without managing infrastructure | Fargate (ECS/EKS) | Container Apps | Cloud Run | **GCP (Cloud Run)**: True scale-to-zero, any container, per-request billing. **Azure (Container Apps)**: Built-in Dapr/KEDA. **AWS (Fargate)**: Tight ECS/EKS integration. |
| **PaaS App Hosting** | Deploy web apps without managing infra | Elastic Beanstalk | App Service | App Engine | **Azure**: Deployment slots, best .NET support. **GCP**: Standard env scales to zero. **AWS**: Supports Docker, broadest language support. |
| **Batch Processing** | Large-scale parallel compute jobs | Batch | Batch | Batch | **AWS**: Most mature. **GCP**: Simplest API. **Azure**: Strong HPC ecosystem with CycleCloud. |
| **Desktop as a Service** | Virtual desktops in the cloud | WorkSpaces | Azure Virtual Desktop | — | **Azure**: Only multi-session Windows 10/11, lowest cost with M365 licenses. **AWS**: Simpler setup for non-Microsoft shops. |

---

## Storage

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Object Storage** | Store unlimited unstructured data (files, media, backups) | S3 | Blob Storage | Cloud Storage | **AWS (S3)**: Most storage classes, deepest ecosystem integration. **GCP**: Uniform API across all classes, simpler pricing. **Azure**: Tight integration with Data Lake Storage Gen2. |
| **Block Storage** | High-performance disks for VMs | EBS | Managed Disks | Persistent Disks | **GCP**: Regional disks replicate across 2 zones automatically. **AWS**: io2 Block Express for highest IOPS. **Azure**: Ultra Disk for sub-ms latency. |
| **File Storage (NFS/SMB)** | Shared file systems across multiple servers | EFS (NFS), FSx | Azure Files (SMB/NFS) | Filestore (NFS) | **Azure**: Best Windows SMB support, File Sync for hybrid. **AWS**: FSx family covers Windows/Lustre/NetApp/OpenZFS. **GCP**: Simpler NFS offering. |
| **Archive Storage** | Long-term, rarely accessed data | S3 Glacier Deep Archive | Blob Archive tier | Cloud Storage Archive | **AWS**: Cheapest archive (~$1/TB/month). **GCP**: Same API as standard storage. **Azure**: Easy lifecycle policies in Blob. |
| **Offline Data Transfer** | Physically ship data to cloud | Snow Family (up to 100 PB) | Data Box (up to 1 PB) | Transfer Appliance (300 TB) | **AWS**: Largest capacity (Snowmobile). **Azure**: Good range of sizes. **GCP**: Smaller capacity but integrates well. |

---

## Databases

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Managed Relational DB** | MySQL, PostgreSQL, SQL Server as a service | RDS / Aurora | Azure SQL / DB for MySQL/PostgreSQL | Cloud SQL / AlloyDB | **AWS (Aurora)**: 5x MySQL performance. **GCP (AlloyDB)**: 4x PostgreSQL + analytical queries. **Azure (SQL)**: Best SQL Server PaaS, Hyperscale. |
| **Global Distributed SQL** | Strongly consistent, horizontally scalable relational DB | Aurora Global | Cosmos DB (SQL API) | Cloud Spanner | **GCP (Spanner)**: Only true globally consistent relational DB, 99.999% SLA. **AWS (Aurora Global)**: Simpler, regional primary with cross-region replicas. |
| **NoSQL Document DB** | Flexible schema document storage | DynamoDB | Cosmos DB | Firestore | **Azure (Cosmos DB)**: 5 consistency levels, 5 APIs, global multi-write. **AWS (DynamoDB)**: Single-digit ms, serverless, DAX cache. **GCP (Firestore)**: Real-time sync, offline support, Firebase integration. |
| **Wide-Column NoSQL** | Petabyte-scale, low-latency NoSQL | DynamoDB / Keyspaces | Cosmos DB (Cassandra API) | Bigtable | **GCP (Bigtable)**: Powers Google Search/Maps, best for time-series and IoT. **AWS (DynamoDB)**: Simpler, serverless. **Azure (Cosmos DB)**: Multi-model flexibility. |
| **In-Memory Cache** | Sub-millisecond data access | ElastiCache | Azure Cache for Redis | Memorystore | **AWS**: Redis + Memcached options. **Azure**: Enterprise tier with Redis modules. **GCP**: Simpler, managed Redis/Memcached. |
| **Data Warehouse** | Analytical queries on large datasets | Redshift | Synapse Analytics | BigQuery | **GCP (BigQuery)**: Best-in-class serverless, per-query pricing, 1 TB/month free. **AWS (Redshift)**: Mature, Spectrum queries S3. **Azure (Synapse)**: Unified workspace with Spark + SQL. |
| **Graph Database** | Relationship-heavy queries | Neptune | Cosmos DB (Gremlin API) | — (use Neo4j on GCP) | **AWS (Neptune)**: Purpose-built graph DB. **Azure**: Cosmos DB Gremlin API for multi-model. |
| **Time-Series DB** | Sensor data, metrics, IoT telemetry | Timestream | Azure Data Explorer | Bigtable / BigQuery | **Azure (ADX)**: KQL is powerful, real-time ingestion. **AWS (Timestream)**: Purpose-built, auto-tiering. **GCP**: Use Bigtable for high-throughput or BigQuery for analytics. |

---

## Networking

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Virtual Network** | Isolated cloud network | VPC (regional) | VNet (regional) | VPC (global) | **GCP**: VPC is global by default — subnets span zones, simplifying multi-region. **AWS/Azure**: Regional VPCs offer more isolation control. |
| **Load Balancing** | Distribute traffic across targets | ELB (ALB/NLB) | Load Balancer + App Gateway | Cloud Load Balancing | **GCP**: Single anycast IP for global HTTP LB — unique. **AWS**: ALB for L7, NLB for ultra-low latency. **Azure**: App Gateway combines L7 LB + WAF. |
| **CDN** | Edge caching for content delivery | CloudFront | Front Door / Azure CDN | Cloud CDN | **AWS (CloudFront)**: 400+ edge locations, Lambda@Edge. **Azure (Front Door)**: Combines CDN + global LB + WAF. **GCP**: Simpler, integrates with Cloud LB. |
| **DNS** | Domain name resolution | Route 53 | Azure DNS | Cloud DNS | **GCP**: 100% SLA (only provider). **AWS**: Most routing policies (geo, latency, failover, weighted). **Azure**: Private DNS zones for VNet resolution. |
| **DDoS Protection** | Shield against DDoS attacks | Shield (Standard free, Advanced $3K/mo) | DDoS Protection (Basic free, Standard) | Cloud Armor (always on) | **GCP**: ML-based adaptive protection included. **AWS**: Advanced has 24/7 DRT team. **Azure**: Standard tier is simpler to enable. |
| **Web Application Firewall** | Protect apps from L7 exploits | WAF | App Gateway WAF / Front Door WAF | Cloud Armor | **AWS**: Most managed rule groups. **GCP**: Adaptive protection with ML. **Azure**: Integrated into App Gateway and Front Door. |
| **Dedicated Connection** | Private link to cloud (not over internet) | Direct Connect | ExpressRoute | Cloud Interconnect | **Azure**: Global Reach connects ExpressRoute circuits. **AWS**: Most locations. **GCP**: Cross-Cloud Interconnect connects directly to other clouds. |
| **API Management** | Publish, manage, and secure APIs | API Gateway | API Management | Apigee | **GCP (Apigee)**: Most full-featured (monetization, developer portal). **Azure (APIM)**: Good developer portal + policies. **AWS**: Simplest, tightly integrated with Lambda. |

---

## Security & Identity

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Identity & Access** | Control who can do what | IAM | Entra ID (Azure AD) | Cloud IAM | **Azure**: SSO with Microsoft 365, conditional access, B2B/B2C. **AWS**: Most granular policies. **GCP**: Simplest role-based model. |
| **Secrets Management** | Store passwords, keys, tokens securely | Secrets Manager | Key Vault | Secret Manager | **AWS**: Native auto-rotation for RDS/Redshift. **Azure**: Combines secrets + keys + certificates in one. **GCP**: Simplest, IAM-integrated. |
| **Encryption Key Management** | Manage cryptographic keys | KMS | Key Vault | Cloud KMS | **GCP**: External Key Manager (EKM) for keys outside Google. **AWS**: Deep integration with 100+ services. **Azure**: HSM-backed by default in Key Vault. |
| **Threat Detection / SIEM** | Detect threats and security events | GuardDuty + Security Hub | Microsoft Sentinel | Security Command Center + Chronicle | **Azure (Sentinel)**: Best SIEM with 200+ connectors, AI-driven. **GCP (Chronicle)**: Fixed pricing regardless of volume. **AWS (GuardDuty)**: Easy to enable, ML-based. |
| **Compliance & Governance** | Enforce org policies and standards | Config + Organizations SCPs | Azure Policy + Management Groups | Organization Policy + Resource Manager | **Azure**: Most granular policy engine (resource-level). **AWS**: SCPs at org level. **GCP**: Organization policies with custom constraints. |
| **Zero Trust Access** | Secure access without VPN | Verified Access | Entra Conditional Access | BeyondCorp Enterprise | **GCP**: Pioneered zero trust with BeyondCorp. **Azure**: Conditional access with Entra ID. **AWS**: Verified Access is newer. |

---

## AI & Machine Learning

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Full ML Platform** | Build, train, deploy ML models end-to-end | SageMaker | Azure ML | Vertex AI | **AWS (SageMaker)**: Broadest feature set. **GCP (Vertex AI)**: Unified, strong AutoML. **Azure**: Best MLOps, responsible AI dashboard. |
| **Generative AI / LLMs** | Access and deploy large language models | Bedrock (Claude, Llama, Titan) | Azure OpenAI (GPT-4, GPT-4o) | Gemini on Vertex AI | **Azure**: Exclusive OpenAI access with enterprise features. **AWS (Bedrock)**: Multiple LLM providers (Claude, Llama). **GCP**: Native Gemini, TPU hardware advantage. |
| **Pre-built AI APIs** | Vision, speech, language, translation without ML expertise | Rekognition, Transcribe, Polly, Comprehend, Translate | Azure AI Services (Vision, Speech, Language) | Vision AI, Speech, Natural Language, Translation | **GCP**: Powered by Google's research (best translation, NLP). **AWS**: Broadest set of individual services. **Azure**: Best integration with Microsoft apps. |
| **Conversational AI** | Build chatbots and virtual agents | Lex | Bot Service | Dialogflow | **GCP (Dialogflow CX)**: Most advanced conversation design. **Azure**: Deep Teams integration. **AWS (Lex)**: Alexa technology. |
| **ML Hardware** | Specialized hardware for training/inference | Inferentia/Trainium | GPU VMs (NVIDIA) | TPU | **GCP (TPU)**: Custom silicon, best for TensorFlow/JAX. **AWS (Trainium)**: Cost-effective custom silicon. **Azure**: Best GPU availability for LLM training. |
| **Document AI** | Extract data from documents (OCR, forms) | Textract | Document Intelligence | Document AI | **AWS (Textract)**: Best form/table extraction. **GCP**: Pre-built processors for invoices, receipts, IDs. **Azure**: Strong handwriting and layout analysis. |
| **Recommendations** | Personalized product/content recommendations | Personalize | — (use Azure ML) | Recommendations AI | **AWS (Personalize)**: Amazon.com technology. **GCP**: Google's recommendation infrastructure. |

---

## Analytics & Big Data

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **Data Warehousing** | Run SQL analytics on large datasets | Redshift | Synapse Analytics | BigQuery | **GCP (BigQuery)**: Serverless, per-query pricing, built-in ML (BQML). **AWS (Redshift)**: Mature, RA3 nodes separate compute/storage. **Azure (Synapse)**: Unified analytics workspace. |
| **Real-Time Streaming** | Process data in real-time as it arrives | Kinesis | Event Hubs + Stream Analytics | Pub/Sub + Dataflow | **AWS (Kinesis)**: Easiest to set up. **GCP (Dataflow)**: Apache Beam, best for complex transformations. **Azure**: Event Hubs is Kafka-compatible, Stream Analytics uses SQL. |
| **ETL / Data Integration** | Transform and move data between systems | Glue | Data Factory | Dataflow / Data Fusion | **AWS (Glue)**: Serverless Spark ETL. **Azure (Data Factory)**: 100+ connectors, visual pipelines. **GCP (Dataflow)**: Unified batch + stream processing. |
| **Business Intelligence** | Dashboards, reports, visual analytics | QuickSight | Power BI | Looker / Looker Studio | **Azure (Power BI)**: Most powerful BI tool, massive adoption, DAX. **GCP (Looker)**: Best semantic layer (LookML), Looker Studio is free. **AWS (QuickSight)**: Serverless, pay-per-session. |
| **Managed Hadoop/Spark** | Run open-source big data frameworks | EMR | HDInsight / Databricks | Dataproc | **GCP (Dataproc)**: 90-second cluster provisioning, cheapest with preemptible VMs. **AWS (EMR)**: Most mature, Serverless option. **Azure (Databricks)**: Best Spark experience, lakehouse architecture. |
| **Search** | Full-text and vector search | OpenSearch Service | Azure AI Search | Vertex AI Search | **Azure (AI Search)**: Best for RAG with Azure OpenAI. **AWS (OpenSearch)**: Open-source based, log analytics. **GCP**: Turnkey search with minimal code. |
| **Data Governance** | Catalog, lineage, quality, access control | Glue Data Catalog + Lake Formation | Microsoft Purview | Dataplex | **Azure (Purview)**: Scans multi-cloud and on-prem. **GCP (Dataplex)**: Unifies governance across BigQuery + GCS. **AWS**: Lake Formation for fine-grained S3/Glue access. |

---

## DevOps & CI/CD

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **CI/CD Pipeline** | Automate build, test, deploy | CodePipeline + CodeBuild | Azure DevOps Pipelines | Cloud Build | **Azure (DevOps)**: Most complete all-in-one DevOps platform. **AWS**: Deepest AWS integration. **GCP**: Container-native builds, supply chain security. |
| **Infrastructure as Code** | Define infrastructure in code | CloudFormation / CDK | ARM / Bicep | Deployment Manager / Terraform | **AWS (CDK)**: Real programming languages (TypeScript, Python). **Azure (Bicep)**: Clean DSL, simpler than ARM JSON. **All**: Terraform works everywhere (recommended for multi-cloud). |
| **Monitoring** | Metrics, logs, alerting | CloudWatch | Azure Monitor | Cloud Monitoring + Logging | **Azure**: Application Insights for APM is excellent. **AWS (CloudWatch)**: Deepest AWS integration. **GCP**: Free tier is generous; integrates with BigQuery for log analytics. |
| **Distributed Tracing** | Debug latency in distributed systems | X-Ray | Application Insights | Cloud Trace | **Azure (App Insights)**: Auto-instrumentation for .NET/Java. **AWS (X-Ray)**: Lambda and ECS integration. **GCP**: OpenTelemetry-native. |

---

## Migration

| Use Case | What | AWS | Azure | GCP | Why Choose This Provider |
|---|---|---|---|---|---|
| **VM Migration** | Move VMs from on-prem or other clouds | Application Migration Service | Azure Migrate | Migrate for Compute Engine | **Azure**: Central migration hub with assessment tools. **AWS**: Most agentless options. **GCP**: Supports direct migration from AWS/Azure. |
| **Database Migration** | Move databases to cloud with minimal downtime | DMS + SCT | Azure Database Migration Service | Database Migration Service | **AWS (DMS)**: Supports most source/target combinations. **Azure**: Best for SQL Server migrations. **GCP**: Simplest for MySQL/PostgreSQL to Cloud SQL/AlloyDB. |
| **Hybrid Cloud** | Extend cloud to on-prem / multi-cloud | Outposts | Azure Arc + Stack | Anthos | **Azure (Arc)**: Manage any infrastructure as Azure resources, broadest hybrid story. **GCP (Anthos)**: Run GKE on AWS/Azure/on-prem. **AWS (Outposts)**: AWS hardware in your datacenter. |

---

## Quick Decision Guide

| If You Need... | Go With | Reason |
|---|---|---|
| Broadest service catalog | **AWS** | 200+ services, most mature |
| Microsoft ecosystem integration | **Azure** | AD, Office 365, .NET, SQL Server, Teams |
| Best data analytics | **GCP** | BigQuery is unmatched for serverless analytics |
| Best Kubernetes experience | **GCP** | Google created K8s; GKE Autopilot is the best managed K8s |
| Best serverless containers | **GCP** | Cloud Run is the simplest and most flexible |
| Best enterprise AI/GenAI | **Azure** | Exclusive OpenAI access + enterprise security |
| Multi-model global NoSQL | **Azure** | Cosmos DB offers 5 APIs with global distribution |
| Cheapest compute | **GCP** | Sustained use discounts, custom machine types, preemptible VMs |
| Strongest hybrid cloud | **Azure** | Arc + Stack + Hybrid Benefit licensing |
| Best ML platform | **AWS** | SageMaker has the broadest feature set |
| Best BI tool | **Azure** | Power BI dominates the BI market |
| Best free tier for learning | **GCP** | $300 credit for 90 days + always free tier |
| Avoiding vendor lock-in | **Use Terraform + Kubernetes + open-source** | Cloud-agnostic tooling |
