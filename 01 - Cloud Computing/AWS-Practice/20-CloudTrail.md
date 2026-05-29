# AWS CloudTrail — Governance, Compliance & Audit

## 1. Core Concepts

### What is CloudTrail?
CloudTrail is AWS's **governance, compliance, and auditing service**. It records **API calls** made to AWS services — by the console, SDK, CLI, or other services — creating an immutable audit trail of "who did what, when, and from where."

### What CloudTrail Records
Every CloudTrail event captures:
| Field | Example |
|-------|---------|
| `eventTime` | `2024-01-15T10:30:00Z` |
| `eventName` | `RunInstances`, `DeleteBucket`, `AssumeRole` |
| `eventSource` | `ec2.amazonaws.com`, `s3.amazonaws.com` |
| `userIdentity` | IAM user, role, federated user, or AWS service |
| `sourceIPAddress` | Client IP address |
| `requestParameters` | API call inputs |
| `responseElements` | API call outputs |
| `errorCode` | Error if the call failed |
| `awsRegion` | Region where the API was called |

### Event Types
| Type | Description | Default |
|------|-------------|---------|
| **Management Events** | Control plane operations: `CreateBucket`, `RunInstances`, `PutBucketPolicy`, `AssumeRole` | Enabled |
| **Data Events** | Data plane operations: S3 `GetObject`/`PutObject`, Lambda `Invoke`, DynamoDB `GetItem` | Disabled (high volume, extra cost) |
| **Insights Events** | Anomalous activity detection (unusual API call rates, unusual error rates) | Disabled (extra cost) |
| **Network Activity Events** | VPC endpoint activity for AWS services | Disabled |

### Trail Types
| Type | Coverage |
|------|---------|
| **Single-region trail** | Records events in one region |
| **Multi-region trail** | Records events across all current and future regions (recommended) |
| **Organization trail** | Records events across all AWS accounts in an AWS Organization |

### Event Delivery
- Events delivered to **S3 within ~15 minutes** (typically within 5 minutes)
- Events delivered to **CloudWatch Logs** in near-real time (optional)
- Events delivered to **CloudWatch Events / EventBridge** for automated response
- Latest events visible in the **Event History** console for 90 days (free, no trail required)

### CloudTrail Insights
**Insights** analyze CloudTrail management events for:
- Unusual write API call volume (e.g., sudden spike in `DeleteObject` calls)
- Unusual error rates (e.g., spike in `AccessDenied` errors → possible credential compromise)

When an anomaly is detected, an Insights event is generated and can trigger an EventBridge rule → Lambda/SNS for automated response.

### Security Best Practices
| Practice | Why |
|----------|-----|
| Enable **multi-region trail** | Catch activity in all regions, including unexpected ones |
| Enable **log file validation** | SHA-256 hash of each log file + manifest signed by CloudTrail; detect tampering |
| Send logs to **dedicated S3 bucket** | Separate account with restrictive access; prevent log deletion |
| Enable **MFA delete** on log bucket | Prevents deletion even with root credentials |
| Enable **S3 Object Lock** on log bucket | WORM protection; meet compliance requirements (PCI-DSS, HIPAA, SOC 2) |
| Enable **CloudWatch Logs** delivery | Real-time monitoring and alerting on CloudTrail events |
| Use **AWS Config** alongside CloudTrail | CloudTrail = who/what/when; Config = resource configuration history |

### Integration with Other Services
| Service | Integration |
|---------|------------|
| **CloudWatch Logs** | Near-real-time log streaming; create metric filters and alarms |
| **EventBridge** | Event-driven response to specific API calls |
| **Athena** | SQL query CloudTrail logs stored in S3 |
| **AWS Security Hub** | Aggregates CloudTrail findings with other security findings |
| **AWS Config** | Correlate API calls with resource configuration changes |
| **GuardDuty** | Analyzes CloudTrail events for threat detection |

### CloudTrail Lake
**CloudTrail Lake** is a managed data lake for CloudTrail events. Events are stored in an **event data store** (retain for 7 years), queryable with SQL (no S3 + Athena setup required). Features:
- Multi-region, multi-account event aggregation
- Faster queries than S3 + Athena
- Integration with AWS Glue and QuickSight for dashboards

---

## 2. Interview Questions & Answers

### Q1. What is CloudTrail and what does it track?
CloudTrail tracks **API calls** made to AWS services — every `CreateBucket`, `RunInstances`, `AssumeRole`, `DeleteUser`, etc. It records who made the call, from what IP, at what time, what parameters were used, and whether it succeeded. This creates an immutable audit trail for compliance and forensic investigation.

---

### Q2. What is the difference between management events and data events in CloudTrail?
**Management events** (control plane) track operations that change the configuration of AWS resources: creating/deleting resources, modifying IAM policies, VPC changes. These are enabled by default.  
**Data events** (data plane) track high-volume operations on data within resources: S3 `GetObject`, Lambda `Invoke`, DynamoDB `PutItem`. These are disabled by default because the volume can be extremely high (millions/hour) and generate significant cost. Enable data events selectively for high-risk resources.

---

### Q3. How do you query CloudTrail logs for a specific event?
**Option 1 — Event History (console)**: Filter by event name, user, resource, or time in the last 90 days. No trail required.  
**Option 2 — Athena query on S3**: Point Athena at the CloudTrail S3 bucket, partition by date and region, and run SQL:
```sql
SELECT eventTime, userIdentity.arn, sourceIPAddress, requestParameters
FROM cloudtrail_logs
WHERE eventName = 'DeleteBucket'
  AND eventTime > '2024-01-01'
ORDER BY eventTime DESC;
```
**Option 3 — CloudTrail Lake**: Use the Lake's native SQL query editor — faster and simpler for multi-account queries.

---

### Q4. What is log file validation and why is it important?
When log file validation is enabled, CloudTrail generates a **digest file** for each hour's log files containing SHA-256 hashes of each log file. The digest file itself is signed by CloudTrail's private key.  
To validate: use `aws cloudtrail validate-logs` CLI command. It detects if any log file was **modified, deleted, or forged** after delivery.  
Important for: compliance (proves logs haven't been tampered with by an attacker covering tracks), forensic investigations, and audit requirements (PCI-DSS, SOC 2).

---

### Q5. How would you detect unauthorized API calls in real time?
1. Enable a **multi-region trail** with CloudWatch Logs delivery
2. Create **CloudWatch Metric Filters** on the log group for specific patterns:
   - `{ $.errorCode = "AccessDenied" }` → alarm when > 5 per 5 minutes
   - `{ $.eventName = "ConsoleLogin" && $.additionalEventData.MFAUsed != "Yes" }` → alarm on non-MFA console login
   - `{ $.eventName = "DeleteTrail" || $.eventName = "StopLogging" }` → immediately alert if someone disables CloudTrail
3. Create **CloudWatch Alarms** → SNS → PagerDuty/Slack
4. Use **EventBridge rules** matching specific event patterns for automated remediation (e.g., trigger Lambda to re-enable CloudTrail if stopped)

---

### Q6. How do you protect CloudTrail logs from deletion by a compromised administrator?
1. Store logs in a **dedicated S3 bucket in a separate AWS account** — a compromised admin in the main account can't delete them
2. Enable **S3 Object Lock** (WORM mode) with a retention period — immutable even with root credentials
3. Enable **MFA Delete** on the bucket — deletion requires MFA even for root
4. Grant the CloudTrail service `PutObject` access via bucket policy but deny `DeleteObject` and `DeleteBucket` to all principals
5. Enable **log file validation** to detect tampering even if someone manages to modify files

---

### Q7. What is the difference between CloudTrail and AWS Config?
| | CloudTrail | AWS Config |
|--|-----------|-----------|
| Tracks | API calls (actions) | Resource configurations (state) |
| Question answered | "Who did what, when?" | "What does this resource look like? Did it change?" |
| Use case | Audit trail, forensics | Compliance, drift detection, configuration history |
| Storage | S3 (JSON event logs) | S3 + Config rules |
| Real-time | Near-real-time | Periodic + change-triggered |

Use both together: CloudTrail tells you who changed an S3 bucket policy; Config tells you what the policy changed from and to.

---

### Q8. How does GuardDuty use CloudTrail?
**GuardDuty** continuously analyzes CloudTrail management and data events (plus VPC Flow Logs and DNS logs) using ML and threat intelligence to detect:
- **Credential compromise**: API calls from unusual IPs, TOR nodes, or during unusual hours
- **Privilege escalation**: Anomalous IAM API calls (`CreateAccessKey`, `AttachRolePolicy`)
- **Data exfiltration**: Unusual S3 data access patterns
- **Cryptocurrency mining**: EC2 API calls associated with mining software

CloudTrail feeds GuardDuty — enable both for automated threat detection.

---

### Q9. What is CloudTrail Insights and how does it work?
**CloudTrail Insights** monitors CloudTrail management events for anomalous activity:
- Establishes a **baseline** of normal API call rates and error rates for your account
- Detects deviations: e.g., `TerminateInstances` normally called 2×/hour suddenly spikes to 50×/hour
- Generates an **Insights event** that can trigger EventBridge → Lambda/SNS for automated response

Cost: additional charge per event analyzed. Enable for accounts with compliance requirements or sensitive workloads.

---

### Q10. How do you centralize CloudTrail across a multi-account AWS Organization?
1. Enable a **CloudTrail Organization Trail** from the management account:
   - Applies to all existing and future member accounts automatically
   - Members cannot disable or modify the trail
2. Deliver all logs to a **centralized S3 bucket** in a dedicated security/logging account
3. Use **CloudTrail Lake** or **Athena** to query across all account logs centrally
4. Member accounts can still create their own trails for local use

---

### Q11. What should you do if CloudTrail is accidentally disabled?
1. Immediately **re-enable CloudTrail** (`StartLogging` API)
2. Investigate if the disabling was malicious:
   - Check Event History for the `StopLogging` or `DeleteTrail` event (90-day buffer even without a trail)
   - Check who made the call, from what IP, at what time
3. Review what happened during the gap (no trail = no audit log for that period)
4. Remediate: add an **EventBridge rule** to automatically re-enable CloudTrail if it's stopped (invoke a Lambda that calls `StartLogging`)
5. Add a **Config Rule** (managed rule `cloud-trail-enabled`) to alert and remediate

---

### Q12. What is the cost structure of CloudTrail?
| Feature | Cost |
|---------|------|
| Management events (first copy per region) | **Free** |
| Additional management event copies | $2.00 per 100,000 events |
| Data events | $0.10 per 100,000 events |
| Insights events | $0.35 per 100,000 write management events analyzed |
| CloudTrail Lake (event data store) | $0.005/GB ingested + $0.005/GB queried |
| S3 storage | Standard S3 pricing |

For most accounts, the primary cost is data events (S3 object-level logging) and S3 storage.

---

### Q13. How do you use CloudTrail to investigate a security incident?
Scenario: Unauthorized S3 data exfiltration suspected.
1. Search **Event History** or **Athena** for `GetObject` events on the sensitive bucket in the incident timeframe
2. Filter by `sourceIPAddress` to identify unusual IP addresses
3. Check `userIdentity` to determine which IAM user/role was used
4. Check CloudTrail for `AssumeRole` events around the same time (lateral movement)
5. Cross-reference with **GuardDuty** findings and **VPC Flow Logs** for network evidence
6. Check if the compromised credentials were subsequently used to create new IAM users or access keys

---

### Q14. What is the difference between a trail and Event History?
| | Event History | Trail |
|--|--------------|-------|
| Retention | 90 days | Indefinite (limited by S3 lifecycle/Lake retention) |
| Cost | Free | S3 storage + delivery costs |
| Coverage | Management events only | Management + data events + Insights |
| Regions | Requires access per region | Multi-region trail covers all |
| Query | Console UI only | Athena, CloudTrail Lake, SIEM tools |

Event History is for quick ad-hoc lookups; a trail is required for long-term compliance and SIEM integration.

---

### Q15. Design a CloudTrail architecture for a PCI-DSS compliant organization.
```
PCI-DSS Requirements:
- Audit all admin actions (Req 10.2)
- Detect unauthorized access (Req 10.6)
- Retain logs for 1 year (Req 10.7)
- Protect log integrity (Req 10.5)

Architecture:

[All AWS Accounts (Org)]
  └── Organization Trail (management account)
        ├── Management Events: ALL
        ├── Data Events: S3 (PCI-scope buckets), Lambda ALL
        ├── Insights: Enabled
        └── Deliver to → [S3: security-account/cloudtrail/]
                              ├── S3 Object Lock (WORM, 365-day retention)
                              ├── Log file validation enabled
                              ├── MFA Delete enabled
                              └── Access: CloudTrail PutObject only;
                                         no DeleteObject for all principals

CloudWatch Logs (near-real-time monitoring):
  ├── Metric Filter: AccessDenied → Alarm → SNS → SOC team
  ├── Metric Filter: ConsoleLoginWithoutMFA → Alarm → Block via Lambda
  ├── Metric Filter: DeleteTrail/StopLogging → Alarm + Auto-remediate
  └── Metric Filter: RootAccountUsage → Immediate PagerDuty alert

Athena:
  └── Partition CloudTrail logs by account/region/date
  └── PCI auditor self-service queries

AWS Config:
  └── Rule: cloud-trail-enabled (auto-remediate)
  └── Rule: s3-bucket-logging-enabled
```

---

## 3. Real-World Use Case: Compliance Audit Trail for FinTech

### Scenario
ShopWave processes credit card payments (PCI-DSS scope). A security audit requires:
- Immutable 12-month audit trail of all admin actions
- Real-time alerting on anomalous IAM activity
- Proof that logs haven't been tampered with

### Architecture

```
[ShopWave AWS Account]          [Security/Log Archive Account]
         │                                   │
  [Organization Trail]    ──delivers──→   [S3: cloudtrail-logs]
  Multi-region                              ├── Object Lock: Compliance
  Management + Data events                 │   Retention: 366 days
  Insights enabled                         ├── MFA Delete: enabled
         │                                 └── Log validation: enabled
         │
  [CloudWatch Logs]
         │
  ┌──────┼──────────────────────────────┐
  │      │                              │
  ▼      ▼                              ▼
[Alarm: AccessDenied] [Alarm: NoMFA] [Alarm: RootLogin]
  → SNS → SOC Slack    → Lambda         → PagerDuty (P1)
                         (disable user)

[Athena] ← partition by account/date
  → PCI auditor self-service portal
  → Monthly compliance reports

[GuardDuty] ← analyzes same CloudTrail events
  → Automated threat detection
  → Security Hub aggregation
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| Logs in separate security account | Compromised admin in main account cannot delete logs |
| S3 Object Lock (Compliance mode) | Even root cannot delete; satisfies PCI-DSS Req 10.5 |
| Log file validation | Provides cryptographic proof of log integrity for auditors |
| Insights enabled | Anomaly detection catches credential compromise before damage |
| Lambda auto-remediation | Disable compromised IAM user the moment anomaly detected — MTTD < 1 min |

### Interview Narration (Whiteboard Script)
> "For PCI-DSS compliance, I'd set up an Organization Trail across all accounts delivering logs to a dedicated security account that the development team has no access to. S3 Object Lock in Compliance mode ensures no one — not even root — can delete the logs within the retention period. Log file validation gives auditors cryptographic proof the logs haven't been tampered with. I'd stream logs to CloudWatch Logs for real-time alerting: any console login without MFA triggers a Lambda that disables the user immediately. GuardDuty analyzes the same CloudTrail events automatically to catch credential compromise patterns we didn't write explicit rules for."
