# AWS IAM — Identity & Access Management

## 1. Core Concepts

### What is IAM?
IAM is AWS's centralized **authentication and authorization** service. It controls **who** (authentication) can do **what** (authorization) on **which** AWS resources.

IAM is **global** — not region-specific. Entities created in IAM are available in all regions.

### Core IAM Entities

| Entity | What It Is | Use Case |
|--------|-----------|---------|
| **Root Account** | Account owner with full access | Lock away after creating admin IAM user — never use for daily tasks |
| **IAM User** | Long-term identity for a human or application | Human users, legacy apps that need access keys |
| **IAM Group** | Collection of users sharing the same policies | Dev team, Ops team, Readonly users |
| **IAM Role** | Temporary identity assumed by users, services, or accounts | EC2 instance role, Lambda execution role, cross-account access |
| **IAM Policy** | JSON document defining allowed/denied actions | Attached to users, groups, or roles |

### IAM Policies
Policies are **JSON documents** with Statements. Each Statement has:
- **Effect**: `Allow` or `Deny`
- **Action**: AWS API action(s) (`s3:GetObject`, `ec2:*`)
- **Resource**: ARN(s) the action applies to
- **Condition**: Optional — restricts when the policy applies

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:GetObject", "s3:PutObject"],
      "Resource": "arn:aws:s3:::my-bucket/*",
      "Condition": {
        "StringEquals": {"s3:prefix": ["uploads/"]}
      }
    }
  ]
}
```

### Policy Types
| Type | Attachment | Description |
|------|-----------|-------------|
| **AWS Managed** | Users/Groups/Roles | AWS maintains; e.g., `AmazonS3ReadOnlyAccess` |
| **Customer Managed** | Users/Groups/Roles | You create and control |
| **Inline** | Single user/group/role | Embedded directly; one-to-one relationship |
| **Resource-based** | Resource (S3, SQS, Lambda) | Attached to resource; controls who can access it |
| **Permission Boundary** | User/Role | Maximum permissions ceiling — even if policy grants more, boundary limits it |
| **SCPs (Service Control Policies)** | AWS Organizations OU/Account | Maximum permissions for entire accounts/OUs |
| **Session Policies** | Assumed role session | Limit permissions for a specific role session |

### Policy Evaluation Logic (Critical for Exams)
1. **Explicit DENY** anywhere → **DENY** (always wins)
2. **No ALLOW** → **DENY** (implicit deny)
3. **ALLOW** present in at least one applicable policy → **ALLOW**
4. Permission Boundaries and SCPs constrain the effective permissions
5. Trust policy on a role must allow the principal to assume it

### IAM Roles
Roles provide **temporary security credentials** via AWS STS (Security Token Service):
- No long-term access keys stored anywhere
- Credentials expire (15 min – 36 hours, configurable)
- **Trust Policy**: Defines who can assume the role (`sts:AssumeRole`)
- **Permission Policy**: Defines what the role can do

Common role patterns:
- **EC2 Instance Profile**: EC2 assumes role → app gets credentials from IMDS
- **Lambda Execution Role**: Lambda assumes role to access DynamoDB, S3, etc.
- **Cross-Account Role**: Account A assumes role in Account B
- **SAML/OIDC Role**: Federated identity (corporate AD, Google, GitHub) assumes role

### AWS STS (Security Token Service)
STS issues temporary credentials (Access Key + Secret Key + Session Token):
- `AssumeRole`: Used by AWS services, cross-account access
- `AssumeRoleWithSAML`: SAML 2.0 federation (corporate AD via ADFS)
- `AssumeRoleWithWebIdentity`: OIDC federation (Cognito, Google, GitHub, Facebook)
- `GetSessionToken`: MFA enforcement for API calls

### IAM Best Practices
1. Lock root account, enable MFA on root immediately
2. Never use root for daily tasks — create admin IAM user
3. Grant **least privilege** — start with minimal permissions, add as needed
4. Use **roles** over long-term access keys wherever possible
5. Enable **MFA** for all human users
6. Rotate access keys regularly (or eliminate them using roles)
7. Use **IAM Access Analyzer** to detect overly permissive policies
8. Use **Permission Boundaries** to delegate admin without privilege escalation

### IAM Access Analyzer
- Analyzes policies to find resources accessible from outside your account/organization
- Generates findings for S3 buckets, IAM roles, KMS keys, Lambda functions with public or cross-account access
- Generates **least-privilege policies** based on CloudTrail activity
- Validates policies before applying (policy validation)

### AWS Organizations & SCPs
- **AWS Organizations**: Manage multiple AWS accounts under one umbrella
- **Organizational Units (OUs)**: Group accounts (Production OU, Dev OU, Sandbox OU)
- **SCPs (Service Control Policies)**: Guardrails applied at OU/account level; restrict what can be done regardless of IAM policies
- **Effective permissions = IAM Policy ∩ SCP** (intersection)

Example SCP: Deny creating resources in regions other than us-east-1 and us-west-2.

---

## 2. Interview Questions & Answers

### Q1. What is the difference between an IAM User and an IAM Role?
**IAM User**: A long-term identity with permanent credentials (username/password or access keys). Best for human users or legacy applications that don't support roles.

**IAM Role**: A temporary identity assumed by a trusted principal (user, service, account). Issues short-lived credentials via STS. No permanent credentials — much safer. Use roles for EC2 instances, Lambda functions, ECS tasks, cross-account access, and federated users.

**Best practice**: Eliminate IAM users with access keys wherever possible; use roles instead.

---

### Q2. Explain IAM policy evaluation. If a user has two policies — one ALLOW and one DENY on the same action — what happens?
**Explicit DENY always wins.**

Evaluation order:
1. Check for explicit DENY in any applicable policy → if found, **DENY**
2. Check for explicit ALLOW in any applicable policy → if found, **ALLOW**
3. Otherwise → **implicit DENY**

This means a DENY policy will override any ALLOW policy, regardless of which is attached or in which order.

---

### Q3. What is the difference between an Identity-based policy and a Resource-based policy?
**Identity-based policies**: Attached to IAM users, groups, or roles. Define what that identity can do.

**Resource-based policies**: Attached to AWS resources (S3 buckets, SQS queues, Lambda functions, KMS keys). Define who can access that resource.

**Cross-account access**: Resource-based policies can grant access to principals in other accounts without requiring the other account to assume a role. For example, an S3 bucket policy can allow `arn:aws:iam::123456789:root` to access objects.

---

### Q4. What is a Permission Boundary and when would you use it?
A Permission Boundary is a managed policy that sets the **maximum permissions** an IAM user or role can have. Even if the user's identity policies grant more, the effective permissions are the intersection.

Use case: **Delegated administration** — you want to allow a developer to create IAM roles for their applications, but prevent them from creating roles more powerful than their own permissions (privilege escalation prevention).

```
Developer's permissions: Full IAM + EC2
Permission Boundary on developer: Cannot grant S3 or DynamoDB
Effective: Developer can create roles, but those roles cannot have S3/DynamoDB
```

---

### Q5. What is AWS STS and what are the common use cases?
AWS STS (Security Token Service) provides **temporary, short-lived credentials**.

Common APIs:
- **AssumeRole**: EC2/Lambda/ECS assume an IAM role; cross-account access
- **AssumeRoleWithSAML**: Corporate users authenticate via AD/ADFS → get AWS credentials
- **AssumeRoleWithWebIdentity**: Cognito/Google/GitHub OIDC users → AWS credentials
- **GetSessionToken**: Users call AWS APIs with MFA — session token proves MFA was used

Temporary credentials expire after 15 minutes to 36 hours (default: 1 hour for AssumeRole).

---

### Q6. How would you set up cross-account access between Account A (DevOps) and Account B (Production)?
1. In **Account B**, create an IAM role (`ProductionDeployRole`) with a trust policy allowing Account A to assume it:
```json
{
  "Principal": {"AWS": "arn:aws:iam::AccountA_ID:root"},
  "Action": "sts:AssumeRole"
}
```
2. Attach permission policies to the role (e.g., `AmazonEC2FullAccess`)
3. In **Account A**, create an IAM policy allowing specific users/roles to call `sts:AssumeRole` on that role ARN
4. CI/CD in Account A calls `aws sts assume-role --role-arn arn:aws:iam::AccountB_ID:role/ProductionDeployRole --role-session-name deploy`
5. Use the temporary credentials to deploy to Account B

---

### Q7. What is IAM Access Analyzer?
IAM Access Analyzer continuously analyzes IAM policies and resource policies to find:
- Resources accessible to **external principals** (outside your account or organization)
- Generates findings with the source of access, the resource, and the access type

Additionally:
- **Policy validation**: Checks policies for syntax errors and best practice violations before applying
- **Unused access analysis**: Identifies IAM users and roles that haven't used certain permissions in 90 days (for least-privilege cleanup)
- **Policy generation**: Analyzes CloudTrail logs to generate a minimal policy that matches actual API usage

---

### Q8. What is ABAC (Attribute-Based Access Control) in IAM?
ABAC uses **tags** to control access. Instead of creating one policy per team, you create one policy that uses conditions based on tags.

Example: Allow access only when the resource tag `Environment` matches the user's tag `Environment`:
```json
{
  "Condition": {
    "StringEquals": {
      "aws:ResourceTag/Environment": "${aws:PrincipalTag/Environment}"
    }
  }
}
```
A developer tagged `Environment=dev` can only access resources tagged `Environment=dev`. Scale-friendly — no policy changes as teams grow.

---

### Q9. How do you implement MFA enforcement for AWS CLI access?
Create an IAM policy with a **Deny** statement for all actions except `sts:GetSessionToken` when MFA is not present:
```json
{
  "Effect": "Deny",
  "NotAction": "sts:GetSessionToken",
  "Resource": "*",
  "Condition": {
    "BoolIfExists": {"aws:MultiFactorAuthPresent": "false"}
  }
}
```
Users must call `aws sts get-session-token --serial-number arn:...mfa/user --token-code 123456` to get temporary credentials with MFA. Only these credentials can make API calls.

---

### Q10. What are Service Control Policies (SCPs) and how are they different from IAM policies?
**SCPs** are guardrails applied at the AWS Organization OU or account level. They don't grant permissions — they **restrict the maximum** permissions available in an account.

Even if an IAM policy in Account A grants full S3 access, if the SCP on Account A's OU denies `s3:DeleteBucket`, no one in Account A can delete S3 buckets — not even root.

**Key difference**: IAM policies grant/deny specific actions to specific principals within an account. SCPs constrain what's possible within an entire account or OU.

---

### Q11. What is the difference between IAM roles for EC2 and Lambda?
Both use IAM roles, but attachment mechanism differs:
- **EC2**: Role attached via **Instance Profile**. Credentials available via IMDS at `169.254.169.254/latest/meta-data/iam/security-credentials/`. EC2 SDK auto-refreshes before expiry.
- **Lambda**: Role specified as the **execution role**. Lambda service injects temporary credentials as environment variables (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN`). SDK picks them up automatically.

In both cases, the role provides auto-refreshing temporary credentials — never hardcode access keys.

---

### Q12. What is an IAM Identity Center (formerly SSO)?
AWS IAM Identity Center provides centralized access management for **multiple AWS accounts and applications** through a single sign-on portal.

Features:
- Integrates with external identity providers (Active Directory, Okta, Google Workspace)
- Users log into a portal, see all accounts/apps they have access to
- Issues short-lived credentials per account automatically
- **Permission Sets**: Define what access level (role) a user gets in each account
- Replaces the need to create IAM users in every account

---

### Q13. What are inline policies vs managed policies? Which should you prefer?
**Inline Policies**: Embedded in a single IAM entity. Deleted when the entity is deleted. Hard to reuse.

**Managed Policies**: Standalone JSON documents attached to multiple entities. Two types:
- *AWS Managed*: Created and updated by AWS; e.g., `ReadOnlyAccess`
- *Customer Managed*: You create, version, and maintain

**Prefer customer-managed policies**: Reusable, versioned, auditable. Use inline only for unique, entity-specific permissions that shouldn't be reused.

---

### Q14. What is the IAM Roles Anywhere feature?
IAM Roles Anywhere lets **workloads outside AWS** (on-premises servers, edge devices, containers) assume IAM roles using **X.509 certificates** issued by your certificate authority.

Use case: On-premises CI/CD server needs to deploy to AWS without hardcoding credentials. Configure IAM Roles Anywhere with your CA certificate, and the server uses its cert to get temporary AWS credentials.

---

### Q15. How do you audit IAM usage in your AWS account?
1. **CloudTrail**: Log all IAM API calls (CreateUser, AttachPolicy, AssumeRole) — store in S3, query with Athena
2. **IAM Access Analyzer**: Finds unused access and external resource sharing
3. **Credential Report**: CSV report of all IAM users, their last password/key usage, MFA status — generate in console or CLI
4. **IAM Access Advisor**: Shows last-used timestamp for each service permission per user/role — helps prune overly broad policies
5. **AWS Config Rules**: `iam-user-mfa-enabled`, `iam-no-inline-policy-check`, `iam-root-access-key-check`

---

## 3. Real-World Use Case: Multi-Account Cross-Account Access Architecture

### Scenario
A SaaS company with AWS Organizations structure needs:
- Development, Staging, Production accounts
- DevOps engineers can deploy to all accounts from a CI/CD account
- Developers can only access Dev account
- No IAM users in Production — only roles
- Audit all access for SOC 2 compliance
- Prevent any account from accessing regions outside us-east-1

### Architecture

```
AWS Organizations
├── Root
│   ├── Management Account (billing, org management only)
│   ├── Security OU
│   │   └── Security Account (CloudTrail, GuardDuty, Security Hub aggregator)
│   ├── Infrastructure OU
│   │   └── CI/CD Account (CodePipeline, GitHub Actions OIDC)
│   ├── Production OU  ←── SCP: deny all non-us-east-1 regions
│   │   └── Prod Account (no IAM users, roles only)
│   └── Non-Prod OU
│       ├── Dev Account
│       └── Staging Account

SCPs applied to Production OU:
  - Deny all actions in non-approved regions
  - Deny iam:CreateUser (no IAM users in prod)
  - Deny s3:DeleteBucket (protect data)
  - Deny ec2:TerminateInstances without tag "Env=prod-approved"

IAM Identity Center (SSO):
  Connects to Okta (SAML 2.0)
  Permission Sets:
    DevAccess → Dev Account (PowerUserAccess)
    StagingReadOnly → Staging Account (ViewOnlyAccess)
    ProdReadOnly → Prod Account (ViewOnlyAccess)

CI/CD (GitHub Actions) → OIDC Provider in each account:
  GitHub Actions → AssumeRoleWithWebIdentity
  → CI/CD-DeployRole (Prod Account)
  → CodeDeploy, ECS, Lambda permissions

Cross-Account Role Assumption:
  CI/CD Account CodePipeline
    → assumes ProdDeployRole in Prod Account
    → ProdDeployRole trust: {"Principal": {"AWS": "arn:aws:iam::CI/CD-AccountID:role/CodePipelineRole"}}

Audit:
  CloudTrail (all accounts) → S3 bucket in Security Account (cross-account delivery)
  IAM Access Analyzer → Organization-wide analysis
  AWS Config → iam-user-mfa-enabled, iam-no-inline-policy-check (all accounts)
  Credential Report → Lambda rotates and emails monthly
```

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| No IAM users in Production | SCP enforced — eliminates long-term credential risk in prod |
| GitHub OIDC → AssumeRoleWithWebIdentity | Eliminates stored access keys in CI/CD secrets — rotate-free |
| SCPs for region restriction | Prevent accidental resource creation in unapproved regions — compliance |
| IAM Identity Center + Okta | Single sign-on for humans — no per-account IAM users; MFA enforced by Okta |
| Permission Boundaries on CI/CD roles | Even if CI/CD role is compromised, cannot create roles more powerful than itself |
| Cross-account CloudTrail to Security Account | Prod teams can't tamper with audit logs |

### Interview Narration (White-board Script)
> "For a multi-account enterprise setup I'd start with AWS Organizations and SCPs as the guardrails. Production OU has an SCP that denies creating resources outside approved regions and denies creating IAM users — roles only. Humans access accounts via IAM Identity Center connected to our Okta IdP — they see a portal listing their permitted accounts and click to assume the right role with short-lived credentials. CI/CD uses GitHub Actions OIDC provider so our pipelines never store AWS access keys — they exchange a GitHub OIDC token for temporary AWS credentials through AssumeRoleWithWebIdentity. All CloudTrail logs land in the Security account's S3 bucket — no production team can delete those logs."
