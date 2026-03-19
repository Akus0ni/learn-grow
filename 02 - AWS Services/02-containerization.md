# Containerization — Interview Prep

---

## Core Concepts

### What is a Container?
A container packages an application + its dependencies (runtime, libraries, config) into a single, portable unit. Runs identically on any machine with a container runtime.

**Container vs VM:**
| | Container | VM |
|---|---|---|
| Boot time | Seconds | Minutes |
| Size | MBs | GBs |
| Isolation | Process-level | Full OS |
| Overhead | Low | High |
| Use case | Microservices, cloud-native | Legacy apps, full OS control |

---

## Docker (Foundation)

### Key Concepts

**Dockerfile** — instructions to build an image
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/MyApp.dll"]
```

**Image** — built artifact (blueprint)
**Container** — running instance of an image
**Registry** — where images are stored (Docker Hub, Amazon ECR)

### Essential Docker Commands
```bash
docker build -t myapp:1.0 .        # Build image
docker run -p 8080:80 myapp:1.0    # Run container
docker ps                           # List running containers
docker images                       # List local images
docker push myrepo/myapp:1.0       # Push to registry
```

### .NET App in Docker (Relevant to Your Stack)
```dockerfile
# Multi-stage build for ASP.NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Amazon ECR (Elastic Container Registry)

AWS-managed Docker image registry.

```bash
# Authenticate to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account>.dkr.ecr.us-east-1.amazonaws.com

# Tag and push
docker tag myapp:1.0 <account>.dkr.ecr.us-east-1.amazonaws.com/myapp:1.0
docker push <account>.dkr.ecr.us-east-1.amazonaws.com/myapp:1.0
```

**Why ECR over Docker Hub?** Private, IAM-integrated, scans for vulnerabilities, native to AWS.

---

## Amazon ECS (Elastic Container Service)

AWS-managed container orchestration — run Docker containers without managing Kubernetes.

### Key Concepts
- **Task Definition** — blueprint: which image, CPU/memory, ports, environment vars
- **Task** — running instance of a task definition (like a container)
- **Service** — maintains desired number of tasks, handles restarts
- **Cluster** — logical grouping of compute resources

### Launch Types
| | EC2 | Fargate |
|---|---|---|
| Infra management | You manage EC2 instances | AWS manages it (serverless) |
| Control | More | Less |
| Cost | Potentially cheaper at scale | Pay per task (no idle cost) |
| Use case | Predictable, high-throughput | Variable workloads, simplicity |

### ECS Architecture
```
ECS Cluster
  └── Service (desired count: 3)
        ├── Task (container: myapp, port: 80)
        ├── Task (container: myapp, port: 80)
        └── Task (container: myapp, port: 80)
              ↓
       Application Load Balancer
              ↓
           Internet
```

---

## Amazon EKS (Elastic Kubernetes Service)

Managed Kubernetes. More complex than ECS but industry standard for large-scale microservices.

### Key Kubernetes Concepts (for interviews)
- **Pod** — smallest deployable unit, wraps one or more containers
- **Deployment** — manages replicas of a pod, handles rolling updates
- **Service** — stable network endpoint for pods
- **Namespace** — logical isolation within a cluster
- **ConfigMap/Secret** — config and sensitive data for pods
- **Ingress** — HTTP/HTTPS routing into the cluster

### ECS vs EKS
| | ECS | EKS |
|---|---|---|
| Complexity | Lower | Higher |
| AWS lock-in | Higher | Lower (Kubernetes is portable) |
| Learning curve | Gentle | Steep |
| Best for | AWS-native teams | Kubernetes-savvy teams, multi-cloud |

---

## App2Container (AWS) — Critical for JD

> See dedicated file `03-amazon-q-app2container-cast.md` for deep coverage.

**Quick summary:** AWS tool that automatically containerizes existing .NET and Java apps running on Windows/Linux servers. Generates Docker images + ECS/EKS deployment artifacts. Perfect for legacy .NET/ASP.NET modernization.

**Your angle:** "App2Container directly addresses modernizing the .NET/ASP.NET applications mentioned in the JD — it automates the containerization lift-and-shift so teams don't have to manually Dockerize each service."

---

## Containerization Interview Questions

**Q: Why containerize applications?**
> Consistency across environments (dev/staging/prod), faster deployments, resource efficiency, easier scaling, microservices enablement, and portability.

**Q: What's the difference between ECS and EKS?**
> ECS is AWS-proprietary, simpler to operate, tightly integrated with AWS services. EKS runs Kubernetes — more portable, industry-standard, higher operational complexity. Choose ECS for AWS-native simplicity, EKS for Kubernetes expertise or multi-cloud portability.

**Q: What is a multi-stage Docker build and why use it?**
> Separate build environment from runtime environment. SDK layer (large) used only for compilation; final image only contains the runtime + binary. Results in smaller, more secure production images.

**Q: How do you pass configuration to containers?**
> Environment variables, AWS Secrets Manager (via ECS task definition secrets), SSM Parameter Store, or mounted config files. Avoid baking secrets into images.

**Q: How do containers handle state?**
> Containers are stateless by design. Persistent state goes to external stores: databases (RDS), object storage (S3), or mounted volumes (EFS). This enables horizontal scaling.
