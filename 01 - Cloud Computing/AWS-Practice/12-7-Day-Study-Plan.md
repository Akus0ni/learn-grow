# 7-Day AWS Study Plan — $100 Credit Budget

> **Goal**: Master the top 10 AWS services in one week using hands-on labs, interview prep, and architecture practice. Graduate ready to demo a real-world solution and answer deep technical questions.

---

## Credit Budget Allocation

| Resource | Estimated Cost | When |
|---------|---------------|------|
| EC2 t3.medium (40 hrs) | ~$2.00 | Day 1 |
| RDS db.t3.medium Multi-AZ (8 hrs) | ~$1.60 | Day 4 |
| ALB (20 hrs) | ~$0.20 | Day 1, 5 |
| NAT Gateway (4 hrs) | ~$0.20 | Day 3 |
| EKS Cluster (4 hrs) | ~$0.40 | Day 6 |
| ECS Fargate tasks (8 hrs) | ~$0.50 | Day 6 |
| Lambda invocations | <$0.01 | Day 5 |
| S3 storage | <$0.01 | Day 2 |
| CloudWatch (custom metrics) | ~$0.30 | Day 2 |
| VPC, Route 53, API GW | ~$0.50 | All days |
| **Buffer / Mistakes** | **$5.00** | — |
| **Total Estimated** | **~$11** | — |

> 💡 **Always terminate resources immediately after each lab.** Your $100 credit lasts all month. Set a **CloudWatch billing alarm** at $20 on Day 1 to avoid surprises.

---

## Day 1 — EC2 + IAM (Foundation)

### Morning (9 AM – 12 PM): Study
- Read [01-EC2.md](./01-EC2.md) — instance families, AMIs, pricing, Auto Scaling
- Read [06-IAM-Security.md](./06-IAM-Security.md) — users, roles, policies, STS

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 1.1: IAM Setup (30 min)
```bash
# ✅ Lock root account immediately
# ✅ Enable MFA on root (use Google Authenticator)
# ✅ Create IAM admin user (never use root again)
# ✅ Set billing alarm at $20
```
Tasks in Console:
1. Lock root account → enable MFA → download root MFA backup
2. Create IAM user `admin-yourname` with `AdministratorAccess` (temporary)
3. Create IAM user group `developers` with `AmazonEC2FullAccess` + `AmazonS3FullAccess`
4. Create a custom IAM policy: Allow EC2 Describe + Start + Stop, deny Terminate
5. Add user to group; test the deny with CLI

#### Lab 1.2: EC2 + Auto Scaling (90 min)
```bash
# Step 1: Create Launch Template
# Step 2: Create ASG (min:1, max:3, desired:1)
# Step 3: Create ALB + Target Group
# Step 4: Attach ASG to ALB
# Step 5: Test scaling with stress tool

# Install stress tool on EC2 (Amazon Linux 2):
sudo yum install -y stress
stress --cpu 4 --timeout 300   # Trigger CPU alarm

# Watch ASG in console — observe new instance launching
```

**Create these AWS resources:**
- Launch Template: `t3.medium`, Amazon Linux 2, 8 GB gp3
- User Data:
  ```bash
  #!/bin/bash
  yum install -y httpd stress
  echo "Hello from $(hostname)" > /var/www/html/index.html
  systemctl start httpd
  systemctl enable httpd
  ```
- ASG: `min=1, max=3, desired=1`, Target Tracking (CPU 60%)
- ALB: public-facing, HTTP listener
- Security Groups: `alb-sg` (0.0.0.0/0:80), `web-sg` (alb-sg:80)

**Expected outcome:** ALB distributes traffic; stress test triggers scale-out; ASG scales in after stress ends.

#### Lab 1.3: IAM Role for EC2 (30 min)
```bash
# Create IAM role: EC2-S3-ReadOnly
# Attach role to EC2 instance
# Test from EC2 shell (no access keys needed):
aws s3 ls  # Works! Credentials from instance profile
aws s3 mb s3://test-bucket-$(date +%s)  # Should fail (read-only)
```

### Evening (6 PM – 8 PM): Interview Practice
- Answer Q1–Q5 from [01-EC2.md](./01-EC2.md) out loud
- Answer Q1–Q5 from [06-IAM-Security.md](./06-IAM-Security.md) out loud
- **🧹 CLEANUP**: Terminate EC2, delete ASG, ALB, Launch Template

---

## Day 2 — S3 + CloudWatch (Storage & Observability)

### Morning (9 AM – 12 PM): Study
- Read [02-S3.md](./02-S3.md) — storage classes, policies, versioning, replication
- Read [10-CloudWatch-Observability.md](./10-CloudWatch-Observability.md) — metrics, logs, alarms, X-Ray

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 2.1: S3 Deep Dive (2 hrs)
```bash
# Resolve your AWS Account ID once and reuse throughout this lab
ACCT=$(aws sts get-caller-identity --query Account --output text)

# Create standard buckets (raw + processed) with versioning
aws s3api create-bucket \
  --bucket shopwave-raw-$ACCT \
  --region us-east-1
aws s3api create-bucket \
  --bucket shopwave-processed-$ACCT \
  --region us-east-1

# Enable Block Public Access on both buckets
aws s3api put-public-access-block \
  --bucket shopwave-raw-$ACCT \
  --public-access-block-configuration "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"

# Enable versioning
aws s3api put-bucket-versioning \
  --bucket shopwave-raw-$ACCT \
  --versioning-configuration Status=Enabled

# Test versioning:
echo "version 1" | aws s3 cp - s3://shopwave-raw-$ACCT/test.txt
echo "version 2" | aws s3 cp - s3://shopwave-raw-$ACCT/test.txt
aws s3api list-object-versions --bucket shopwave-raw-$ACCT --prefix test.txt

# ⚠️  IMPORTANT: S3 Object Lock MUST be enabled at bucket creation time.
# You cannot enable Object Lock on an existing bucket.
# Create a dedicated compliance/audit bucket with Object Lock enabled from the start:
aws s3api create-bucket \
  --bucket shopwave-audit-$ACCT \
  --region us-east-1 \
  --object-lock-enabled-for-bucket

# Enable versioning on the Object Lock bucket (required for Object Lock to work)
aws s3api put-bucket-versioning \
  --bucket shopwave-audit-$ACCT \
  --versioning-configuration Status=Enabled

# Apply a default Object Lock retention rule (Governance mode, 30 days)
aws s3api put-object-lock-configuration \
  --bucket shopwave-audit-$ACCT \
  --object-lock-configuration '{
    "ObjectLockEnabled": "Enabled",
    "Rule": {
      "DefaultRetention": {
        "Mode": "GOVERNANCE",
        "Days": 30
      }
    }
  }'

# Upload a test object and verify its retention settings
echo "audit record" | aws s3 cp - s3://shopwave-audit-$ACCT/audit.txt
aws s3api get-object-retention --bucket shopwave-audit-$ACCT --key audit.txt

# Create lifecycle policy on raw bucket:
aws s3api put-bucket-lifecycle-configuration \
  --bucket shopwave-raw-$ACCT \
  --lifecycle-configuration file://lifecycle.json

# lifecycle.json:
{
  "Rules": [{
    "ID": "archive-raw-after-30d",
    "Filter": {"Prefix": ""},
    "Status": "Enabled",
    "Transitions": [
      {"Days": 30, "StorageClass": "STANDARD_IA"},
      {"Days": 90, "StorageClass": "GLACIER"}
    ],
    "Expiration": {"Days": 365}
  }]
}

# Generate a pre-signed URL:
aws s3 presign s3://shopwave-raw-$ACCT/test.txt --expires-in 3600
```

> **Key constraint**: S3 Object Lock can only be enabled when a bucket is first created
> (`--object-lock-enabled-for-bucket` flag). It cannot be added to an existing bucket.
> Plan your bucket naming strategy upfront for any compliance use cases.

**Tasks:**
1. Create standard buckets with Block Public Access enabled
2. Create a separate Object Lock bucket at creation time (`--object-lock-enabled-for-bucket`), then configure Governance mode retention (30 days)
3. Set up lifecycle policy on the raw bucket (IA after 30d, Glacier after 90d)
4. Create a bucket policy allowing only a specific IAM role
5. Enable S3 Server Access Logging
6. Generate and test a pre-signed URL

#### Lab 2.2: CloudWatch Dashboard + Alarms (2 hrs)
```bash
# Resolve your AWS Account ID (used in SNS alarm action ARN below)
ACCT=$(aws sts get-caller-identity --query Account --output text)

# Publish a custom metric:
aws cloudwatch put-metric-data \
  --namespace "ShopWave/Orders" \
  --metric-data '[{
    "MetricName": "OrdersPerMinute",
    "Value": 42,
    "Unit": "Count",
    "Dimensions": [{"Name": "Environment", "Value": "dev"}]
  }]'

# Create an alarm:
aws cloudwatch put-metric-alarm \
  --alarm-name "HighOrderVolume" \
  --metric-name "OrdersPerMinute" \
  --namespace "ShopWave/Orders" \
  --threshold 100 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --period 60 \
  --statistic Sum \
  --alarm-actions arn:aws:sns:us-east-1:$ACCT:MyAlarmTopic
```

**Tasks:**
1. Create custom CloudWatch namespace and publish metrics
2. Create a CloudWatch Dashboard with 3 widgets (line chart, number, alarm status)
3. Create a CloudWatch Alarm on the custom metric
4. Create an SNS topic and subscribe your email
5. Manually set alarm to ALARM state and verify email notification
6. Enable CloudWatch Logs for a Lambda function (create a simple test Lambda)

### Evening (6 PM – 8 PM): Interview Practice
- Answer Q1–Q8 from S3 file out loud
- Answer Q1–Q8 from CloudWatch file out loud
- **🧹 CLEANUP**: Delete S3 buckets (empty first), delete alarms, SNS topic

---

## Day 3 — VPC & Networking (Network Architecture)

### Morning (9 AM – 12 PM): Study
- Read [05-VPC-Networking.md](./05-VPC-Networking.md) — VPC, subnets, SGs, NACLs, endpoints

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 3.1: Build a 3-Tier VPC from Scratch (3 hrs)
```bash
# Using CloudFormation for practice (prepare for Day 7!)
# Template: vpc-3tier.yaml

# Manual steps to understand each piece:
# 1. Create VPC: 10.0.0.0/16
# 2. Create Internet Gateway → Attach to VPC
# 3. Create Public Subnets: 10.0.0.0/24 (AZ-a), 10.0.1.0/24 (AZ-b)
# 4. Create Private Subnets: 10.0.10.0/24 (AZ-a), 10.0.11.0/24 (AZ-b)
# 5. Create DB Subnets: 10.0.20.0/24 (AZ-a), 10.0.21.0/24 (AZ-b)
# 6. Create NAT Gateway in public subnet AZ-a
# 7. Create route tables: public (→ IGW), private (→ NAT), DB (local only)
# 8. Associate subnets with route tables
```

**Tasks:**
1. Build the VPC manually in console (learn the flow)
2. Launch EC2 in public subnet (no public IP) — SSH via Session Manager
3. Launch EC2 in private subnet — verify: can reach internet (via NAT), cannot be reached from internet
4. Create VPC Gateway Endpoint for S3 — verify EC2 in private subnet can access S3 without internet
5. Test Security Group rules — open port 8080 from `alb-sg`, verify port 80 is blocked
6. Create a NACL rule to block a specific IP — verify it's blocked (stateless!)

#### Lab 3.2: VPC Flow Logs (30 min)
```bash
# Resolve your AWS Account ID (used in the Flow Logs IAM role ARN below)
ACCT=$(aws sts get-caller-identity --query Account --output text)

# Enable Flow Logs on the VPC:
aws ec2 create-flow-logs \
  --resource-ids vpc-xxx \
  --resource-type VPC \
  --traffic-type ALL \
  --log-destination-type cloud-watch-logs \
  --log-group-name /aws/vpc/flowlogs \
  --deliver-logs-permission-arn arn:aws:iam::$ACCT:role/FlowLogsRole

# Query in CloudWatch Logs Insights:
# fields @timestamp, srcAddr, dstAddr, action
# | filter action = "REJECT"
# | limit 20
```

### Evening (6 PM – 8 PM): Interview Practice
- Answer Q1–Q10 from VPC file out loud — especially the Security Group vs NACL question
- Draw the VPC architecture diagram on paper from memory
- **🧹 CLEANUP**: Delete NAT Gateway (expensive if left running!), delete Flow Logs, delete VPC

---

## Day 4 — RDS / Aurora (Database)

### Morning (9 AM – 12 PM): Study
- Read [04-RDS-Aurora.md](./04-RDS-Aurora.md) — Multi-AZ, Read Replicas, Aurora, Proxy

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 4.1: RDS MySQL with Multi-AZ (2 hrs)
```bash
# Create RDS MySQL in the VPC from Day 3 (or create a new simple VPC)
# Use console for visual learning:

# Settings:
# Engine: MySQL 8.0
# Template: Dev/Test (to reduce cost)
# DB identifier: shopwave-dev
# Instance: db.t3.micro (Free Tier eligible)
# Storage: 20 GB gp2 (not Multi-AZ for cost, but understand the concept)
# VPC: use your VPC
# Subnet Group: DB subnets
# Public access: No
# Security Group: Allow port 3306 from EC2 SG

# Connect from EC2 in private subnet:
mysql -h your-rds-endpoint -u admin -p

# Test queries:
CREATE DATABASE shopwave;
USE shopwave;
CREATE TABLE orders (id INT AUTO_INCREMENT PRIMARY KEY, amount DECIMAL(10,2), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
INSERT INTO orders (amount) VALUES (99.99), (149.50), (25.00);
SELECT * FROM orders;
```

#### Lab 4.2: RDS Proxy (1 hr)
```bash
# Create RDS Proxy in front of your RDS instance:
# (Requires Secrets Manager secret for credentials)

# Create secret:
aws secretsmanager create-secret \
  --name shopwave/rds/admin \
  --generate-secret-string '{"SecretStringTemplate":"{\"username\":\"admin\"}","GenerateStringKey":"password","PasswordLength":32,"ExcludePunctuation":true}'

# Create RDS Proxy (Console):
# - Target: your RDS instance
# - Credentials: Secrets Manager secret above
# - IAM auth: enabled

# Test: Connect through Proxy endpoint (same MySQL client)
# Notice: Proxy reuses connections; your app doesn't need connection pool code
```

#### Lab 4.3: Aurora Serverless v2 (1 hr)
```bash
# Create Aurora Serverless v2 (MySQL-compatible)
# Settings:
# Engine: Aurora MySQL 8.0
# Capacity: Min 0.5 ACU, Max 8 ACU
# Enable: Data API (for HTTP access without VPC)

# Query via Data API (no connection needed!):
aws rds-data execute-statement \
  --resource-arn arn:aws:rds:... \
  --secret-arn arn:aws:secretsmanager:... \
  --database shopwave \
  --sql "SELECT COUNT(*) FROM orders"
```

### Evening (6 PM – 8 PM): Interview Practice
- Answer Q1–Q10 from RDS-Aurora file
- Draw the Aurora Global Database architecture from memory
- **🧹 CLEANUP**: Delete RDS instances, RDS Proxy, Secrets (Secrets Manager costs $0.40/secret/month)

---

## Day 5 — Lambda + API Gateway (Serverless)

### Morning (9 AM – 12 PM): Study
- Read [03-Lambda.md](./03-Lambda.md) — cold starts, concurrency, triggers
- Read [08-API-Gateway.md](./08-API-Gateway.md) — REST vs HTTP, auth, throttling

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 5.1: Build a Serverless REST API (3 hrs)
Build a complete Orders API:

```python
# lambda/order_handler.py
import json
import boto3
import os
import uuid
from datetime import datetime

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table(os.environ['ORDERS_TABLE'])

def handler(event, context):
    method = event['httpMethod']
    path = event['path']
    
    if method == 'POST' and path == '/orders':
        body = json.loads(event['body'])
        order_id = str(uuid.uuid4())
        
        table.put_item(Item={
            'orderId': order_id,
            'amount': str(body['amount']),
            'status': 'pending',
            'createdAt': datetime.utcnow().isoformat()
        })
        
        return {
            'statusCode': 201,
            'body': json.dumps({'orderId': order_id})
        }
    
    elif method == 'GET' and path.startswith('/orders/'):
        order_id = path.split('/')[-1]
        response = table.get_item(Key={'orderId': order_id})
        
        if 'Item' not in response:
            return {'statusCode': 404, 'body': json.dumps({'error': 'Not found'})}
        
        return {
            'statusCode': 200,
            'body': json.dumps(response['Item'])
        }
    
    return {'statusCode': 400, 'body': json.dumps({'error': 'Unknown route'})}
```

**Full Lab Steps:**
1. Create DynamoDB table: `orders` (PK: `orderId`)
2. Create Lambda function with IAM role (DynamoDB read/write + CloudWatch Logs)
3. Set environment variable: `ORDERS_TABLE=orders`
4. Create API Gateway REST API with Lambda Proxy integration
5. Create resources: `POST /orders`, `GET /orders/{orderId}`
6. Deploy to `dev` stage
7. Test with curl:
   ```bash
   API_URL="https://xxx.execute-api.us-east-1.amazonaws.com/dev"
   
   # Create order
   curl -X POST $API_URL/orders \
     -H "Content-Type: application/json" \
     -d '{"amount": 99.99}'
   
   # Get order
   curl $API_URL/orders/{order-id-from-above}
   ```
8. Enable X-Ray on Lambda + API Gateway
9. View X-Ray Service Map and traces in console

#### Lab 5.2: Lambda Cold Start Experiment (1 hr)
```bash
# Deploy Lambda with Python (fast) and Java (slow) and compare cold starts

# Invoke a cold Lambda (after 15 min idle):
aws lambda invoke --function-name order-handler output.json
# Check Init Duration in CloudWatch Logs

# Enable Provisioned Concurrency on Python function:
aws lambda put-provisioned-concurrency-config \
  --function-name order-handler \
  --qualifier prod \
  --provisioned-concurrent-executions 5

# Invoke again — Init Duration should be 0ms
```

### Evening (6 PM – 8 PM): Interview Practice
- Answer all 15 Q&As from Lambda file
- Answer Q1–Q8 from API Gateway file
- **🧹 CLEANUP**: Delete Lambda, API Gateway, DynamoDB table, Provisioned Concurrency

---

## Day 6 — ECS / EKS (Containers)

### Morning (9 AM – 12 PM): Study
- Read [07-ECS-EKS.md](./07-ECS-EKS.md) — Fargate, task definitions, K8s fundamentals

### Afternoon (1 PM – 5 PM): Hands-On Lab

#### Lab 6.1: ECS Fargate Service (2 hrs)
```bash
# Build and push Docker image:
cat > Dockerfile <<EOF
FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install flask
COPY app.py .
EXPOSE 8080
CMD ["python", "app.py"]
EOF

cat > app.py <<EOF
from flask import Flask
app = Flask(__name__)

@app.route('/health')
def health():
    return {'status': 'healthy'}

@app.route('/')
def home():
    return {'message': 'ShopWave API on Fargate!'}

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8080)
EOF

# Set AWS Account ID used in ECR URLs
ACCT=$(aws sts get-caller-identity --query Account --output text)
# Set AWS Account ID used in ECR URLs
ACCT=$(aws sts get-caller-identity --query Account --output text)

# Authenticate to ECR:
aws ecr get-login-password | docker login --username AWS \
  --password-stdin $ACCT.dkr.ecr.us-east-1.amazonaws.com

# Create ECR repo + build + push:
aws ecr create-repository --repository-name shopwave-api
docker build -t shopwave-api .
docker tag shopwave-api:latest $ACCT.dkr.ecr.us-east-1.amazonaws.com/shopwave-api:latest
docker push $ACCT.dkr.ecr.us-east-1.amazonaws.com/shopwave-api:latest

# Create ECS Cluster:
aws ecs create-cluster --cluster-name shopwave-cluster --capacity-providers FARGATE

# Register Task Definition:
aws ecs register-task-definition --cli-input-json file://task-def.json

# Create Service with ALB:
aws ecs create-service \
  --cluster shopwave-cluster \
  --service-name shopwave-api \
  --task-definition shopwave-api:1 \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx,subnet-yyy],securityGroups=[sg-xxx],assignPublicIp=ENABLED}" \
  --load-balancers "targetGroupArn=arn:...,containerName=shopwave-api,containerPort=8080"
```

#### Lab 6.2: EKS Quick Start (2 hrs)
```bash
# Resolve your AWS Account ID (used in ECR image URIs below)
ACCT=$(aws sts get-caller-identity --query Account --output text)

# Install eksctl (AWS CLI for EKS)
brew install eksctl  # or see https://eksctl.io

# Create a minimal EKS cluster:
eksctl create cluster \
  --name shopwave-eks \
  --region us-east-1 \
  --nodegroup-name general \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-min 1 \
  --nodes-max 3

# Deploy the same Docker image:
kubectl create deployment shopwave --image=$ACCT.dkr.ecr.us-east-1.amazonaws.com/shopwave-api:latest
kubectl expose deployment shopwave --port=80 --target-port=8080 --type=LoadBalancer
kubectl get svc  # Wait for EXTERNAL-IP

# Scale:
kubectl scale deployment shopwave --replicas=3
kubectl get pods

# View logs:
kubectl logs -l app=shopwave --tail=50

# ⚠️ EKS cluster costs $0.10/hr — delete after lab!
eksctl delete cluster --name shopwave-eks
```

### Evening (6 PM – 8 PM): Interview Practice
- Answer all 15 Q&As from ECS/EKS file
- Compare ECS vs EKS — practice the decision framework out loud
- **🧹 CLEANUP**: Delete ECS service, cluster, ALB, ECR images; delete EKS cluster

---

## Day 7 — CloudFormation / CDK + Mock Interview + Review

### Morning (9 AM – 12 PM): Study + IaC Lab

#### Lab 7.1: Deploy with CloudFormation (2 hrs)
```bash
# Deploy the 3-tier stack from Day 3 using CloudFormation:
aws cloudformation create-stack \
  --stack-name shopwave-dev \
  --template-body file://infrastructure/vpc.yaml \
  --parameters ParameterKey=Environment,ParameterValue=dev \
  --capabilities CAPABILITY_IAM

# Watch progress:
aws cloudformation describe-stack-events \
  --stack-name shopwave-dev \
  --query 'StackEvents[?ResourceStatus==`CREATE_FAILED`]'

# Create a change set (modify instance type):
aws cloudformation create-change-set \
  --stack-name shopwave-dev \
  --change-set-name update-instance-type \
  --template-body file://infrastructure/vpc-v2.yaml

# Review change set before executing:
aws cloudformation describe-change-set \
  --stack-name shopwave-dev \
  --change-set-name update-instance-type

# Execute:
aws cloudformation execute-change-set \
  --stack-name shopwave-dev \
  --change-set-name update-instance-type

# Clean up:
aws cloudformation delete-stack --stack-name shopwave-dev
```

### Afternoon (1 PM – 5 PM): Mock Interview

#### Mock Interview Q&A Session (2 hrs)
Pick 5 random questions from each service file and answer them out loud as if in an interview. Time yourself — 2 minutes per question.

**Scoring Criteria:**
- [ ] Answered correctly (technical accuracy)
- [ ] Explained the "why" (not just "what")
- [ ] Gave a real-world example or use case
- [ ] Mentioned trade-offs or when NOT to use this approach

#### Whiteboard Architecture Practice (1 hr)
On paper or a whiteboard, draw the capstone architecture from memory:
1. Edge layer (Route 53, WAF, CloudFront)
2. API layer (API Gateway, Lambda, ECS)
3. Async layer (SQS, Lambda processors)
4. Data layer (Aurora, DynamoDB, S3, ElastiCache)
5. Observability (CloudWatch, X-Ray)
6. Security (IAM, VPC, CloudTrail)

Then narrate it as if presenting to an interviewer. Target: 10 minutes.

### Evening (5 PM – 8 PM): Gaps & Review

#### Final Review Checklist
Go through each topic and mark confidence:

| Service | Concepts | Interview Q&A | Architecture |
|---------|---------|--------------|--------------|
| EC2 | ⬜ | ⬜ | ⬜ |
| S3 | ⬜ | ⬜ | ⬜ |
| Lambda | ⬜ | ⬜ | ⬜ |
| RDS/Aurora | ⬜ | ⬜ | ⬜ |
| VPC | ⬜ | ⬜ | ⬜ |
| IAM | ⬜ | ⬜ | ⬜ |
| ECS/EKS | ⬜ | ⬜ | ⬜ |
| API Gateway | ⬜ | ⬜ | ⬜ |
| CloudFormation/CDK | ⬜ | ⬜ | ⬜ |
| CloudWatch | ⬜ | ⬜ | ⬜ |
| **Capstone Architecture** | ⬜ | ⬜ | ⬜ |

Re-read any files where you marked less than confident. Focus on Q&As for the interview the next morning.

---

## Interview Day Checklist

### Before the Interview
- [ ] Review the capstone architecture diagram one more time
- [ ] Practice the 15-minute whiteboard narration out loud
- [ ] Have your AWS console open with a practice environment to reference

### Key Phrases to Use
| Situation | Phrase |
|-----------|--------|
| High availability | "I'd deploy across at least 2 AZs with an ALB for automatic failover" |
| Security | "Following least-privilege principle, I'd use an IAM role with only the specific permissions needed" |
| Cost optimization | "For the baseline I'd use Reserved Instances, and burst on Spot for cost-sensitive batch" |
| Scalability | "The architecture scales horizontally — Lambda scales automatically; for ECS I'd set target tracking on ALB request count per target" |
| Observability | "I'd instrument with X-Ray for tracing, CloudWatch for metrics and alarms, and ship logs to S3 for long-term retention" |
| Trade-offs | "The trade-off here is cost vs latency — Provisioned Concurrency eliminates cold starts but costs even when idle" |

### If You Don't Know the Answer
> "That's a great question. My first instinct is [X]. I'd want to verify [Y] in the documentation because the behavior might differ in [specific scenario]. Have you seen this come up in your environment?"

---

## Quick Reference: Limits to Memorize

| Service | Key Limit |
|---------|---------|
| Lambda timeout | 15 minutes |
| Lambda memory | 10 GB |
| Lambda payload (sync) | 6 MB |
| API Gateway timeout | 29 seconds |
| API Gateway payload | 10 MB |
| S3 object size | 5 TB |
| S3 multipart threshold | > 100 MB recommended, required > 5 GB |
| EC2 Spot interruption notice | 2 minutes |
| RDS backup retention | 0–35 days |
| Aurora max storage | 128 TB |
| DynamoDB item size | 400 KB |
| SQS max message size | 256 KB |
| SQS visibility timeout | 0–12 hours |
| CloudWatch metric retention | 15 months |
| CloudTrail default storage | S3 (indefinite until you delete) |

---

## Recommended Next Steps After the Week

1. **AWS Certified Solutions Architect – Associate (SAA-C03)**: 2–3 more weeks of study; this week gives you 70% of the content
2. **AWS Certified Developer – Associate (DVA-C02)**: Focus on Lambda, DynamoDB, X-Ray, CodePipeline
3. **Build a portfolio project**: Deploy the capstone architecture with real data (a simple personal project)
4. **Contribute to open source**: Find AWS CDK constructs or Terraform AWS modules to contribute to

---

> 🎯 **Final Tip**: In the interview, always connect your answer to business outcomes: availability, cost, security, and speed-to-market. Technical correctness wins points; connecting it to business value wins the job.
