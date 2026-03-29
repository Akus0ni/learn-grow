# AWS ECS & EKS — Container Orchestration

## 1. Core Concepts

### Why Container Orchestration?
Containers package code + dependencies + config into a portable image. Orchestration solves:
- **Scheduling**: Where to run containers?
- **Scaling**: How many containers to run?
- **Health**: Restart failed containers
- **Service discovery**: How do containers find each other?
- **Load balancing**: Distribute traffic across containers
- **Secrets management**: Inject credentials safely

---

## 2. Amazon ECS (Elastic Container Service)

### What is ECS?
ECS is AWS's **native container orchestration service**. You define **Task Definitions** (blueprint for containers) and **Services** (how many tasks to run). ECS handles scheduling, scaling, and health.

### Launch Types
| | Fargate | EC2 |
|--|---------|-----|
| Infrastructure | AWS manages | You manage EC2 instances |
| Control | Less | More (instance types, AMIs) |
| Cost | Per vCPU + memory per second | Per EC2 instance (+ optional savings) |
| Use case | Most workloads, serverless containers | GPU workloads, specific instance requirements |

### Key ECS Concepts

**Task Definition**: JSON blueprint defining:
- Docker image (from ECR, Docker Hub)
- CPU + memory allocation (hard/soft limits)
- Port mappings
- Environment variables
- Secrets (from SSM/Secrets Manager)
- IAM Task Role (what the container can do)
- IAM Task Execution Role (what ECS can do: pull image, write logs)
- Log configuration (awslogs → CloudWatch)
- Volume mounts (EFS for shared storage)

**Service**: Maintains N running copies of a task definition:
- Integrates with ALB/NLB
- Rolling updates and blue/green (with CodeDeploy)
- Service Auto Scaling (Target Tracking, Step, Scheduled)

**Cluster**: Logical grouping of tasks/services. Can mix Fargate and EC2 launch types.

### ECS Networking Modes
| Mode | Description | Use Case |
|------|-------------|---------|
| **awsvpc** | Each task gets its own ENI and private IP | Recommended — task-level security groups; required for Fargate |
| **bridge** | Docker bridge networking, port mapping | Legacy EC2 tasks |
| **host** | Container uses EC2 host network | Maximum performance, no port isolation |
| **none** | No networking | Batch tasks that don't need network |

### ECS + Fargate Benefits
- No cluster management — AWS provisions and scales compute
- Pay only for running containers (vCPU-second + GB-second)
- Each task gets its own isolated compute environment
- Works with Spot (Fargate Spot) — up to 70% discount, with 2-minute interruption notice

### Amazon ECR (Elastic Container Registry)
- Managed Docker-compatible container registry
- Integrates natively with ECS, EKS, CodeBuild, Lambda
- **Lifecycle policies**: Automatically clean old/untagged images
- **Image scanning**: Basic scanning (free) or Enhanced scanning (Inspector — paid)
- **Cross-account access**: Via ECR repository policies
- **OCI artifact support**: Helm charts, Wasm modules

---

## 3. Amazon EKS (Elastic Kubernetes Service)

### What is EKS?
EKS is AWS's **managed Kubernetes** service. AWS manages the control plane (API server, etcd, controller manager, scheduler). You manage the worker nodes (or use Fargate for serverless nodes).

### Kubernetes Fundamentals (for EKS Interviews)
| Concept | Description |
|---------|-------------|
| **Pod** | Smallest deployable unit — 1+ containers with shared network and storage |
| **Deployment** | Manages ReplicaSets; rolling updates, rollbacks |
| **ReplicaSet** | Ensures N pod replicas are running |
| **Service** | Stable networking endpoint for a set of pods (ClusterIP, NodePort, LoadBalancer) |
| **Ingress** | HTTP routing rules (host/path) → backend Services; managed by an Ingress Controller |
| **ConfigMap** | Non-sensitive config data injected into pods |
| **Secret** | Sensitive data (base64-encoded, not encrypted by default — use KMS envelope encryption) |
| **Namespace** | Virtual cluster for resource isolation |
| **DaemonSet** | Run one pod per node (logging, monitoring agents) |
| **StatefulSet** | For stateful apps — stable network identity, ordered deployment |
| **Job / CronJob** | One-time or scheduled batch workloads |
| **HPA** | Horizontal Pod Autoscaler — scale pod count based on metrics |
| **VPA** | Vertical Pod Autoscaler — adjust CPU/memory requests/limits |
| **Karpenter** | AWS-native node autoscaler — provisions right-sized nodes just in time |

### EKS Node Options
| Option | Description |
|--------|-------------|
| **Managed Node Groups** | AWS manages EC2 lifecycle, auto-updates, AMI replacement |
| **Self-managed Nodes** | You control everything (custom AMIs, manual updates) |
| **Fargate Profiles** | Serverless nodes — AWS provisions micro-VMs per pod; no node management |

### EKS Add-ons
- **CoreDNS**: Cluster DNS
- **kube-proxy**: Network rules
- **VPC CNI**: Assigns VPC IPs to pods (native VPC networking)
- **EBS CSI Driver**: EBS volume provisioning for PersistentVolumes
- **EFS CSI Driver**: EFS for shared ReadWriteMany volumes
- **AWS Load Balancer Controller**: Provisions ALB (for Ingress) and NLB (for Services) automatically

### EKS Security
- **RBAC**: Role and ClusterRole → bind to Kubernetes users or groups
- **IAM + aws-auth ConfigMap**: Map IAM users/roles to Kubernetes RBAC groups
- **IRSA (IAM Roles for Service Accounts)**: Attach IAM roles to Kubernetes Service Accounts via OIDC — granular per-pod IAM without node-level credentials
- **EKS Pod Identity**: Newer alternative to IRSA — simpler configuration
- **Secrets encryption**: Enable envelope encryption of etcd secrets with KMS

---

## 4. ECS vs EKS Comparison

| | ECS | EKS |
|--|-----|-----|
| Control plane | AWS-managed (fully hidden) | AWS-managed Kubernetes API ($0.10/hr) |
| Learning curve | Low | High (Kubernetes expertise needed) |
| Config complexity | Low | High (YAML manifests, RBAC, etc.) |
| Portability | AWS-only | Any Kubernetes environment |
| Ecosystem | AWS-native | Kubernetes CNCF ecosystem |
| Multi-cloud strategy | No | Yes |
| Fargate support | Yes | Yes (via Fargate Profiles) |
| GPU workloads | EC2 launch type | Yes (specialized node groups) |
| Best for | AWS-native teams, new projects | Existing K8s expertise, multi-cloud |

---

## 5. Interview Questions & Answers

### Q1. What is the difference between ECS and EKS?
**ECS** is AWS's proprietary container orchestration service. Simple to use, deeply integrated with AWS services, no Kubernetes knowledge needed. Great for teams going all-in on AWS.

**EKS** is managed Kubernetes on AWS. Ideal when you need Kubernetes portability (run on-premises with EKS Anywhere, migrate to GKE/AKS), have existing K8s expertise, or need the rich CNCF ecosystem (Helm, Istio, Argo CD, etc.).

For new projects without K8s experience: ECS/Fargate is faster to set up. For enterprise standardization on K8s: EKS.

---

### Q2. What is the difference between ECS Fargate and EC2 launch type?
**Fargate**: AWS provisions compute per task. No EC2 instances to manage. Pay per vCPU-second and GB-second while the task runs. Ideal for variable workloads, microservices, and teams that don't want to manage infrastructure.

**EC2 launch type**: You manage an EC2 cluster. More control over instance type, placement, local storage. Better for GPU workloads, high network throughput requirements, or when Reserved Instance savings matter. Bin-packing multiple tasks on one instance.

---

### Q3. What is a Task Definition and what does it contain?
A Task Definition is a blueprint for running containers in ECS. It specifies:
- **Container definitions**: Image URI, CPU/memory, port mappings, environment variables, secrets (SSM/Secrets Manager), log driver
- **Task IAM role**: Permissions the container code needs (e.g., S3, DynamoDB)
- **Task execution role**: Permissions ECS needs to pull image from ECR, write logs to CloudWatch
- **Network mode**: awsvpc (recommended), bridge, host
- **Volumes**: EFS mounts, bind mounts for Fargate

---

### Q4. How does ECS service discovery work?
Options:
1. **ALB + Service**: ALB distributes traffic to ECS tasks via Target Groups. Tasks register/deregister automatically on scale events.
2. **AWS Cloud Map**: ECS services register with Cloud Map; provides DNS (`service.namespace.local`) and HTTP discovery. Tasks query DNS to find other services.
3. **Service Connect**: ECS-managed service mesh using Cloud Map — adds traffic stats, retries, timeouts without a separate service mesh.

---

### Q5. What is IRSA and why is it better than node-level IAM roles?
**IRSA (IAM Roles for Service Accounts)** associates an IAM role with a Kubernetes Service Account via OIDC federation.

Without IRSA: Every pod on a node shares the node's IAM role. If one pod is compromised, it has all the node's permissions.

With IRSA: Each pod's Service Account maps to a specific IAM role with least-privilege permissions. A compromised pod can only access what its specific role allows.

```yaml
# Service Account with IRSA annotation
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-app
  annotations:
    eks.amazonaws.com/role-arn: arn:aws:iam::123456:role/MyAppRole
```

---

### Q6. What is Kubernetes HPA and how does it work with EKS?
**HPA (Horizontal Pod Autoscaler)** scales the number of pod replicas based on metrics:
- **CPU/Memory**: Built-in (from Metrics Server)
- **Custom metrics**: Application metrics via Prometheus Adapter
- **External metrics**: SQS queue depth, custom CloudWatch metrics via KEDA

In EKS, HPA works the same as upstream Kubernetes. When HPA scales pods beyond current node capacity, **Karpenter** (or Cluster Autoscaler) provisions new nodes automatically.

---

### Q7. How do you manage secrets in ECS and EKS?
**ECS**:
- In Task Definition, reference secrets from **AWS Secrets Manager** or **SSM Parameter Store** (SecureString)
- ECS injects them as environment variables or mounts them as files
- Task Execution Role must have `secretsmanager:GetSecretValue` or `ssm:GetParameters` permission

**EKS**:
- Native K8s Secrets (base64, not encrypted by default)
- Enable **KMS envelope encryption** for etcd secrets
- Use **AWS Secrets and Config Provider (ASCP)** — Secrets Manager/SSM values mounted as files in pods
- Use **External Secrets Operator** — syncs Secrets Manager/SSM to K8s Secrets automatically

---

### Q8. What is Karpenter and how is it different from Cluster Autoscaler?
**Cluster Autoscaler**: Works with existing Auto Scaling Groups. Scales node groups up/down based on pending pods. Limited to predefined instance types in the ASG.

**Karpenter**: AWS-native node provisioner. Watches for unschedulable pods and provisions the **optimal** node (instance type, size, spot vs on-demand) directly — without pre-defined ASGs. Faster (~seconds vs minutes), cheaper (right-sized instances), simpler configuration.

---

### Q9. How do you do a blue/green deployment with ECS?
Using **AWS CodeDeploy + ECS**:
1. ECS Service has a **deployment controller** set to `CODE_DEPLOY`
2. CodeDeploy creates a new task set (green) with the new task definition version
3. Traffic is shifted from the original target group (blue) to the replacement (green) using the ALB listener
4. Shift strategies: `AllAtOnce`, `Canary` (10% then all), `Linear` (10% per minute)
5. If CloudWatch alarms trigger, CodeDeploy automatically rolls back

---

### Q10. What is the difference between a Kubernetes Deployment and a StatefulSet?
| | Deployment | StatefulSet |
|--|-----------|------------|
| Pod identity | Random names (pod-abc12) | Stable ordinal names (pod-0, pod-1) |
| Storage | Shared or ephemeral | Each pod gets its own PVC |
| Scaling | Any order | Ordered (0→1→2 scale up; 2→1→0 scale down) |
| Use case | Stateless apps (web servers, APIs) | Databases, Kafka, Elasticsearch, Zookeeper |
| DNS | Service DNS only | Stable per-pod DNS headless service |

---

### Q11. How does Fargate handle security isolation compared to EC2-based containers?
**Fargate**: Each task runs in its own micro-VM (Firecracker). There is no shared kernel between tasks — strong security isolation. No need to patch or secure host OS.

**EC2**: Multiple containers share the EC2 host kernel. Container escape = access to host and other containers. Mitigate with seccomp profiles, AppArmor, read-only root filesystem.

For compliance-sensitive workloads (PCI-DSS, HIPAA), Fargate's isolation is preferred.

---

### Q12. What is Amazon EKS Anywhere?
EKS Anywhere lets you run EKS (Kubernetes) on your **own on-premises infrastructure** (VMware vSphere, bare metal, AWS Outposts). You use the same EKS API, CLI, and tooling. Great for hybrid environments where data must stay on-premises.

---

### Q13. How do you roll back a failed ECS deployment?
1. In the AWS Console, navigate to the ECS Service → Update Service
2. Select the previous task definition revision
3. ECS rolling update replaces running tasks with the previous version

Or with CodeDeploy blue/green: CodeDeploy rolls back to the original task set if a CloudWatch alarm triggers or you manually initiate rollback.

Automate with: **AWS Deployment Pipeline** — test in staging, gate promotion to prod using CloudWatch canary alarms.

---

### Q14. What is a Kubernetes Ingress vs a Service of type LoadBalancer?
**Service type LoadBalancer**: Provisions an AWS NLB (or CLB) per Service. One LB per microservice = expensive, unmanageable at scale.

**Ingress**: A single AWS ALB (via the AWS Load Balancer Controller) handles HTTP/HTTPS routing for multiple services using host-based and path-based rules. One ALB can route to dozens of services — much cheaper and simpler.

```yaml
# Ingress routes
/api/*   → api-service:80
/web/*   → web-service:80
reviews.example.com → reviews-service:80
```

---

### Q15. How do you monitor ECS and EKS workloads?
**ECS**:
- **CloudWatch Container Insights**: CPU, memory, network, disk per cluster/service/task
- **CloudWatch Logs**: Container stdout/stderr via `awslogs` log driver
- **X-Ray**: Distributed tracing (instrument with X-Ray SDK)
- **ECS Exec**: Interactive shell into running containers for debugging

**EKS**:
- **CloudWatch Container Insights** (with Container Insights operator)
- **Prometheus + Grafana** (via Amazon Managed Grafana + Amazon Managed Prometheus)
- **AWS Distro for OpenTelemetry (ADOT)**: Metrics + traces collection
- **Fluent Bit (DaemonSet)**: Log forwarding to CloudWatch, OpenSearch, or S3

---

## 6. Real-World Use Case: Microservices Platform on ECS Fargate

### Scenario
A B2B SaaS company needs to:
- Run 12 microservices, each maintained by a different team
- Scale services independently based on SQS queue depth or HTTP traffic
- Zero-downtime deployments with automatic rollback
- No server management — pure serverless containers
- Sub-second inter-service latency

### Architecture

```
           Internet
               │
           [WAF + ALB]  (public subnets, Multi-AZ)
               │
    ┌──────────┼──────────────┐
    │   Path/Host Routing     │
    │                         │
    ▼                         ▼
[/api/*  Target Group]  [/auth/* Target Group]
    │                         │
[API Service]           [Auth Service]
(Fargate, 2-10 tasks)   (Fargate, 2-5 tasks)
    │
    ├── SQS → [Order Processor] (Fargate, scales 0-50 on queue depth)
    ├── SQS → [Email Service] (Fargate, scales 0-20)
    └── SQS → [Report Generator] (Fargate Spot, scales 0-10)
    
[ECS Service Connect]  ← Internal service mesh
  api.local, auth.local, order.local
  (auto-retry, circuit breaker, metrics)

[Amazon ECR] ← All 12 service images
  Lifecycle policy: keep last 10 tagged + clean untagged after 1 day
  Enhanced scanning: Inspector on every push

[AWS Secrets Manager] → Injected into task definitions as env vars
[SSM Parameter Store]  → Non-sensitive config

[CloudWatch Container Insights]
  Dashboard per service: CPU, memory, task count, ALB 5xx rate
  Alarm: 5xx > 1% → SNS → PagerDuty

[CodePipeline + CodeDeploy]
  GitHub push → ECR build → ECS Blue/Green
  Canary: 10% traffic to green, CloudWatch alarm gates 100%

Fargate Spot for batch:
  Report Generator runs on Fargate Spot (70% cheaper)
  Interruption handled: save checkpoint to DynamoDB, task retries from checkpoint
```

### Deployment Strategy per Service Type

| Service | Strategy | Reason |
|---------|----------|--------|
| API (customer-facing) | Blue/Green (Canary 10%) | Zero downtime, easy rollback |
| Auth | Blue/Green (All at once) | Small service, instant rollback |
| Order Processor | Rolling update | SQS-backed — can process during update |
| Report Generator | All at once | Fargate Spot, idempotent jobs |

### Interview Narration (White-board Script)
> "For this microservices platform I'd use ECS Fargate — each team owns their service's task definition, and ECS handles scheduling without them managing EC2 instances. The ALB does path and host-based routing to different target groups per service. For inter-service communication I'd use ECS Service Connect which gives us service mesh capabilities — retries, circuit breaking, and metrics — without deploying Istio. Deployments go through CodePipeline to CodeDeploy with a canary strategy: 10% of traffic to the new version, CloudWatch checks the error rate alarm, and if it stays green for 5 minutes, we shift 100%. If the alarm fires, CodeDeploy automatically rolls back. Batch workloads like report generation run on Fargate Spot — when interrupted, the task saves its checkpoint to DynamoDB and retries from where it left off."
