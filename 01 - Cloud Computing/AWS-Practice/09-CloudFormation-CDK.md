# AWS CloudFormation & CDK — Infrastructure as Code

## 1. Core Concepts

### What is Infrastructure as Code (IaC)?
IaC treats infrastructure provisioning as software — defined in code, version-controlled, tested, and deployed through CI/CD pipelines. Benefits:
- **Reproducibility**: Same code → identical environments every time
- **Version control**: Track changes, roll back, audit who changed what
- **Automation**: No manual console clicks in production
- **Drift detection**: Know when real infrastructure diverges from declared state

### AWS CloudFormation
CloudFormation is AWS's **declarative IaC** service. You write **templates** (JSON or YAML) describing the desired state; CloudFormation figures out the create/update/delete order.

### AWS CDK (Cloud Development Kit)
CDK is a framework that lets you define AWS infrastructure using **real programming languages** (TypeScript, Python, Java, Go, .NET). CDK synthesizes CloudFormation templates — it's CloudFormation under the hood.

---

## 2. CloudFormation Deep Dive

### Template Structure
```yaml
AWSTemplateFormatVersion: "2010-09-09"
Description: "Web Application Stack"

Parameters:
  Environment:
    Type: String
    AllowedValues: [dev, staging, prod]
    Default: dev
  InstanceType:
    Type: String
    Default: t3.medium

Mappings:
  RegionAMI:
    us-east-1:
      AMI: ami-0abcdef1234567890
    us-west-2:
      AMI: ami-0fedcba9876543210

Conditions:
  IsProduction: !Equals [!Ref Environment, prod]

Resources:
  MyBucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub "my-app-${Environment}-${AWS::AccountId}"
      VersioningConfiguration:
        Status: !If [IsProduction, Enabled, Suspended]

  MyEC2Instance:
    Type: AWS::EC2::Instance
    Properties:
      ImageId: !FindInMap [RegionAMI, !Ref AWS::Region, AMI]
      InstanceType: !Ref InstanceType

Outputs:
  BucketName:
    Value: !Ref MyBucket
    Export:
      Name: !Sub "${AWS::StackName}-BucketName"
```

### Key Template Sections
| Section | Required | Purpose |
|---------|---------|---------|
| `AWSTemplateFormatVersion` | No | Template format version (always `2010-09-09`) |
| `Description` | No | Human-readable description |
| `Parameters` | No | Inputs at deploy time |
| `Mappings` | No | Lookup tables (region → AMI) |
| `Conditions` | No | Conditional resource creation |
| `Resources` | **Yes** | AWS resources to create |
| `Outputs` | No | Values to export to other stacks |
| `Metadata` | No | Additional template info |
| `Transform` | No | SAM transform, macros |

### Intrinsic Functions (Must Know)
| Function | Short Form | Purpose |
|----------|-----------|---------|
| `Fn::Ref` | `!Ref` | Reference parameter or resource |
| `Fn::Sub` | `!Sub` | String substitution with variables |
| `Fn::GetAtt` | `!GetAtt` | Get attribute from a resource |
| `Fn::Join` | `!Join` | Join string array |
| `Fn::Select` | `!Select` | Select item from list |
| `Fn::If` | `!If` | Conditional value |
| `Fn::Equals` | `!Equals` | Comparison for conditions |
| `Fn::FindInMap` | `!FindInMap` | Lookup in Mappings |
| `Fn::ImportValue` | `!ImportValue` | Import exported value from another stack |
| `Fn::Base64` | `!Base64` | Encode string (EC2 UserData) |

### Stacks & Stack Sets
- **Stack**: Deployed instance of a template in one region/account
- **Nested Stack**: A stack that creates another stack (`AWS::CloudFormation::Stack`) — reuse common patterns
- **StackSet**: Deploy the same stack to multiple accounts and regions simultaneously via AWS Organizations
- **Change Set**: Preview changes before applying — shows what will be created/modified/deleted

### Deletion Policies
```yaml
Resources:
  MyDatabase:
    Type: AWS::RDS::DBInstance
    DeletionPolicy: Snapshot    # Take snapshot before deleting
    UpdateReplacePolicy: Retain # Keep old resource if replaced
```
| Policy | Behavior |
|--------|---------|
| `Delete` | Default — resource is deleted with stack |
| `Retain` | Resource stays after stack deletion |
| `Snapshot` | Take snapshot before deletion (RDS, EBS, ElastiCache) |

### CloudFormation Drift Detection
Detects when deployed resources differ from the template (manual console changes). Run drift detection → see which resources drifted and what changed. Helps enforce IaC discipline.

### CloudFormation Registry & Modules
- **Resource types**: Register third-party or custom resources
- **Modules**: Reusable template fragments you publish to the registry

### SAM (Serverless Application Model)
SAM is a CloudFormation extension (`Transform: AWS::Serverless-2016-10-31`) that adds simplified types for serverless resources:
```yaml
Transform: AWS::Serverless-2016-10-31
Resources:
  MyFunction:
    Type: AWS::Serverless::Function  # Expands to Lambda + IAM Role + Event source
    Properties:
      Handler: index.handler
      Runtime: python3.12
      Events:
        Api:
          Type: Api
          Properties:
            Path: /orders
            Method: POST
```

---

## 3. AWS CDK Deep Dive

### Why CDK over CloudFormation YAML?
- Use **loops, conditionals, abstractions** — no repetitive YAML
- **Type safety** — IDE autocomplete catches typos at compile time
- **Unit tests** — assert your infrastructure with Jest, pytest
- **Reusable Constructs** — share infrastructure patterns as npm/PyPI packages
- Synthesizes to CloudFormation — full CloudFormation compatibility

### CDK Concepts
| Term | Equivalent | Description |
|------|-----------|-------------|
| **App** | — | Root of the CDK app; synthesizes all stacks |
| **Stack** | CloudFormation Stack | Unit of deployment |
| **Construct** | Resource or group | Building block; can compose constructs |
| **L1 Construct** (Cfn*) | Raw CloudFormation | 1-to-1 with CF; e.g., `CfnBucket` |
| **L2 Construct** | Higher-level | Intent-based; e.g., `Bucket` with sensible defaults |
| **L3 Construct** (Patterns) | Multiple resources | Common patterns; e.g., `ApplicationLoadBalancedFargateService` |

### CDK Example (TypeScript)
```typescript
import * as cdk from 'aws-cdk-lib';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as apigw from 'aws-cdk-lib/aws-apigateway';

export class OrderApiStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // S3 bucket with versioning and lifecycle
    const ordersBucket = new s3.Bucket(this, 'OrdersBucket', {
      versioned: true,
      encryption: s3.BucketEncryption.KMS_MANAGED,
      removalPolicy: cdk.RemovalPolicy.RETAIN,
      lifecycleRules: [{
        expiration: cdk.Duration.days(365),
        transitions: [{
          storageClass: s3.StorageClass.GLACIER,
          transitionAfter: cdk.Duration.days(90),
        }],
      }],
    });

    // Lambda function
    const orderHandler = new lambda.Function(this, 'OrderHandler', {
      runtime: lambda.Runtime.PYTHON_3_12,
      handler: 'index.handler',
      code: lambda.Code.fromAsset('lambda/order'),
      environment: {
        BUCKET_NAME: ordersBucket.bucketName,
      },
    });

    // Grant Lambda read/write on bucket
    ordersBucket.grantReadWrite(orderHandler);

    // API Gateway REST API
    const api = new apigw.LambdaRestApi(this, 'OrderApi', {
      handler: orderHandler,
      deployOptions: {
        stageName: 'prod',
        cachingEnabled: true,
        cacheTtl: cdk.Duration.seconds(60),
      },
    });

    new cdk.CfnOutput(this, 'ApiUrl', { value: api.url });
  }
}
```

### CDK Useful Commands
```bash
cdk init app --language typescript  # Bootstrap new CDK app
cdk synth                           # Synthesize CloudFormation template
cdk diff                            # Preview changes (like change set)
cdk deploy                          # Deploy stack
cdk destroy                         # Delete stack
cdk bootstrap                       # One-time account/region setup (CDKToolkit stack)
cdk ls                              # List all stacks
```

### CDK Testing (Important for Interviews)
```typescript
import { Template } from 'aws-cdk-lib/assertions';

test('S3 bucket has versioning', () => {
  const app = new cdk.App();
  const stack = new OrderApiStack(app, 'TestStack');
  const template = Template.fromStack(stack);
  
  template.hasResourceProperties('AWS::S3::Bucket', {
    VersioningConfiguration: { Status: 'Enabled' }
  });
});

test('Lambda has correct runtime', () => {
  const template = Template.fromStack(new OrderApiStack(new cdk.App(), 'TestStack'));
  template.hasResourceProperties('AWS::Lambda::Function', {
    Runtime: 'python3.12'
  });
});
```

---

## 4. Interview Questions & Answers

### Q1. What is the difference between CloudFormation and CDK?
**CloudFormation**: Declarative JSON/YAML templates. No programming constructs — describe resources directly. Lower entry barrier, but verbose and repetitive for complex setups.

**CDK**: Use TypeScript, Python, Java, Go, .NET to define infrastructure. CDK synthesizes to CloudFormation under the hood. You get loops, conditions, type safety, unit tests, and reusable constructs. Higher initial learning but dramatically more productive for complex infrastructure.

Both deploy via CloudFormation — CDK is not a separate deployment engine.

---

### Q2. What is a CloudFormation Change Set?
A Change Set is a preview of the changes CloudFormation will make before you apply them. It shows:
- What resources will be Created / Modified / Deleted / Replaced
- Whether a replacement will cause downtime (e.g., changing an RDS engine type)

Best practice: Always create and review a Change Set before updating production stacks.

---

### Q3. What is CloudFormation StackSets?
StackSets extend CloudFormation to deploy the same stack to **multiple accounts and regions** simultaneously. Integration with AWS Organizations allows deploying to all accounts in an OU automatically (auto-deployment when new accounts join). Use cases:
- Deploy security baseline (CloudTrail, GuardDuty, Config) to all accounts
- Deploy shared VPC network components
- Deploy tagging/compliance resources

---

### Q4. How do you handle sensitive parameters (passwords) in CloudFormation?
Options:
1. **Parameter type `AWS::SSM::Parameter::Value<String>`**: Reference an SSM SecureString parameter — CloudFormation resolves it at deploy time
2. **`NoEcho: true`** on parameters: Masks the value in outputs and events, but it's still passed in plain text in the template call
3. **Reference Secrets Manager ARN**: Pass the ARN as a parameter, application retrieves the secret at runtime (not in template)
4. **CDK + Secrets Manager**: `secretsmanager.Secret.fromSecretNameV2()` — CDK generates a CloudFormation dynamic reference `{{resolve:secretsmanager:...}}`

**Best practice**: Never put secrets in template body. Use dynamic references or pass ARNs.

---

### Q5. What is CloudFormation Drift Detection?
Drift occurs when real-world resources diverge from the CloudFormation template (e.g., someone manually changed an S3 bucket policy in the console). 

Drift Detection:
1. Run drift detection on a stack or specific resources
2. CloudFormation compares actual resource configuration with the template
3. Returns status: `IN_SYNC` or `DRIFTED` with specific property diffs

Use case: Enforce IaC discipline — alert on drift, apply runbooks to remediate via IaC, never via console in prod.

---

### Q6. What are the three levels of CDK Constructs?
- **L1 (Low-level / Cfn*)**: One-to-one mapping with CloudFormation resources. Every CloudFormation resource type is available as an L1. Full control, no defaults. Prefixed with `Cfn`: `CfnBucket`, `CfnFunction`.

- **L2 (High-level)**: Intent-based with sensible defaults and helper methods. `Bucket`, `Function`, `RestApi`. `.grantRead()`, `.addEventSource()` — convenient abstractions.

- **L3 (Patterns)**: Multiple AWS resources combined into a common pattern. `ApplicationLoadBalancedFargateService` creates an ALB + ECS cluster + Fargate service + security groups. One line for a full pattern.

---

### Q7. How does the CDK bootstrap process work?
Before you can use CDK in an account/region, you run `cdk bootstrap`. This creates a CloudFormation stack called `CDKToolkit` that provisions:
- An **S3 bucket** for storing CDK assets (Lambda zips, Docker images, CloudFormation templates)
- An **ECR repository** for container image assets
- IAM roles for CDK deployments (can be customized for CI/CD cross-account access)

Run once per account/region. In multi-account setups, bootstrap each target account with a trust policy pointing to the CI/CD account.

---

### Q8. What is `DeletionPolicy: Retain` and when would you use it?
When you delete a CloudFormation stack, by default AWS deletes all resources. `DeletionPolicy: Retain` tells CloudFormation to keep the resource even when the stack is deleted.

Use cases:
- **S3 buckets**: Can't delete non-empty buckets; retain to avoid data loss
- **RDS databases**: Keep production data even if stack is accidentally deleted
- **DynamoDB tables**: Retain critical data

Also set `UpdateReplacePolicy: Retain` to keep the old resource if CloudFormation needs to replace it (e.g., changing a property that requires replacement).

---

### Q9. How do you share outputs between CloudFormation stacks?
**Stack Outputs + Cross-Stack References**:
1. In the **exporting stack**, define an Output with `Export.Name`
2. In the **importing stack**, use `!ImportValue ExportedName`

```yaml
# Stack A - exports
Outputs:
  VpcId:
    Value: !Ref MyVPC
    Export:
      Name: NetworkStack-VpcId

# Stack B - imports
Resources:
  MySubnet:
    Properties:
      VpcId: !ImportValue NetworkStack-VpcId
```

**Limitation**: Can't delete the exporting stack while importing stacks exist. For loose coupling, use SSM Parameter Store instead.

---

### Q10. What is SAM (Serverless Application Model)?
SAM is a CloudFormation extension (via `Transform`) that adds higher-level resource types for serverless:
- `AWS::Serverless::Function` → Lambda + IAM Role + EventSourceMappings
- `AWS::Serverless::Api` → API Gateway + deployment
- `AWS::Serverless::SimpleTable` → DynamoDB table

SAM also provides a CLI (`sam local invoke`, `sam local start-api`) for local Lambda testing and a build/deploy pipeline.

---

### Q11. How do you handle multiple environments (dev/staging/prod) with CloudFormation?
Three approaches:
1. **Parameters**: Single template, different parameter values per environment. Simple but less isolation.
2. **Separate stacks per environment**: `stack-dev`, `stack-staging`, `stack-prod`. Deploy the same template with different parameters. Track via CI/CD.
3. **CDK Environments**: `new MyStack(app, 'ProdStack', { env: { account: '123', region: 'us-east-1' } })`. Different CDK apps or conditions per environment.

Best practice: Separate stacks per environment (separate AWS accounts) with the same template for true isolation.

---

### Q12. What is `cfn-init` and `cfn-signal`?
`cfn-init`: CloudFormation helper script on EC2 that reads metadata from the template to install packages, create files, and start services.

`cfn-signal`: EC2 sends a signal back to CloudFormation's `WaitCondition` to indicate successful initialization. CloudFormation waits until the signal is received (or times out) before marking the stack as `CREATE_COMPLETE`.

Use case: Ensure EC2 UserData bootstrap completes before CloudFormation moves to dependent resources.

---

### Q13. What is AWS CDK Aspects?
CDK Aspects traverse the entire construct tree and apply changes to all matching constructs. Use case: apply a tag to every resource in a stack, enforce S3 bucket encryption on every bucket, or add a DeletionPolicy to all databases.

```typescript
class AddTagsAspect implements cdk.IAspect {
  visit(node: cdk.IConstruct): void {
    if (node instanceof cdk.CfnResource) {
      cdk.Tags.of(node).add('ManagedBy', 'CDK');
    }
  }
}

cdk.Aspects.of(app).add(new AddTagsAspect());
```

---

### Q14. What causes a CloudFormation stack to roll back?
CloudFormation rolls back when resource creation/update fails:
- Resource provisioning errors (e.g., incorrect AMI ID)
- IAM permission errors
- Stack policy violations
- Timeout exceeded

Rollback behavior:
- **CREATE**: All resources deleted on failure
- **UPDATE**: Resources reverted to previous state
- `DisableRollback: true` can be set to keep failed state for debugging

---

### Q15. How do you integrate CloudFormation/CDK with a CI/CD pipeline?
```
GitHub → CodePipeline → CodeBuild → CloudFormation

CodeBuild stages:
  1. Unit tests (CDK: jest/pytest)
  2. cdk synth / cfn-lint (validate template)
  3. cdk diff / changeset (preview changes)
  4. Manual approval gate (for prod)
  5. cdk deploy / execute-change-set

IAM Role for CI/CD:
  - Assume role in each target account (cross-account)
  - Role permissions: CloudFormation, S3 (CDK bucket), ECR (CDK images)
  - Permission Boundary limits blast radius
```

---

## 5. Real-World Use Case: Full IaC Pipeline with CDK

### Scenario
A team needs to:
- Define an entire application stack (VPC, ECS, RDS, ALB, CloudWatch) as code
- Support three environments: dev, staging, prod
- Block all manual console changes to prod
- Peer-review all infrastructure changes via pull requests
- Auto-deploy to dev on merge, require approval for prod

### Pipeline Architecture

```
GitHub Repository
  infrastructure/
    bin/app.ts         ← CDK App entry point
    lib/
      network-stack.ts ← VPC, subnets, SGs
      database-stack.ts← Aurora, RDS Proxy
      app-stack.ts     ← ECS, ALB, CloudWatch
      pipeline-stack.ts← CodePipeline definition
    test/
      network.test.ts  ← CDK assertions
      database.test.ts

Pull Request → GitHub Actions:
  1. npm run test (CDK unit tests)
  2. cdk synth (validate)
  3. cfn-nag (security linting)
  4. cdk diff dev (show what will change in dev)
  ← PR review required

Merge to main → CodePipeline:
  Source: GitHub → S3
  │
  Build: CodeBuild
    npm ci
    npm run test
    cdk synth
    cdk diff staging
  │
  Deploy Dev: cdk deploy dev-NetworkStack dev-AppStack --require-approval never
  │
  Integration Tests: CodeBuild (Postman/Newman API tests)
  │
  Deploy Staging: cdk deploy staging-*
  │
  ← Manual approval (Team Lead)
  │
  Deploy Prod: cdk deploy prod-* (change set + approve)

AWS Config Rule: cloudformation-stack-drift-detection-check
  → Alert if anyone makes manual changes to prod resources
  → Auto-remediate via Lambda: reapply CloudFormation template
```

### CDK Stack Composition

```typescript
// bin/app.ts
const app = new cdk.App();
const env = { account: process.env.CDK_ACCOUNT, region: 'us-east-1' };

const network = new NetworkStack(app, 'prod-NetworkStack', { env });
const db = new DatabaseStack(app, 'prod-DatabaseStack', {
  env,
  vpc: network.vpc,
});
const application = new AppStack(app, 'prod-AppStack', {
  env,
  vpc: network.vpc,
  cluster: db.cluster,
});

// Tag everything in prod
cdk.Tags.of(app).add('Environment', 'prod');
cdk.Tags.of(app).add('ManagedBy', 'CDK');
cdk.Tags.of(app).add('CostCenter', 'engineering');
```

### Interview Narration (White-board Script)
> "For IaC I'd use CDK with TypeScript because type safety catches mistakes at compile time and we can write unit tests for the infrastructure. The stack is split into Network, Database, and App stacks — this lets us update the app layer without touching the network. Every pull request runs `cdk synth` and `cdk diff` so reviewers see exactly what infrastructure will change, not just what YAML changed. After merge, CodePipeline auto-deploys to dev, runs integration tests, deploys to staging, and then requires a manual approval for prod. AWS Config drift detection alerts us if anyone makes manual console changes to production, and a Lambda remediation function reapplies the CloudFormation template automatically."
