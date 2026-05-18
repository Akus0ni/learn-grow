# Qentelli Interview Prep — Akash Soni

**Role:** .NET Developer with AWS | **Company:** Qentelli Solutions, Hyderabad

---

## Your Profile vs JD at a Glance

| JD Requirement | Your Experience | Strength |
|---|---|---|
| C#, ASP.NET Core, MVC | All 3 roles — APIs, NuGet libs, CLI tools | Strong |
| AWS (Lambda, EC2, RDS) | eGain — Lambda, EC2, Redshift, Secrets Manager | Strong (Redshift not RDS — see gaps) |
| Angular 10+ | IKS Health — JWT module in Angular + TypeScript | Moderate (Vue.js more recent) |
| CI/CD + Git | GitHub Actions, peer reviews, 25% defect reduction at eGain | Strong |
| Security best practices | IKS Health — JWT auth hardening, HIPAA-adjacent | Strong |

### Gaps to Handle Confidently
- **S3**: Not on resume. Say: *"My AWS work was Lambda, EC2, Redshift, Secrets Manager in production. S3 I've used for artifact/static storage — the SDK pattern is identical to what I've already used."*
- **RDS**: You used Redshift (OLAP) and SQL Server, not RDS. Say: *"I worked with Redshift as the managed cloud database at eGain. RDS is the OLTP equivalent — same .NET connection string patterns, same EF Core setup."*
- **Angular recency**: Vue.js is more recent. Say: *"I built the JWT auth module at IKS Health in Angular + TypeScript. More recently I've been in Vue.js — same component model, RxJS, TypeScript. I can pick Angular back up immediately."*

---

## Study Order (time-boxed for 3 hours)

| Time | File | Focus |
|------|------|-------|
| 0–30 min | [my-stories-star.md](my-stories-star.md) | YOUR real stories — say each one out loud |
| 30–60 min | [dotnet-csharp-quick-ref.md](dotnet-csharp-quick-ref.md) | async/await, DI lifetimes, EF Core, Web API |
| 60–90 min | [aws-quick-ref.md](aws-quick-ref.md) | S3 + Lambda sections — fill your gaps |
| 90–110 min | [angular-quick-ref.md](angular-quick-ref.md) | RxJS operators, change detection, interceptors |
| 110–120 min | [cicd-git-quick-ref.md](cicd-git-quick-ref.md) | CI/CD pipeline stages, GitHub Actions |
| 120–180 min | [interview-qa.md](interview-qa.md) | Say answers out loud — don't just read |

---

## Your 3 Best Stories (memorize these)

**1. AWS Migration (eGain)**
SSAS → AWS Redshift. Redesigned ETL pipelines, refactored C#/.NET data-access layer, automated validation with Lambda + EC2 + Secrets Manager. **Result: >30% query performance improvement, zero data loss.**
*Use for: cloud experience, AWS, performance, migration*

**2. JWT Auth Hardening (IKS Health)**
Resolved critical security vulnerabilities in a HIPAA-adjacent SaaS. Built JWT module in Angular + TypeScript + ASP.NET, fixed session management, secured document services, integrated Redox API for EHR data exchange.
*Use for: security, Angular, ASP.NET, critical bug resolution*

**3. NuGet Service Library (Energy Exemplar)**
Packaged Import/Export integration logic as a reusable NuGet library + CLI tool — adopted by multiple product teams as a shared service pattern. No more duplicate custom scripts per team.
*Use for: reusable architecture, clean code, cross-team impact*

---

## High-Priority Topics (most likely asked)

1. `async/await` in C# — how it releases threads, not just syntax
2. DI lifetimes — Scoped vs Transient vs Singleton with real examples
3. Lambda vs EC2 — when to use which (you have real experience with both)
4. S3 upload/download from .NET SDK — skim the code in aws-quick-ref.md
5. RxJS `switchMap` vs `mergeMap` — most common Angular operator question
6. CI/CD pipeline stages — what happens between push and prod
7. REST API design — HTTP verbs, status codes, versioning

---

## Questions to Ask Them
- What does the day-to-day development workflow look like?
- Which AWS services does the team use most heavily?
- How is the engineering team structured — squads or feature teams?
- What does success look like in the first 90 days?
