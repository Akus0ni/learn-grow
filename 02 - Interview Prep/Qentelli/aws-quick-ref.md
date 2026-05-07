# AWS — Quick Reference for .NET Developers

## Core Services You Must Know

### EC2 (Elastic Compute Cloud)
- Virtual servers in the cloud
- **Instance types**: t3.micro (dev/test), m5.large (general), c5 (compute-heavy)
- **AMI** (Amazon Machine Image) = OS + pre-installed software snapshot
- **Security Groups** = virtual firewall (inbound/outbound rules)
- **Key pairs** = SSH access to Linux instances
- **Auto Scaling Group (ASG)** = automatically add/remove EC2s based on load
- **Load Balancer (ALB)** = distributes HTTP traffic across multiple EC2s

**When to use EC2 over Lambda:** Long-running processes, stateful workloads, need full OS control, >15 min execution.

### S3 (Simple Storage Service)
- Object storage — files, images, backups, static sites
- **Bucket** = container; **Object** = file + metadata
- Globally unique bucket names
- **Storage classes**: Standard, Infrequent Access (IA), Glacier (archival)
- **Pre-signed URLs** — temporary access to private objects
- **Lifecycle policies** — auto-move old files to cheaper storage
- **Versioning** — keep multiple versions of an object

**From .NET (AWSSDK.S3):**
```csharp
var client = new AmazonS3Client(RegionEndpoint.USEast1);

// Upload
await client.PutObjectAsync(new PutObjectRequest {
    BucketName = "my-bucket",
    Key = "uploads/file.pdf",
    FilePath = localPath
});

// Download
var response = await client.GetObjectAsync("my-bucket", "uploads/file.pdf");

// Pre-signed URL
var urlRequest = new GetPreSignedUrlRequest {
    BucketName = "my-bucket",
    Key = "uploads/file.pdf",
    Expires = DateTime.UtcNow.AddHours(1)
};
string url = client.GetPreSignedURL(urlRequest);
```

### Lambda
- Serverless compute — runs code in response to events
- **Triggers**: API Gateway, S3 events, SNS, SQS, EventBridge (scheduled)
- **Limits**: 15 min max execution, 10 GB memory, 512 MB–10 GB ephemeral storage
- **Cold start** — first invocation spins up container (latency spike); mitigate with provisioned concurrency
- Pay per invocation + duration (ms)

**When to use Lambda over EC2:** Short-lived tasks, event-driven, infrequent traffic, want no server management.

**From .NET:**
```csharp
// Lambda function handler
public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine($"Request: {request.Body}");
        return new APIGatewayProxyResponse { StatusCode = 200, Body = "OK" };
    }
}
```

### RDS (Relational Database Service)
- Managed SQL databases: MySQL, PostgreSQL, SQL Server, Aurora
- **Multi-AZ** = standby replica in another availability zone (automatic failover)
- **Read Replicas** = scale read workload across multiple copies
- **Aurora** = AWS's own engine, MySQL/PostgreSQL compatible, 5x faster than MySQL
- **Parameter Groups** = DB config settings
- Automated backups, point-in-time restore

**Connection from .NET:** Standard connection string — RDS endpoint replaces server name. Use AWS Secrets Manager for credentials.

### API Gateway
- HTTP/REST/WebSocket API front door
- Routes requests to Lambda, EC2, or any HTTP endpoint
- Handles auth (Cognito, Lambda authorizers), throttling, caching
- **Usage Plans + API Keys** for rate limiting per client

---

## IAM (Identity & Access Management)

- **Users** — individual humans
- **Roles** — assumed by services (EC2, Lambda, etc.) — no passwords
- **Policies** — JSON documents defining Allow/Deny for actions on resources
- **Principle of Least Privilege** — grant only what's needed

```json
{
  "Effect": "Allow",
  "Action": ["s3:GetObject", "s3:PutObject"],
  "Resource": "arn:aws:s3:::my-bucket/*"
}
```

**EC2/Lambda use Roles, not Access Keys** — best practice.

---

## CloudWatch
- **Logs** — aggregate application logs (from Lambda, EC2, ECS)
- **Metrics** — CPU, memory, custom app metrics
- **Alarms** — trigger SNS notification or Auto Scaling when metric breaches threshold
- **Log Insights** — query logs with SQL-like syntax

```csharp
// Push custom metric from .NET
var cw = new AmazonCloudWatchClient();
await cw.PutMetricDataAsync(new PutMetricDataRequest {
    Namespace = "MyApp",
    MetricData = new List<MetricDatum> {
        new() { MetricName = "OrdersProcessed", Value = 1, Unit = StandardUnit.Count }
    }
});
```

---

## SNS & SQS (Messaging)

| | SNS | SQS |
|-|-----|-----|
| Type | Pub/Sub | Queue |
| Use | Fan-out to many subscribers | Decouple producers from consumers |
| Delivery | Push (immediate) | Pull (consumers poll) |
| Retention | None | Up to 14 days |

**Pattern**: SNS → SQS (fan-out + buffer) — SNS pushes to multiple SQS queues.

---

## Secrets Manager vs Parameter Store
- **Secrets Manager** — for secrets (DB passwords, API keys), auto-rotation, costs $0.40/secret/month
- **Parameter Store** — config values, free tier available, no auto-rotation

---

## Security Best Practices (key talking points)
1. Never hardcode credentials — use IAM Roles or Secrets Manager
2. Enable VPC for RDS — no public access
3. Use Security Groups to restrict traffic to minimum ports
4. Enable S3 bucket versioning + block public access by default
5. Use HTTPS everywhere — SSL/TLS on ALB
6. Enable CloudTrail for audit logging of all API calls
7. Encrypt data at rest (S3 SSE, RDS encryption) and in transit (TLS)

---

## Common Architecture: .NET Web API on AWS
```
Client → Route 53 (DNS)
       → ALB (Load Balancer)
       → EC2 Auto Scaling Group (ASP.NET Core API)
       → RDS (SQL Server / PostgreSQL) in private subnet
       → S3 (file storage)
       → ElastiCache (Redis for caching)
       → CloudWatch (logs + metrics)
       → Secrets Manager (DB credentials)
```

## Lambda Architecture: Event-Driven
```
S3 Upload → Lambda (process file) → RDS / DynamoDB
API Gateway → Lambda → Business Logic → RDS
EventBridge (cron) → Lambda (scheduled job)
SQS → Lambda (process messages in batch)
```
