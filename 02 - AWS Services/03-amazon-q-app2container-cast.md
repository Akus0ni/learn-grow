# Amazon Q, App2Container & CAST Analysis — Interview Prep

These 3 tools are specifically called out in the JD. This is your biggest knowledge gap — prioritize this file.

---

## Amazon Q

### What Is It?
Amazon Q is AWS's **generative AI assistant** embedded across AWS services and developer tools. Think of it as GitHub Copilot / Claude Code — but deeply integrated into the AWS ecosystem.

### Variants

| Product | What It Does |
|---|---|
| **Amazon Q Developer** | AI coding assistant — code gen, debugging, testing, AWS-specific suggestions in IDEs (VS Code, JetBrains, Visual Studio) |
| **Amazon Q Business** | Enterprise chatbot on your company's internal data (connects to S3, Confluence, Jira, Salesforce) |
| **Amazon Q in QuickSight** | Natural language to BI dashboards/visualizations |
| **Amazon Q in Connect** | AI agent for customer service/contact centers |

### Amazon Q Developer (Most Relevant to JD)

**Key capabilities:**
- **Code generation** — suggest completions, generate functions from comments
- **Code transformation** — upgrade legacy code (e.g., Java 8 → Java 17, .NET Framework → .NET 8)
- **Security scanning** — detect vulnerabilities in code
- **Chat** — ask AWS architecture questions, get service recommendations
- **CLI integration** — `q chat` in terminal for AWS CLI help
- **Infrastructure generation** — generate CloudFormation/CDK from descriptions

**Supported IDEs:** Visual Studio, VS Code, JetBrains, Eclipse

**Your Angle:**
> "I've been using GitHub Copilot and Claude Code extensively in my recent work at Energy Exemplar and eGain. Amazon Q Developer is essentially the AWS-native equivalent — I understand the workflow of AI-assisted development and how it accelerates delivery. I'd get up to speed on Q Developer quickly given this existing mindset."

**Code Transformation Feature (Key for JD):**
Amazon Q Developer can automatically migrate:
- Java 8/11 → Java 17/21
- .NET Framework → .NET 8 (in preview)
This is critical for modernization projects — directly tied to App2Container use cases.

---

## App2Container (A2C)

### What Is It?
AWS App2Container is a **command-line tool** that automatically containerizes existing .NET and Java web applications running on Windows or Linux servers — without requiring source code changes.

**Supported apps:**
- ASP.NET on IIS (Windows) — directly relevant to JD's ASP.NET requirement
- Java apps on Tomcat/JBoss (Linux)

### How It Works (4 Steps)

```
1. DISCOVER    → A2C scans server for running .NET/Java apps
      ↓
2. ANALYZE     → Identifies app dependencies, ports, configs
      ↓
3. CONTAINERIZE → Generates Dockerfile + container image
      ↓
4. DEPLOY      → Generates ECS/EKS deployment manifests (CloudFormation)
```

### Commands
```bash
app2container discover --all                        # Find all apps on server
app2container analyze --application-id <id>        # Analyze specific app
app2container containerize --application-id <id>   # Build container image
app2container generate app-deployment --application-id <id>  # Generate ECS/EKS config
```

### What It Generates
- **Dockerfile** — for building the container image
- **docker-compose.yml** — for local testing
- **CloudFormation template** — for deploying to ECS or EKS
- **ECR push commands** — to publish the image

### Why It Matters for ASP.NET Modernization
Legacy ASP.NET apps on IIS are traditionally hard to containerize:
- Windows containers vs Linux containers
- IIS configuration complexity
- App pool dependencies

App2Container handles this automatically — critical for enterprises with .NET Framework apps that want to move to AWS.

**Your Angle:**
> "App2Container directly aligns with the ASP.NET experience in the JD. Many organizations have legacy ASP.NET/VB.NET applications running on IIS that they want to modernize. App2Container automates the containerization of those apps and generates ECS/EKS deployment artifacts — reducing what would be weeks of manual effort to hours. Given my background with .NET and AWS services, I can bridge the gap between the legacy app team and the cloud platform team."

---

## CAST Analysis (CAST Software / CAST Highlight)

### What Is It?
CAST is a **software intelligence and application analysis** platform. In the context of AWS/cloud migrations, it's used to assess application portfolios before modernization.

### Two Main Products

#### CAST Highlight (Cloud Migration Focus)
- **Portfolio assessment tool** — scans source code to assess cloud readiness
- Analyzes hundreds of apps quickly (days not months)
- Outputs: cloud readiness scores, migration effort estimates, risk areas
- Integrates with AWS Migration Hub

**Metrics it produces:**
| Metric | What It Means |
|---|---|
| Cloud Readiness | How easily the app can move to cloud (0-100) |
| Resiliency | App's ability to handle failures |
| Agility | How easily the codebase can change |
| Elegance | Code quality / technical debt |
| Business Impact | Business criticality of the app |

#### CAST Imaging (Deep Analysis)
- Full application dependency mapping
- Shows how components, databases, and services interact
- Transaction flow analysis
- Identifies hidden dependencies (critical for microservices decomposition)

### CAST in a Migration Project Workflow

```
1. Portfolio Discovery (CAST Highlight)
   → Scan all apps → get cloud readiness scores
   → Categorize: Rehost / Replatform / Refactor / Retire

2. Deep Analysis (CAST Imaging)
   → Map dependencies of complex apps
   → Identify refactoring targets

3. Modernization Execution
   → App2Container for lift-and-shift containers
   → Amazon Q for code transformation
   → Manual refactoring for complex components

4. Validate & Deploy
   → ECS/EKS deployment
   → CloudWatch monitoring
```

### The 6 R's of Migration (Context for CAST)
CAST helps classify apps using this framework:
- **Retire** — decommission unused apps
- **Retain** — keep on-prem (compliance, no cloud benefit)
- **Rehost** (Lift & Shift) — move as-is to EC2
- **Replatform** — minor optimizations (e.g., RDS instead of self-managed DB)
- **Refactor/Re-architect** — redesign for cloud-native
- **Repurchase** — switch to SaaS (e.g., Salesforce)

**Your Angle:**
> "CAST analysis is typically the first step in a large-scale migration engagement. It gives stakeholders a data-driven view of the application portfolio — which apps are cloud-ready, which carry technical debt, and where migration risk lies. I understand the value of having this kind of objective assessment before committing modernization resources. In my work at eGain, we went through a similar analysis phase before the SSAS to Redshift migration to understand data dependencies and risk."

---

## How These 3 Tools Work Together

```
CAST Highlight          Amazon Q Developer        App2Container
(Assess)                (Transform)               (Containerize)
    ↓                        ↓                         ↓
Identify which       AI-assisted refactoring    Auto-containerize
apps to migrate      of legacy code             .NET/Java apps
    ↓                        ↓                         ↓
                    AWS Migration Hub → ECS/EKS Deployment
```

**One-liner for interview:**
> "CAST tells you what to modernize and how hard it will be. Amazon Q helps developers transform the code. App2Container packages it into containers. Together they form AWS's modernization stack for legacy enterprise applications."

---

## Quick Q&A

**Q: What is Amazon Q?**
> AWS's AI-powered assistant embedded in developer tools and AWS services. For developers, Q Developer provides code generation, security scanning, and code transformation (e.g., .NET Framework modernization). I'm familiar with this category of tools — I actively use GitHub Copilot and Claude Code.

**Q: How does App2Container handle ASP.NET apps?**
> It discovers apps running on IIS, analyzes their configuration and dependencies, generates a Dockerfile and Windows container image, then produces CloudFormation templates for ECS or EKS deployment — all without requiring source code changes.

**Q: What is CAST used for?**
> CAST Highlight is used to assess application portfolios for cloud migration readiness. It scans source code to produce cloud readiness scores, technical debt metrics, and migration effort estimates, helping teams prioritize which apps to migrate and how.

**Q: What's the difference between App2Container and Docker?**
> Docker requires you to manually write Dockerfiles and understand the app's build process. App2Container automates this for existing .NET/Java apps — it discovers running apps, generates the Dockerfile, and creates deployment artifacts for ECS/EKS. It's specifically designed for legacy app modernization without needing source code expertise.
