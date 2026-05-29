# AWS CloudWatch & Observability

## 1. Core Concepts

### What is Observability?
Observability is the ability to understand the internal state of a system by examining its external outputs. The three pillars:
- **Metrics**: Numeric measurements over time (CPU %, request count, error rate)
- **Logs**: Timestamped text records of events (app logs, access logs, audit logs)
- **Traces**: End-to-end request journey through distributed services

AWS's native observability stack:
- **CloudWatch**: Metrics + Logs + Alarms + Dashboards
- **AWS X-Ray**: Distributed tracing
- **AWS CloudTrail**: API audit logging
- **Amazon OpenSearch Service**: Log analytics at scale

---

## 2. CloudWatch Metrics

### What are CloudWatch Metrics?
Metrics are time-ordered data points published by AWS services or your application. Each metric has:
- **Namespace**: e.g., `AWS/EC2`, `AWS/Lambda`, `MyApp/Orders`
- **Metric name**: e.g., `CPUUtilization`, `Errors`, `OrderCount`
- **Dimensions**: Key-value pairs that identify the metric (e.g., `InstanceId=i-1234`)
- **Value**: Numeric
- **Unit**: Seconds, Count, Bytes, Percent, etc.
- **Timestamp**: When it was recorded

### Default vs Custom Metrics
| | Default Metrics | Custom Metrics |
|--|----------------|----------------|
| Source | AWS services automatically | Your application via SDK/CLI |
| Cost | Free | $0.30/metric/month (Standard), $0.02/metric/month (High Resolution) |
| Resolution | 1-minute (Standard), 1-second (Detailed) | 1-second to 1-minute |
| Examples | EC2 CPUUtilization, Lambda Duration | OrdersPerMinute, ActiveUsers, CacheHitRate |

**Important**: Default EC2 metrics do NOT include RAM or disk usage — use **CloudWatch Agent** for those.

### CloudWatch Agent
Install on EC2/on-premises servers to collect:
- **Memory utilization**: Not available in default EC2 metrics
- **Disk space**: Not in default EC2 metrics
- **Swap usage**
- **Application logs**: Any log file
- **System metrics**: netstat, processes

Configuration via SSM Parameter Store for centralized management.

### Metric Math
Combine multiple metrics into a computed metric:
- Error rate: `m1/m2 * 100` (Errors / Requests × 100)
- Request rate: `RATE(m1)` (calculate per-second rate)
- Anomaly detection band: `ANOMALY_DETECTION_BAND(m1, 2)`

### CloudWatch Metric Streams
Stream CloudWatch metrics in near-real time to:
- **Amazon Kinesis Firehose** → S3 / OpenSearch / Splunk / Datadog / New Relic
- Format: JSON or OpenTelemetry Protocol (OTLP)
- Low latency (~2-3 minutes) vs polling APIs (5-minute minimum aggregation)

---

## 3. CloudWatch Alarms

### What are CloudWatch Alarms?
Alarms watch a metric and change state based on a threshold:
- **OK**: Metric within threshold
- **ALARM**: Metric breached threshold
- **INSUFFICIENT_DATA**: Not enough data to evaluate

### Alarm Actions
- **SNS notification**: Email, SMS, HTTP webhook, Lambda, SQS, Teams/Slack (via Lambda)
- **Auto Scaling**: Scale EC2 ASG up/down
- **EC2 action**: Stop, terminate, reboot, or recover an instance
- **Systems Manager OpsItem**: Create an incident

### Alarm Types
| Type | Description | Use Case |
|------|-------------|---------|
| **Static threshold** | Fixed value (CPU > 80%) | Known baseline thresholds |
| **Anomaly detection** | ML-based — alarm when outside historical pattern | Variable workloads with no fixed threshold |
| **Metric math** | Computed expression-based | Error rate = errors/requests |
| **Composite alarm** | AND/OR of multiple alarms | Reduce alarm noise — alert only when both CPU AND memory are high |

### Best Practice Alarm Configuration
```
Alarm: HighErrorRate
Metric: 5XXError (API Gateway)
Statistic: Sum
Period: 60 seconds
Evaluation Periods: 3
Datapoints to Alarm: 2  ← 2 of 3 periods must breach (reduces false alarms)
Threshold: > 10
Actions: SNS → PagerDuty
```

---

## 4. CloudWatch Logs

### What are CloudWatch Logs?
CloudWatch Logs collects, monitors, and stores log data. Structure:
- **Log Group**: Container for log streams (e.g., `/aws/lambda/my-function`)
- **Log Stream**: Sequence of events from one source (e.g., one Lambda instance)
- **Log Events**: Individual records with timestamp + message

### Log Retention
Default: Logs never expire (cost accumulates). Set retention periods:
- Lambda: 14 days (adjust per function)
- Application logs: 30–90 days
- Audit/compliance: 1–7 years → archive to S3 via subscription filter

### CloudWatch Logs Insights
Interactive SQL-like query language for analyzing logs:
```sql
fields @timestamp, @message, @logStream
| filter @message like /ERROR/
| stats count(*) as errorCount by bin(5m)
| sort errorCount desc
| limit 20
```

Useful queries:
- Top 10 slowest Lambda invocations
- Error frequency per hour
- P99 latency calculation

### Subscription Filters
Stream log events in real time to:
- **Lambda**: Custom processing (parse, enrich, alert)
- **Kinesis Data Streams**: High-throughput processing
- **Kinesis Firehose**: Deliver to S3, OpenSearch, Splunk

Common pattern: CloudWatch Logs → Kinesis Firehose → S3 (long-term archive) + OpenSearch (real-time search).

### CloudWatch Logs Anomaly Detection
ML-based detection of unusual patterns in log data:
- Detects new log patterns never seen before
- Detects unusual frequency of known patterns
- Automatic — no threshold to configure

---

## 5. CloudWatch Dashboards

- **Automatic dashboards**: AWS creates per-service dashboards
- **Custom dashboards**: Combine metrics, alarms, text, logs insights queries, alarms in one view
- **Cross-account, cross-region dashboards**: View your entire infrastructure in one place
- **Sharing**: Share dashboards with specific users or make them public (read-only)

---

## 6. AWS X-Ray — Distributed Tracing

### What is X-Ray?
X-Ray traces requests as they travel through distributed systems — API Gateway → Lambda → DynamoDB → SQS. You see the end-to-end latency breakdown, identify bottlenecks, and debug errors across service boundaries.

### Key Concepts
| Term | Description |
|------|-------------|
| **Trace** | Full end-to-end journey of a request |
| **Segment** | Work done by one service in the trace |
| **Subsegment** | Breakdown within a service (DB call, HTTP call) |
| **Annotation** | Key-value indexed for filtering traces |
| **Metadata** | Key-value not indexed — stored with the trace |
| **Sampling** | Percentage of requests traced (default: 5% + 1 per second) |
| **Group** | Filter expression to create a trace group (e.g., all errors) |

### X-Ray with Lambda
Enable active tracing on Lambda function → Lambda service automatically creates segments. Use X-Ray SDK to instrument:
```python
from aws_xray_sdk.core import xray_recorder, patch_all
patch_all()  # Patches boto3 calls automatically

@xray_recorder.capture('process_order')
def process_order(order_id):
    xray_recorder.current_segment().put_annotation('orderId', order_id)
    # ... code here creates a subsegment
```

### X-Ray Service Map
Visual map of all services in your application with:
- Latency per service (P50, P99)
- Error/fault/throttle rates
- Connections between services
- Identify which service is the bottleneck

---

## 7. AWS CloudTrail

### What is CloudTrail?
CloudTrail records **AWS API calls** made in your account — who did what, when, from where.

| Trail Type | Coverage |
|-----------|----------|
| **Management Events** | Control plane (CreateBucket, RunInstances, CreateUser) — enabled by default |
| **Data Events** | S3 object-level (GetObject, PutObject), Lambda invocations — extra cost |
| **Insights Events** | Detect unusual API activity — automated anomaly detection on management events |

### CloudTrail Best Practices
- Enable a **multi-region trail** in a dedicated **Security account** (no one in other accounts can delete it)
- **S3 bucket** with MFA Delete enabled for trail logs
- Enable **Log file integrity validation** — SHA-256 hash per file, detect tampering
- Use **Athena** to query CloudTrail logs in S3: "Who deleted this S3 object?"
- **EventBridge**: Trigger alerts on specific API calls (e.g., alert when root login detected, security group changed)

---

## 8. Amazon Managed Grafana & Prometheus

- **Amazon Managed Prometheus (AMP)**: Managed Prometheus for EKS/ECS metrics
- **Amazon Managed Grafana**: Managed Grafana for visualization; integrates with CloudWatch, AMP, X-Ray, OpenSearch

---

## 9. Interview Questions & Answers

### Q1. What are the three pillars of observability in AWS?
1. **Metrics** (CloudWatch Metrics): Numeric measurements over time — CPU, memory, error rate, latency
2. **Logs** (CloudWatch Logs): Timestamped event records — application logs, access logs, audit logs
3. **Traces** (AWS X-Ray): End-to-end request journeys through distributed services

CloudTrail is sometimes called the fourth pillar: **audit/events** — tracking AWS API calls for security and compliance.

---

### Q2. What is the difference between CloudWatch Logs and CloudTrail?
**CloudWatch Logs**: Collects logs from your application, EC2 instances, Lambda functions, containers. You can write anything to it. Used for operational visibility, debugging, alerting.

**CloudTrail**: Automatically records AWS API calls (who called which AWS API, from which IP, when). Used for security audit, compliance, incident investigation.

Example: If an S3 bucket was deleted, CloudTrail tells you who deleted it and when. CloudWatch Logs tells you what the application logged when accessing that bucket.

---

### Q3. What is the default memory/disk metric for EC2? How do you get them?
**Default EC2 metrics DO NOT include memory or disk** — they come from the hypervisor and AWS can't access the guest OS metrics without an agent.

To collect memory and disk: Install the **CloudWatch Agent** on EC2 instances. The agent reads OS-level metrics and pushes them as custom metrics to CloudWatch. Configure via SSM Parameter Store for centralized fleet management.

---

### Q4. What is an Anomaly Detection alarm in CloudWatch?
CloudWatch Anomaly Detection uses ML to model the expected value of a metric based on historical patterns (including time-of-day and day-of-week seasonality). An alarm fires when the metric falls outside the expected band.

**Advantage over static threshold**: Works for variable workloads. A spike on Black Friday is normal; a spike on a Tuesday night at 3 AM is anomalous.

---

### Q5. How does AWS X-Ray sampling work?
X-Ray doesn't trace every request by default — that would be expensive and noisy. Sampling rules:
- **Default**: 1 request per second + 5% of additional requests
- **Custom rules**: Configure by service name, HTTP method, URL path, etc.
- **Reservoir**: Guaranteed traces per second
- **Rate**: Percentage of additional requests above reservoir

For production, sample 5–10% for low-cost visibility. Increase for specific critical paths or during incidents.

---

### Q6. What is the difference between a CloudWatch Alarm and a CloudWatch Event (EventBridge)?
**CloudWatch Alarm**: Watches a metric value. Fires when a threshold is breached (e.g., CPU > 80%). Best for metric-based thresholds and operational alerts.

**EventBridge (CloudWatch Events)**: Event-driven routing. Responds to events (state changes, API calls, schedules). Examples:
- EC2 instance state changed → trigger Lambda to update CMDB
- CloudTrail detects root login → trigger SNS alert
- Schedule: Every 5 minutes → trigger Lambda

Alarms = metric thresholds; EventBridge = event routing for state changes and API actions.

---

### Q7. How do you query CloudWatch Logs at scale?
**CloudWatch Logs Insights**: Ad-hoc interactive queries. Good for tens of GB. Results in seconds to minutes.

**S3 + Athena**: Export logs to S3 (using subscription filter or export), query with SQL via Athena. Better for petabytes and complex joins across multiple log sources.

**OpenSearch**: For full-text search, complex aggregations, Kibana dashboards. Use for application-level log analytics requiring sub-second search.

---

### Q8. What is a Composite Alarm in CloudWatch?
A Composite Alarm combines multiple alarms using AND/OR logic. Fires only when all/any conditions are met.

Use case: Reduce alarm noise — only page the on-call engineer when BOTH `HighCPU` AND `HighErrorRate` are in ALARM state simultaneously (likely a real incident), not when only CPU spikes (might be legitimate traffic).

```
CompositeAlarm: ALARM("HighCPU") AND ALARM("HighErrorRate")
```

---

### Q9. How do you send CloudWatch alarms to Slack?
1. Create an **SNS topic**
2. Create a **Lambda function** that formats the SNS message and calls the Slack Incoming Webhook URL
3. Subscribe Lambda to the SNS topic
4. Set the SNS topic as the alarm action

Alternative: Use **AWS Chatbot** — native integration between CloudWatch/SNS and Slack/Teams channels without a custom Lambda.

---

### Q10. What is CloudWatch Container Insights?
Container Insights collects, aggregates, and analyzes metrics and logs from containerized applications (ECS, EKS, K8s). Provides:
- CPU and memory per cluster, service, task/pod
- Network and disk I/O
- Pre-built dashboards
- Automated diagnostics

For EKS: Deployed as a DaemonSet (CloudWatch Agent + Fluent Bit). For ECS: Enable via cluster settings.

---

### Q11. How do you create a custom CloudWatch metric from your application?
Using the AWS SDK (Python example):
```python
import boto3
cloudwatch = boto3.client('cloudwatch')

cloudwatch.put_metric_data(
    Namespace='MyApp/Orders',
    MetricData=[{
        'MetricName': 'OrdersProcessed',
        'Value': 42,
        'Unit': 'Count',
        'Dimensions': [
            {'Name': 'Environment', 'Value': 'prod'},
            {'Name': 'Region', 'Value': 'us-east-1'}
        ]
    }]
)
```
Or use **EMF (Embedded Metrics Format)**: Embed metrics in structured log lines — CloudWatch automatically extracts them as metrics. Zero additional API calls.

---

### Q12. What is CloudWatch Synthetics?
Synthetics creates **canary scripts** — automated testing that simulates user traffic to your endpoints. A canary is a Lambda function that runs on a schedule and:
- Loads a URL and checks response
- Simulates user flows (login, search, checkout)
- Checks API endpoints and validates responses
- Alerts when the endpoint is down or response changes

Catches user-facing issues before real users experience them.

---

### Q13. Explain the CloudTrail log file integrity validation feature.
When enabled, CloudTrail creates a **digest file** every hour containing:
- SHA-256 hashes of all log files delivered in that hour
- The digest file itself is signed with CloudTrail's private key

To verify: `aws cloudtrail validate-logs --trail-arn ... --start-time ...`

This detects: log file deletion, modification, or injection — critical for forensic investigations and compliance (SOC 2, PCI-DSS).

---

### Q14. What is AWS X-Ray Service Map?
A visual diagram auto-generated by X-Ray showing your application's architecture based on actual trace data:
- Nodes: Each service (API Gateway, Lambda, DynamoDB, SQS)
- Edges: Connections between services
- Color coding: Green (OK), Yellow (throttled), Red (faulted)
- Latency ring: Shows P50/P99 response time

Invaluable for identifying bottlenecks in microservices — shows which service is actually slow.

---

### Q15. How do you set up a complete observability stack for a serverless application?
```
Application:
  Lambda → EMF logs with metrics (requests, errors, latency)
  Lambda → Active X-Ray tracing

Metrics:
  CloudWatch custom metrics (EMF)
  Alarm: ErrorRate > 1% → SNS → PagerDuty
  Alarm: P99Latency > 1000ms → SNS → Slack
  Anomaly Detection on OrderCount

Logs:
  CloudWatch Logs (/aws/lambda/function-name)
  Retention: 14 days
  Subscription Filter → Kinesis Firehose → S3 (1-year archive)
  CloudWatch Logs Insights: saved queries for common investigations

Traces:
  X-Ray: 10% sampling in prod, 100% on errors
  X-Ray Groups: Error traces only
  X-Ray Service Map: verify service topology

Audit:
  CloudTrail: multi-region, to Security Account S3 bucket
  Integrity validation: enabled
  EventBridge: root login → immediate SNS alert

Dashboard:
  CloudWatch Dashboard: requests/min, error rate, P99 latency, Lambda concurrent executions
  X-Ray Service Map embedded
```

---

## 10. Real-World Use Case: Full Observability Stack for an E-Commerce Platform

### Scenario
A retail platform with 50 microservices needs:
- Know immediately when checkout fails (< 1 min MTTR)
- Trace any slow request end-to-end through 8 services
- Audit who deleted any order record
- 12-month log retention for PCI-DSS
- One dashboard for entire platform

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     METRICS LAYER                                │
│                                                                  │
│  ECS Tasks → CloudWatch Agent → CW Metrics (CPU, mem, disk)    │
│  Lambda → EMF → CW Custom Metrics (OrderCount, CheckoutErrors) │
│  RDS → CW Metrics (DB connections, IOPS, replica lag)          │
│  API GW → CW Metrics (5XX, latency, cache hits)                │
│                                                                  │
│  Alarms:                                                         │
│    CheckoutError > 0.1% → CRITICAL → PagerDuty (immediate)     │
│    API Latency P99 > 2s → WARNING → Slack                       │
│    RDS Connections > 80% → WARNING → Slack                      │
│    Composite: CriticalAlarm = CPU AND Memory AND ErrorRate      │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                      LOGS LAYER                                  │
│                                                                  │
│  App logs → CloudWatch Logs (7-day retention)                   │
│  Access logs → CloudWatch Logs (30-day retention)               │
│  Subscription Filter → Kinesis Firehose → S3 (12-month)        │
│  S3 → Athena → query historical logs                            │
│  CloudWatch Logs Insights: saved queries per service team       │
│  Anomaly Detection: alert on new error patterns                 │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                     TRACING LAYER                                │
│                                                                  │
│  API GW + Lambda + ECS → X-Ray SDK (active tracing)            │
│  X-Ray sampling: 5% normal, 100% on errors                     │
│  X-Ray Groups: CheckoutErrors, SlowRequests (>2s)               │
│  X-Ray Service Map: 50-service topology, color-coded health     │
│  X-Ray Insights: automated anomaly detection in trace data      │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                      AUDIT LAYER                                 │
│                                                                  │
│  CloudTrail → Security Account S3 (immutable, MFA Delete)      │
│  Integrity validation → detect log tampering                    │
│  EventBridge rules:                                              │
│    Root login → immediate SNS alert                             │
│    Security group changed → SNS + Jira ticket                   │
│    S3 bucket public → automatic remediation Lambda              │
│  Athena: query 12 months of API activity for PCI-DSS audit     │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                   DASHBOARD LAYER                                │
│                                                                  │
│  CloudWatch Dashboard (NOC view):                               │
│    - Request rate (all services)                                 │
│    - Error rate (5XX, 4XX per service)                          │
│    - P99 latency (API GW, checkout, payment)                    │
│    - Lambda concurrent executions                               │
│    - RDS connections, IOPS, replica lag                         │
│    - Active alarms widget                                        │
│    - X-Ray service map (embedded)                               │
│    - CloudWatch Synthetics: checkout health (canary)            │
└─────────────────────────────────────────────────────────────────┘
```

### Alert Routing Strategy

| Severity | Condition | Action | Response |
|----------|-----------|--------|---------|
| P1 Critical | Checkout error > 0.1% | PagerDuty → on-call | 5-min response |
| P2 High | API latency P99 > 2s | PagerDuty → team | 15-min response |
| P3 Medium | Non-critical service errors | Slack #ops-alerts | Next business hour |
| P4 Low | Metrics anomaly detected | Jira ticket auto-created | Planned review |

### Interview Narration (White-board Script)
> "For observability I cover the three pillars: metrics, logs, and traces. For metrics, everything flows to CloudWatch — EC2 and ECS use the CloudWatch Agent for memory and disk; Lambda uses EMF to embed business metrics in log lines without extra API calls. Alarms are layered: service-level alarms for error rates, and composite alarms that only page the on-call engineer when multiple signals align — avoiding false alarms from momentary CPU spikes. Logs go to CloudWatch with tiered retention: 7 days in CW, then Kinesis Firehose delivers to S3 for 12 months for PCI compliance. For tracing, X-Ray instruments every service; the Service Map gives us a real-time topology of which service is the bottleneck. CloudTrail feeds into the Security account — nobody in production can tamper with the audit logs."
