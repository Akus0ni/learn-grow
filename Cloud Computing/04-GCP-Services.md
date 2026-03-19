# Google Cloud Platform (GCP) — Services Quick Reference

> Comprehensive table of GCP services for interview preparation and quick revision.

---

## Compute Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Compute Engine** | IaaS Virtual Machines | Custom machine types, preemptible/spot VMs, sole-tenant nodes, live migration, sustained use discounts | Full OS control, lift-and-shift, high-performance computing | Custom machine types are unique to GCP — pick exact vCPU/RAM |
| **Cloud Functions** | Serverless functions (FaaS) | Event-driven, 1st gen (HTTP + events) and 2nd gen (Cloud Run-based), supports Node/Python/Go/Java/.NET/Ruby/PHP | Lightweight event processing, webhooks, glue code | 2nd gen is built on Cloud Run; longer timeout (60 min), larger instances |
| **Cloud Run** | Serverless containers | Any container, auto-scales to zero, per-request billing, Cloud Run Jobs for batch, custom domains | Containerized web apps, APIs, microservices, batch jobs | Unique: serverless + any container image — no lock-in to specific runtime |
| **GKE** (Google Kubernetes Engine) | Managed Kubernetes | Autopilot (fully managed) and Standard mode, multi-cluster, GKE Enterprise, release channels | Container orchestration at scale, microservices | Google invented K8s — GKE is the most mature managed K8s service |
| **App Engine** | PaaS for web apps | Standard (sandboxed, scale to zero) and Flexible (custom Docker) environments, auto-scaling, versioning | Simple web apps, rapid prototyping, apps with variable traffic | Standard = faster scaling, restricted runtimes; Flexible = any Docker container |
| **Bare Metal Solution** | Dedicated bare metal servers | Oracle, SAP HANA certified; low-latency to GCP services | Running specialized workloads (Oracle DB, SAP) near GCP | Dedicated hardware, not virtualized |
| **VMware Engine** | Managed VMware on GCP | Full VMware stack (vSphere, vSAN, NSX-T), HCX for migration | Migrating VMware workloads to cloud without refactoring | Fully managed VMware SDDC on GCP infrastructure |
| **Batch** | Managed batch processing | Job queues, auto-provisioning VMs, spot VM support, GPU support | HPC, ML training data processing, rendering, genomics | Newer service — simpler than running Compute Engine manually for batch |
| **Sole-Tenant Nodes** | Dedicated physical servers | Hardware isolation, bring your own license (BYOL), compliance | Licensing compliance (per-core), hardware isolation requirements | Dedicated hardware within GCP; unlike Bare Metal — still virtualized |

---

## Storage Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Cloud Storage** | Object storage | Standard, Nearline (30-day min), Coldline (90-day min), Archive (365-day min); unified API, lifecycle rules, Object Versioning, retention policies | Any unstructured data, backups, data lakes, media, static hosting | All classes have same API and ms-latency access; classes differ in cost/min retention |
| **Persistent Disks** | Block storage for VMs | Standard (HDD), Balanced (SSD), SSD, Extreme (high-IOPS); regional disks (multi-zone), snapshots | VM boot/data disks, databases | Regional persistent disks replicate across 2 zones automatically |
| **Filestore** | Managed NFS file shares | Basic, High Scale, Enterprise tiers; NFSv3, snapshots, backups | Shared file storage, GKE persistent volumes, content management | Enterprise tier offers multi-zone availability |
| **Cloud Storage for Firebase** | Mobile/web file storage | Client SDKs, security rules, integrates with Firebase Auth | Mobile app file uploads/downloads | Built on Cloud Storage with Firebase-specific client SDKs |
| **Local SSD** | Ultra-high-performance local storage | Up to 9 TB, sub-ms latency, 2.4M read IOPS | Caches, scratch space, high-performance temp storage | Ephemeral — data lost when VM stops; not for persistent data |
| **Transfer Service** | Data transfer orchestration | Storage Transfer Service (online), Transfer Appliance (offline, 300 TB) | Migrating data from other clouds/on-prem to Cloud Storage | Can transfer directly from AWS S3 or Azure Blob |
| **NetApp Volumes** | Enterprise NAS on GCP | SMB/NFS, cross-region replication, snapshots, clones | SAP, Oracle, enterprise NAS, high-performance shared storage | Managed NetApp ONTAP; sub-ms latency |

---

## Database Services

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Cloud SQL** | Managed relational DB | MySQL, PostgreSQL, SQL Server; HA, read replicas, automated backups, up to 128 TB | Traditional relational workloads, OLTP, small-medium databases | Equivalent to AWS RDS; max 128 TB storage |
| **AlloyDB** | PostgreSQL-compatible high-performance DB | 4x faster than standard PostgreSQL, 100x faster analytical queries, columnar engine, ML integration | High-performance PostgreSQL, HTAP (transactional + analytical) | GCP's answer to Aurora; superior for mixed workloads |
| **Cloud Spanner** | Globally distributed relational DB | Unlimited horizontal scale, 99.999% SLA, strong consistency globally, SQL interface | Global transactions, financial systems, gaming, retail at planet-scale | Only DB with strong consistency + horizontal scaling globally; 5 nines SLA |
| **Firestore** | Serverless NoSQL document DB | Real-time sync, offline support, ACID transactions, auto-scales, Native and Datastore modes | Mobile/web apps, real-time collaboration, content management | Native mode for mobile/web; Datastore mode for server-side workloads |
| **Bigtable** | Wide-column NoSQL (HBase compatible) | Single-digit ms latency, petabyte scale, HBase API compatible, time-series optimized | IoT, financial time-series, adtech, analytics, Hadoop workloads | Powers Google Search, Maps, Gmail; ideal for >1 TB datasets |
| **Memorystore** | Managed in-memory store | Redis and Memcached, sub-ms latency, HA, auto-failover | Caching, session management, gaming leaderboards | Redis Cluster mode for larger deployments |
| **BigQuery** | Serverless data warehouse | Columnar, petabyte-scale, SQL, ML (BQML), BI Engine, streaming ingestion, federated queries, free tier (1 TB/month) | Analytics, data warehousing, ad-hoc querying, ML on SQL | Best-in-class data warehouse; serverless with per-query pricing |
| **Firebase Realtime Database** | Real-time JSON database | Millisecond sync, offline support, simple REST/SDK access | Simple real-time apps, prototyping, small-scale mobile apps | Consider Firestore for new projects (more features) |
| **Database Migration Service** | DB migration tool | Continuous replication, minimal downtime, supports MySQL/PostgreSQL/SQL Server/Oracle/AlloyDB | Migrating databases to Cloud SQL, AlloyDB, or Spanner | Equivalent to AWS DMS |

---

## Networking

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **VPC** (Virtual Private Cloud) | Software-defined network | Global by default, subnets are regional, firewall rules, Shared VPC, VPC peering, Private Google Access | Foundation for all GCP networking | GCP VPC is global (unlike AWS/Azure where VPC = regional) |
| **Cloud Load Balancing** | Global and regional LB | HTTP(S) (global L7), TCP/SSL (global L4), internal TCP/UDP, internal HTTP(S), URL maps | Distributing traffic across regions and zones | Global LB with single anycast IP across all regions — unique to GCP |
| **Cloud CDN** | Content Delivery Network | Cache with Cloud Load Balancing, signed URLs/cookies, cache invalidation | Static content delivery, API acceleration | Tightly integrated with Cloud Load Balancing |
| **Cloud DNS** | Managed DNS | 100% SLA, DNSSEC, private zones, forwarding, peering | DNS hosting for GCP and external domains | Only DNS service with 100% SLA |
| **Cloud Armor** | DDoS and WAF | L3/L4 DDoS protection (always on), L7 WAF rules, adaptive protection (ML-based), bot management | Protecting applications behind Load Balancer | Equivalent to AWS WAF + Shield; ML-based adaptive protection is unique |
| **Cloud Interconnect** | Dedicated/partner connection | Dedicated (10/100 Gbps), Partner (50 Mbps–50 Gbps), Cross-Cloud Interconnect | Hybrid connectivity, high-bandwidth, low-latency | Cross-Cloud Interconnect connects GCP to other clouds directly |
| **Cloud VPN** | Managed IPsec VPN | HA VPN (99.99% SLA), Classic VPN, supports dynamic routing (BGP) | Encrypted connectivity to GCP | HA VPN with 2 tunnels provides 99.99% SLA |
| **Cloud NAT** | Managed NAT gateway | Regional, auto-scaling, no VM needed, port allocation | Outbound internet for private VMs without external IPs | Software-defined — no single point of failure |
| **Private Service Connect** | Private connectivity to services | Consume Google APIs and partner services over private IP | Securing access to Google APIs and third-party services | Similar to AWS PrivateLink |
| **Network Connectivity Center** | Hub-and-spoke networking | Central hub for hybrid connectivity, VPN/Interconnect spokes | Multi-site hybrid networking, SD-WAN integration | Simplifies complex hybrid network topologies |
| **Service Mesh (Traffic Director)** | Managed service mesh control plane | xDS API, traffic management, security policies, observability | Service-to-service communication in microservices | Control plane for Envoy proxies |

---

## Security & Identity

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Cloud IAM** | Identity and access management | Roles (basic, predefined, custom), service accounts, policy binding, Workload Identity Federation | Access control for all GCP resources | Deny policies now available (in addition to allow policies) |
| **Identity Platform** | Customer identity (CIAM) | Multi-tenancy, social login, SAML/OIDC, MFA, blocking functions | Adding auth to customer-facing apps | Firebase Auth's enterprise version |
| **Cloud KMS** | Key management | Software and HSM-backed keys, CMEK, CSEK, EKM (external), auto-rotation | Encryption key management, compliance | EKM lets you use keys stored outside Google |
| **Secret Manager** | Secret storage | Versioning, automatic rotation, IAM-based access, audit logging | Storing API keys, passwords, certificates | Simpler than KMS for secret storage; IAM-integrated |
| **Security Command Center** | Security posture management | Asset inventory, vulnerability scanning, threat detection (Event Threat Detection), compliance monitoring, attack path simulation | Centralized security visibility and threat detection | Premium tier includes threat detection and compliance; attack path simulation is unique |
| **BeyondCorp Enterprise** | Zero-trust access | Context-aware access, threat/data protection, agentless, clientless | Zero-trust security for enterprise apps and resources | Google's own zero-trust implementation, productized |
| **Certificate Authority Service** | Managed PKI | Private CAs, certificate lifecycle, templates, IAM integration | Internal PKI, mTLS, IoT device certificates | Scalable private CA without managing HSMs |
| **reCAPTCHA Enterprise** | Bot and fraud protection | Score-based (invisible), checkpoint, account protection, password leak detection | Protecting web apps from bots and abuse | Enterprise version with more features and SLA |
| **VPC Service Controls** | Data exfiltration prevention | Service perimeters, access levels, ingress/egress rules | Preventing data leaks from GCP services | Unique to GCP — creates a security perimeter around GCP services |
| **Chronicle** | Cloud-native SIEM | Petabyte-scale, YARA-L rules, built on Google infrastructure, fixed-price | Enterprise security analytics, threat hunting | Fixed pricing regardless of data volume — unique in SIEM market |

---

## Application Integration & Messaging

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Pub/Sub** | Global messaging and streaming | At-least-once delivery, exactly-once processing, push/pull subscriptions, schema validation, dead-letter topics, ordering | Event-driven architectures, streaming data, async microservices | Combines SQS + SNS + Kinesis concepts into one service |
| **Cloud Tasks** | Managed task queues | HTTP/App Engine targets, rate limiting, scheduling, retry policies | Distributing work across services, rate-limited API calls | Like SQS but with explicit rate control and scheduling |
| **Cloud Scheduler** | Managed cron service | Cron syntax, HTTP/Pub/Sub/App Engine targets, retry configuration | Scheduled jobs, periodic data processing | Fully managed cron-as-a-service |
| **Workflows** | Serverless workflow orchestration | YAML/JSON syntax, connectors for GCP/HTTP services, error handling, parallel steps | Orchestrating microservices, API automation, ETL pipelines | Equivalent to AWS Step Functions; YAML-based |
| **Eventarc** | Event routing service | CloudEvents format, 130+ GCP event sources, custom events, Cloud Run/Workflows/Cloud Functions destinations | Event-driven architectures reacting to GCP resource changes | Like AWS EventBridge for GCP |
| **Apigee** | Full-lifecycle API management | Developer portal, monetization, analytics, traffic management, security policies, hybrid deployment | Enterprise API management, API monetization, partner APIs | Most full-featured API management — includes monetization |
| **Application Integration** | iPaaS (low-code integration) | 100+ connectors, visual designer, triggers, error handling | SaaS integration, business process automation | Like Logic Apps for GCP; enterprise iPaaS |

---

## Analytics & Big Data

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **BigQuery** | Serverless data warehouse | Standard SQL, ML (BQML), BI Engine (in-memory), Omni (multi-cloud), streaming ingestion, materialized views, federated queries | Analytics, data warehousing, real-time dashboards, ML on SQL data | GCP's crown jewel; 1 TB/month free queries; separate storage and compute pricing |
| **Dataflow** | Managed Apache Beam | Batch and streaming, auto-scaling, exactly-once processing, templates | Stream/batch ETL, real-time analytics, data enrichment | Based on Apache Beam — portable across runners |
| **Dataproc** | Managed Hadoop/Spark | 90-second cluster creation, auto-scaling, preemptible VMs, Serverless Spark, integrates with BigQuery/GCS | Hadoop/Spark workloads, migration from on-prem Hadoop | Use Dataproc Serverless for intermittent Spark workloads |
| **Dataform** | SQL-based data transformation | Git-based, dependency management, testing, documentation, BigQuery integration | ELT transformations in BigQuery | dbt alternative built into GCP; SQL-based transformations |
| **Data Fusion** | Visual ETL/ELT (CDAP-based) | Visual interface, 200+ connectors, code-free transformations, lineage | Visual data integration, citizen developer ETL | Based on open-source CDAP; good for non-coders |
| **Composer** | Managed Apache Airflow | DAG orchestration, 1700+ operators, auto-scaling, GCP integrations | Complex workflow orchestration, ML pipelines, multi-step ETL | Managed Airflow; integrates deeply with all GCP data services |
| **Looker** | Enterprise BI platform | LookML modeling, embedded analytics, semantic layer, governed metrics | Self-service analytics, embedded dashboards, governed BI | Semantic modeling layer (LookML) is unique differentiator |
| **Looker Studio** (formerly Data Studio) | Free BI/reporting | Drag-and-drop, 800+ connectors, real-time collaboration, free | Quick dashboards, reports, sharing insights | Free; great for BigQuery visualization |
| **Datastream** | CDC and replication | Real-time CDC from Oracle/MySQL/PostgreSQL to BigQuery/Cloud Storage/AlloyDB | Real-time data replication, database synchronization | Serverless CDC — no infrastructure to manage |
| **Dataplex** | Data governance/management | Data discovery, quality, lineage, zones, security policies | Organizing and governing distributed data across lakes and warehouses | Unifies governance across BigQuery and Cloud Storage |

---

## AI & Machine Learning

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Vertex AI** | Unified ML platform | AutoML, custom training, model registry, feature store, pipelines, prediction, model garden, Gemini | End-to-end ML development and deployment | GCP's unified platform — replaces separate AI Platform products |
| **Vertex AI Search & Conversation** | Enterprise search and chatbots | Grounded in your data, RAG built-in, multi-turn conversation | Enterprise search, customer support bots, RAG applications | Turnkey RAG solution with minimal code |
| **Gemini** (on Vertex AI) | Google's multimodal LLM | Text, image, video, audio understanding; long context, function calling, grounding | GenAI applications, multimodal analysis, code generation | Available through Vertex AI with enterprise features |
| **BigQuery ML (BQML)** | ML within BigQuery using SQL | Linear/logistic regression, boosted trees, DNN, time-series, LLM, import TensorFlow/XGBoost | ML for SQL-proficient analysts, quick prototyping | Train and predict with SQL — no data export needed |
| **Vision AI** | Image analysis | Object detection, OCR, product search, custom models (AutoML Vision) | Image classification, OCR, visual inspection | Pre-trained + custom training options |
| **Natural Language AI** | NLP service | Sentiment, entity analysis, classification, syntax analysis, Healthcare NLP | Text analysis, document classification, entity extraction | Healthcare NLP for clinical text is a differentiator |
| **Speech-to-Text / Text-to-Speech** | Audio transcription and synthesis | 125+ languages, real-time streaming, speaker diarization, WaveNet/Neural2 voices | Transcription, voice bots, accessibility, media subtitling | WaveNet voices are among the most natural-sounding |
| **Translation AI** | Machine translation | 100+ languages, AutoML Translation (custom), Adaptive Translation, glossaries | Multilingual apps, content localization | Powered by Google Translate infrastructure |
| **Document AI** | Document processing | OCR, form parsing, custom processors, human-in-the-loop, pre-built processors (invoice, receipt, ID) | Invoice processing, form extraction, document classification | Pre-built processors for common document types accelerate development |
| **Recommendations AI** | Recommendation engine | Product recommendations, real-time personalization, A/B testing | E-commerce, media content recommendations | Powers Google's own recommendation systems |
| **TPU** (Tensor Processing Unit) | Custom ML accelerator hardware | Cloud TPU v5, TPU Pods, optimized for TensorFlow/JAX | Large-scale ML training, LLM fine-tuning | Google-designed hardware — not available on other clouds |
| **Dialogflow** | Conversational AI platform | CX (enterprise) and ES (standard), NLU, multi-channel, visual flow builder | Chatbots, IVR systems, virtual agents | CX for complex; ES for simple bots |

---

## DevOps & Developer Tools

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Cloud Build** | CI/CD platform | Build triggers, container-native, multi-step builds, GitHub/GitLab/Bitbucket integration, supply chain security | Building, testing, deploying applications | Builds run in containers; integrates with Artifact Registry |
| **Artifact Registry** | Package and container registry | Docker, Maven, npm, Python, Go, Apt, Yum; vulnerability scanning, IAM | Storing and managing build artifacts and containers | Successor to Container Registry; supports multiple formats |
| **Cloud Deploy** | Managed CD for GKE/Cloud Run | Delivery pipelines, approval gates, canary/blue-green, rollback | Progressive deployment to GKE and Cloud Run | Purpose-built CD — not general-purpose CI/CD |
| **Cloud Source Repositories** | Private Git hosting | Integrates with Cloud Build, mirrors from GitHub/Bitbucket | Source control within GCP (limited adoption) | Consider GitHub/GitLab instead for most cases |
| **Cloud Monitoring** | Infrastructure and app monitoring | Metrics, dashboards, uptime checks, alerting, SLO monitoring | Monitoring all GCP resources, multi-cloud | Part of Google Cloud Ops Suite (formerly Stackdriver) |
| **Cloud Logging** | Centralized log management | Real-time ingestion, log-based metrics, log analytics (BigQuery), sinks, audit logs | Centralized logging, compliance, troubleshooting | Can route logs to BigQuery for advanced analytics |
| **Cloud Trace** | Distributed tracing | Auto-instrumentation for GCP services, latency analysis, OpenTelemetry support | Debugging latency issues in distributed systems | Equivalent to AWS X-Ray |
| **Error Reporting** | Automated error tracking | Groups errors, notifications, stack trace analysis | Real-time error monitoring | Auto-detects and groups errors from logs |
| **Cloud Profiler** | Continuous code profiling | CPU and heap profiling in production, low overhead (<5%) | Optimizing production application performance | Continuous profiling with minimal overhead — unique |

---

## Management & Governance

| Service | What It Is | Key Features | When to Use | Interview Tip |
|---|---|---|---|---|
| **Resource Manager** | Resource hierarchy management | Organization → Folders → Projects → Resources; IAM inheritance, org policies | Organizing GCP resources, governance | Everything in GCP lives inside a Project |
| **Organization Policy Service** | Governance constraints | Boolean, list, and custom constraints; prevent resource creation in wrong regions, restrict APIs | Enforcing organizational standards | Like Azure Policy; controls what resources can be created and where |
| **Billing** | Cost management | Budgets, alerts, exports to BigQuery, committed use discounts, sustained use discounts | Cost control and optimization | Sustained use discounts are automatic — no commitment needed |
| **Cloud Asset Inventory** | Resource metadata and history | Real-time asset inventory, change history, IAM/org policy analysis | Understanding what resources exist and their configuration | Search across all GCP resources in one query |
| **Recommender** | AI-powered recommendations | VM rightsizing, idle resource detection, IAM role recommendations, cost optimization | Optimizing cost, security, and performance | Proactive AI recommendations based on actual usage |
| **Deployment Manager** | IaC (native) | YAML/Jinja2/Python templates, declarative | Legacy IaC — use Terraform or Pulumi for new projects | Deprecated in favor of Terraform for most use cases |
| **Migrate to GCP** | Migration tools | Migrate for Compute Engine (VM migration), Migrate for GKE (container migration), Database Migration Service | Lift-and-shift VMs and databases to GCP | Agentless VM replication with minimal downtime |
| **Anthos** | Hybrid/multi-cloud platform | GKE everywhere (on-prem, AWS, Azure), config management, service mesh, serverless | Running K8s consistently across clouds and on-prem | Azure Arc equivalent; runs GKE on AWS/Azure/on-prem |
