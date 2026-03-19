# Microsoft Azure — Services Quick Reference

> Comprehensive table of Azure services for interview preparation and quick revision.

---

## Compute Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Virtual Machines** | IaaS VMs in the cloud | Windows/Linux, B/D/E/F/G/H/L/M/N series, availability sets/zones, Spot VMs, Reserved Instances | Full OS control, lift-and-shift, Windows Server workloads | Azure Hybrid Benefit saves up to 85% with existing Windows/SQL licenses |
| **Azure Functions** | Serverless compute | Event-driven, Consumption/Premium/Dedicated plans, Durable Functions for workflows, supports C#/JS/Python/Java/PowerShell | Event-driven tasks, microservices, APIs, automation | Durable Functions enable stateful orchestration patterns |
| **App Service** | PaaS for web apps | .NET, Java, Node, Python, PHP, Ruby; deployment slots, auto-scale, SSL, custom domains | Web apps, REST APIs, mobile backends | Deployment slots enable blue/green with zero downtime swap |
| **AKS** (Azure Kubernetes Service) | Managed Kubernetes | Free control plane, Azure AD integration, Azure CNI networking, virtual nodes (ACI), KEDA auto-scaling | Container orchestration, microservices, K8s workloads | Control plane is free — you only pay for worker nodes |
| **Container Instances (ACI)** | Serverless containers | Fast startup, per-second billing, no orchestration overhead, GPU support | Simple container workloads, burst scaling from AKS, CI/CD jobs | Fastest way to run a container in Azure — no infra to manage |
| **Container Apps** | Serverless container platform | Built on K8s/KEDA/Dapr/Envoy, auto-scale to zero, revisions, traffic splitting | Microservices, event-driven apps, APIs without managing K8s | Simpler than AKS; more capable than ACI |
| **Azure Spring Apps** | Managed Spring Boot platform | Built-in service discovery, config management, blue/green deployment | Enterprise Java/Spring Boot microservices | Joint engineering with VMware/Tanzu |
| **Batch** | Managed batch computing | Auto-scales to thousands of VMs, job scheduling, low-priority VMs for cost savings | HPC, rendering, large-scale parallel processing | Supports Linux and Windows; integrates with Spot VMs |
| **Azure Stack** (Hub/HCI/Edge) | Azure on-premises | Same Azure services/APIs in your datacenter, disconnected scenarios | Hybrid cloud, data sovereignty, edge computing, disconnected environments | Stack Hub = full Azure; Stack HCI = hyperconverged; Stack Edge = edge AI |
| **Virtual Machine Scale Sets (VMSS)** | Auto-scaling VM groups | Auto-scale rules, rolling upgrades, zone-redundant, up to 1000 VMs | High-availability workloads, auto-scaling web tiers | Foundation for many Azure services (AKS nodes, etc.) |
| **Azure Virtual Desktop (AVD)** | Desktop-as-a-Service | Multi-session Windows 11/10, FSLogix profiles, MSIX app attach | Remote work, BYOD, seasonal workers, regulated industries | Only cloud offering with multi-session Windows 10/11 |

---

## Storage Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Blob Storage** | Object storage | Hot/Cool/Cold/Archive tiers, lifecycle management, immutable blobs, versioning, soft delete | Unstructured data, backups, data lakes, media files | Similar to AWS S3; Archive tier equivalent to Glacier |
| **Azure Files** | Managed file shares (SMB/NFS) | SMB 3.0 and NFS 4.1, Azure File Sync (hybrid), AD authentication, snapshots | Lift-and-shift file shares, shared app config, home directories | Azure File Sync keeps hot data on-prem, tiers cold data to cloud |
| **Managed Disks** | Block storage for VMs | Standard HDD, Standard SSD, Premium SSD, Ultra Disk; snapshots, disk encryption | VM OS/data disks, databases, high-IOPS workloads | Ultra Disks for sub-ms latency (SAP HANA, databases) |
| **Data Lake Storage Gen2** | Analytics-optimized storage | Hierarchical namespace on Blob Storage, Hadoop compatible, fine-grained ACLs | Big data analytics, data lakes, Spark/Hadoop workloads | Built on Blob Storage with hierarchical namespace enabled |
| **Queue Storage** | Simple message queue | 64 KB messages, 500 TB queue size, REST API | Simple decoupling; for advanced features use Service Bus | Simpler and cheaper than Service Bus for basic queuing |
| **Table Storage** | NoSQL key-value store | Schemaless, auto-scales, cheap | Simple NoSQL needs; for advanced features use Cosmos DB | Consider Cosmos DB Table API for premium features |
| **Azure NetApp Files** | Enterprise NAS (NetApp) | Sub-ms latency, SMB/NFS, snapshots, cross-region replication | SAP HANA, Oracle, SQL Server, HPC file shares | When Azure Files performance isn't enough |
| **Data Box** | Physical data transfer | Data Box (100 TB), Data Box Disk (40 TB), Data Box Heavy (1 PB) | Large offline data migration to Azure | Equivalent to AWS Snow Family |
| **StorSimple** | Hybrid storage arrays | Cloud-tiered storage, automated data management | Legacy — being retired; use Azure File Sync | Retiring; migrate to Azure File Sync or other solutions |

---

## Database Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Azure SQL Database** | Managed SQL Server | Serverless tier, Hyperscale (100 TB), elastic pools, geo-replication, built-in AI tuning | Cloud-native SQL Server workloads, SaaS backends | Three models: DTU (bundled), vCore (flexible), Hyperscale (massive scale) |
| **Azure SQL Managed Instance** | Near-100% SQL Server compatible PaaS | SQL Agent, CLR, linked servers, cross-DB queries; PaaS benefits | Lift-and-shift SQL Server with near-full compatibility | Highest SQL Server compatibility in PaaS |
| **Cosmos DB** | Multi-model globally distributed NoSQL | <10 ms latency anywhere, 5 consistency levels, multi-region writes, APIs for SQL/MongoDB/Cassandra/Gremlin/Table | Global apps, low-latency, multi-model, planet-scale | Know the 5 consistency levels: Strong → Bounded Staleness → Session → Consistent Prefix → Eventual |
| **Azure Database for MySQL** | Managed MySQL | Flexible server, high availability, read replicas, up to 16 TB storage | MySQL workloads, WordPress, LAMP stack apps | Flexible Server is the recommended deployment option |
| **Azure Database for PostgreSQL** | Managed PostgreSQL | Flexible Server, Citus for distributed, high availability, intelligent performance | PostgreSQL workloads, distributed databases (Citus) | Citus extension enables horizontal scaling (sharding) |
| **Azure Cache for Redis** | Managed Redis | Basic/Standard/Premium/Enterprise tiers, clustering, geo-replication, Redis modules | Session caching, real-time analytics, message brokering | Enterprise tier supports RediSearch, RedisJSON, RedisTimeSeries |
| **Azure Synapse Analytics** | Unified analytics platform | Serverless/dedicated SQL pools, Spark pools, Data Explorer, pipelines, integrated studio | Data warehousing + big data analytics in one platform | Replaces and extends SQL Data Warehouse; one workspace for all analytics |
| **Azure Database for MariaDB** | Managed MariaDB | Built-in HA, automated backups, scaling | MariaDB workloads | Being retired — migrate to MySQL Flexible Server |

---

## Networking

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Virtual Network (VNet)** | Azure virtual network | Subnets, NSGs, UDRs, service endpoints, private endpoints, VNet peering | Foundation for all Azure networking | NSGs = NACLs + Security Groups combined |
| **Azure Load Balancer** | Layer 4 load balancer | TCP/UDP, internal/public, HA ports, zone-redundant | High-performance L4 load balancing | Basic (free) vs Standard (production) SKUs |
| **Application Gateway** | Layer 7 load balancer + WAF | URL-based routing, SSL termination, cookie affinity, autoscaling, WAF v2 | Web application load balancing with WAF | Equivalent to AWS ALB + WAF combined |
| **Azure Front Door** | Global CDN + load balancer | Anycast, SSL offloading, WAF, URL rewrite, caching, traffic acceleration | Global web applications, multi-region failover, CDN | Combines CDN + global LB + WAF in one service |
| **Azure DNS** | DNS hosting | Alias records, private DNS zones, DNSSEC | DNS management for Azure-hosted domains | Supports private DNS zones for VNet name resolution |
| **Azure CDN** | Content Delivery Network | Microsoft/Akamai/Verizon providers, custom rules, HTTPS | Static content delivery, media streaming | Being consolidated into Azure Front Door |
| **ExpressRoute** | Dedicated private connection | 50 Mbps–100 Gbps, Global Reach, Direct, private peering | Hybrid connectivity, data-intensive workloads, compliance | Equivalent to AWS Direct Connect |
| **VPN Gateway** | VPN connectivity | Site-to-site, point-to-site, VNet-to-VNet, IKEv2/OpenVPN/SSTP | Encrypted connectivity to Azure | S2S for offices; P2S for remote users |
| **Azure Firewall** | Managed cloud firewall | L3-L7, FQDN filtering, threat intelligence, TLS inspection, IDPS | Centralized network security, hub-and-spoke architectures | Premium tier adds TLS inspection and IDPS |
| **Traffic Manager** | DNS-based global load balancer | Priority, weighted, performance, geographic, multivalue routing | Multi-region failover, geographic routing | DNS-based (not proxy); compare with Front Door (proxy-based) |
| **Private Link / Private Endpoint** | Private connectivity to Azure services | Access PaaS over private IP, no public internet exposure | Securing access to Azure SQL, Storage, Cosmos DB, etc. | Creates a private IP in your VNet for the Azure service |
| **Azure Bastion** | Managed bastion host | RDP/SSH over TLS in Azure Portal, no public IP on VMs needed | Secure remote access to VMs without exposing RDP/SSH | Replaces jump boxes and public IPs on VMs |
| **Virtual WAN** | Managed hub-and-spoke networking | Automated branch connectivity, VPN/ExpressRoute/P2S in one hub | Large-scale branch connectivity, global transit | Microsoft-managed hub replaces custom hub-and-spoke |

---

## Security, Identity & Compliance

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Microsoft Entra ID** (formerly Azure AD) | Identity and access management | SSO, MFA, conditional access, B2B/B2C, privileged identity management, identity governance | Every Azure/Microsoft 365 deployment | Not like on-prem AD — it's an identity-as-a-service platform |
| **Key Vault** | Secret and key management | Secrets, keys, certificates; HSM-backed, access policies, soft delete, RBAC | Storing secrets, managing encryption keys, SSL certificates | Equivalent to AWS KMS + Secrets Manager combined |
| **Microsoft Defender for Cloud** | Cloud security posture management | Secure Score, regulatory compliance, threat protection for Azure/hybrid/multi-cloud | Security posture assessment and workload protection | Covers Azure, AWS, and GCP workloads |
| **Azure Policy** | Governance and compliance | Policy definitions, initiatives, remediation tasks, deny/audit/append effects | Enforcing organizational standards across subscriptions | Like AWS SCPs but more granular — applies to resource properties |
| **Azure Sentinel (Microsoft Sentinel)** | Cloud-native SIEM/SOAR | AI-powered analytics, 200+ connectors, automated response playbooks | Security monitoring, threat detection, incident response | Built on Log Analytics; uses KQL for queries |
| **DDoS Protection** | DDoS protection | Basic (free, always on), Standard (advanced metrics, alerting, SLA) | Protecting public endpoints from DDoS attacks | Standard tier provides cost protection during attacks |
| **Azure Information Protection** | Data classification and labeling | Sensitivity labels, encryption, rights management | Protecting sensitive documents and emails | Integrated with Microsoft 365 apps |
| **Managed HSM** | FIPS 140-2 Level 3 HSM | Single-tenant, customer-controlled, sovereign keys | Highest compliance requirements, BYOK | When standard Key Vault doesn't meet compliance needs |

---

## Application Integration & Messaging

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Service Bus** | Enterprise message broker | Queues, topics/subscriptions, sessions, transactions, dead-letter, duplicate detection | Enterprise messaging, ordered/transactional messaging | More feature-rich than Storage Queue; supports pub/sub with topics |
| **Event Grid** | Event routing service | React to Azure/custom events, serverless, filtering, retry, dead-letter | Event-driven architectures, reacting to Azure resource changes | Push model — delivers events to subscribers |
| **Event Hubs** | Big data streaming platform | Millions of events/sec, Kafka compatible, capture to storage, partitions | Real-time analytics, telemetry ingestion, log streaming | Equivalent to AWS Kinesis; Kafka-compatible endpoint |
| **Logic Apps** | Workflow automation (low-code) | 450+ connectors, visual designer, B2B/EDI, Standard (single-tenant) / Consumption (multi-tenant) | Integration workflows, SaaS connectivity, business process automation | Like AWS Step Functions but with 450+ pre-built connectors |
| **API Management** | Full-lifecycle API management | Gateway, developer portal, policies, versioning, rate limiting, analytics | Publishing APIs to internal/external developers | More full-featured than AWS API Gateway (includes developer portal) |
| **SignalR Service** | Managed real-time messaging | WebSockets, Server-Sent Events, long polling; auto-scales | Real-time dashboards, chat, notifications, live updates | Serverless mode for event-driven real-time messaging |
| **Notification Hubs** | Push notification engine | Cross-platform push (iOS, Android, Windows, Kindle), personalization, scheduling | Mobile app push notifications at scale | Supports millions of pushes per minute |

---

## Analytics & Big Data

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Synapse Analytics** | Unified analytics workspace | Serverless SQL, dedicated SQL pools, Apache Spark, Data Explorer pools, pipelines | Data warehousing, big data processing, real-time analytics | Successor to SQL Data Warehouse; one workspace for everything |
| **HDInsight** | Managed Hadoop ecosystem | Spark, Hive, HBase, Kafka, Storm, Interactive Query (LLAP) | Open-source big data processing, Hadoop/Spark workloads | Being superseded by Synapse Spark pools for many use cases |
| **Databricks** (Azure) | Unified analytics platform | Lakehouse architecture, MLflow, Delta Lake, Unity Catalog | Advanced data engineering, data science, ML at scale | Joint venture with Databricks — runs on Azure infrastructure |
| **Data Factory** | Cloud ETL/ELT | 100+ connectors, visual pipelines, mapping data flows (Spark-based), CI/CD integration | Data ingestion, ETL pipelines, data orchestration | Similar to AWS Glue; the "SSIS in the cloud" |
| **Stream Analytics** | Real-time stream processing | SQL-like queries on streams, reference data joins, anomaly detection | Real-time dashboards, IoT analytics, fraud detection | T-SQL-like syntax on streaming data |
| **Power BI** | Business intelligence | DAX, DirectQuery, import mode, paginated reports, embedded analytics, AI visuals | Dashboards, reports, self-service analytics | Deeply integrated with Microsoft ecosystem |
| **Data Explorer (ADX)** | Real-time analytics on large volumes | KQL query language, fast ingestion, time-series analysis, free cluster option | Log analytics, IoT telemetry, time-series data | KQL is also used in Sentinel, Defender, and Monitor |
| **Purview** (Microsoft Purview) | Data governance | Data catalog, lineage, classification, access policies, multi-cloud scanning | Data discovery, governance, compliance across cloud/on-prem | Scans Azure, AWS, GCP, and on-prem data sources |

---

## AI & Machine Learning

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Azure Machine Learning** | Full ML platform | Automated ML, designer (drag-and-drop), notebooks, MLOps, responsible AI dashboard | End-to-end ML development and deployment | Competes with AWS SageMaker; strong MLOps capabilities |
| **Azure OpenAI Service** | Managed OpenAI models | GPT-4, GPT-4o, DALL-E, Whisper; enterprise security, private networking, content filtering | Enterprise GenAI apps, chatbots, code generation | Azure-exclusive access to OpenAI models with enterprise features |
| **Azure AI Services** (formerly Cognitive Services) | Pre-built AI APIs | Vision, Speech, Language, Decision, Document Intelligence | Adding AI to apps without ML expertise | No ML expertise needed; simple REST API calls |
| **Azure AI Search** (formerly Cognitive Search) | AI-powered search | Full-text search, vector search, semantic ranking, integrated AI enrichment | Enterprise search, RAG applications, document search | Key component for building RAG with Azure OpenAI |
| **Bot Service** | Bot framework | Multi-channel (Teams, Slack, web), Bot Framework SDK, Composer (low-code) | Conversational bots for customer service, internal tools | Deep integration with Microsoft Teams |

---

## DevOps & Developer Tools

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Azure DevOps** | Complete DevOps platform | Boards (work tracking), Repos (Git), Pipelines (CI/CD), Test Plans, Artifacts (packages) | Full DevOps lifecycle management | 5 tools in 1; can use individually or together |
| **GitHub Actions** (Azure-integrated) | CI/CD from GitHub | Native Azure deployment actions, OIDC authentication, marketplace actions | GitHub-hosted projects deploying to Azure | Microsoft owns GitHub; deep integration |
| **Azure Monitor** | Full observability platform | Metrics, logs (Log Analytics), alerts, Application Insights (APM), workbooks, autoscale | Monitoring all Azure resources, APM, alerting | Application Insights for APM; Log Analytics for log queries (KQL) |
| **ARM Templates** | IaC in JSON | Declarative, idempotent, what-if preview, template specs | Azure infrastructure provisioning | Being superseded by Bicep |
| **Bicep** | IaC DSL for Azure | Simpler than ARM JSON, type-safe, modules, VS Code extension | Modern Azure IaC (recommended over ARM templates) | Compiles to ARM templates; much more readable |
| **Azure DevTest Labs** | Managed dev/test environments | Quotas, auto-shutdown, artifact installation, formulas, policies | Cost-controlled dev/test environments | Auto-shutdown saves money on dev/test VMs |

---

## Management & Governance

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Azure Resource Manager (ARM)** | Management layer for all Azure resources | Resource groups, RBAC, tags, locks, templates | Every Azure deployment — all operations go through ARM | Everything in Azure is a resource managed by ARM |
| **Management Groups** | Hierarchy above subscriptions | Organize subscriptions, apply policies/RBAC at scale | Enterprise governance across multiple subscriptions | Up to 6 levels deep; root management group at top |
| **Cost Management + Billing** | Cost tracking and optimization | Budgets, cost analysis, advisor recommendations, exports | Controlling and optimizing Azure spend | Built-in (free); also works with AWS costs |
| **Azure Advisor** | Best practice recommendations | Reliability, security, performance, cost, operational excellence | Proactive optimization across all Azure resources | Free; actionable recommendations with one-click fixes |
| **Azure Migrate** | Migration assessment and execution | Server discovery, assessment, replication, database migration, web app migration | Planning and executing migrations to Azure | Central hub for all migration tools |
| **Azure Arc** | Hybrid and multi-cloud management | Manage on-prem/other-cloud servers, K8s, SQL, data services as Azure resources | Extending Azure management to any infrastructure | Project Azure policies, RBAC, and monitoring everywhere |
| **Azure Lighthouse** | Multi-tenant management | Delegated resource management, cross-tenant visibility | MSPs managing multiple customer Azure environments | Designed for managed service providers |
