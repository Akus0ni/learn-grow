# Your STAR Stories — Mapped to Qentelli JD

These are YOUR real examples. Say these out loud before the interview.

---

## 1. Performance Optimization / Cloud Migration
*Use for: "Tell me about a performance improvement", "AWS experience", "cloud-native work"*

**Situation:** At eGain, the analytics platform ran on a legacy SSAS stack — slow, hard to scale, expensive to maintain.

**Task:** Migrate the entire analytics layer to AWS without data loss or downtime for stakeholders.

**Action:** Led the service-oriented modernization — redesigned ETL data pipelines (Apache NiFi), refactored the data-access layer in C#/.NET to point to Redshift, introduced automated validation scripts to verify data integrity at every migration step. Used AWS Lambda and EC2 for orchestration, Secrets Manager for credentials.

**Result:** >30% query performance improvement. Zero data loss. Multiple stakeholder-facing SSRS reports replaced with Redshift-backed dashboards.

**One-liner to open with:** "I led a full analytics platform migration from SSAS to AWS Redshift at eGain — delivered 30% query performance improvement with no data loss."

---

## 2. Security / JWT Authentication
*Use for: "Security best practices", "authentication", "resolving vulnerabilities", AWS security questions*

**Situation:** At IKS Health, the Scribble WFM platform had critical security vulnerabilities in its authentication and document-management services — a HIPAA-adjacent clinical SaaS, so the risk was severe.

**Task:** Harden access control and resolve the security weaknesses before they could be exploited.

**Action:** Engineered a JWT-based authentication module from scratch using Angular + TypeScript + ASP.NET. Fixed session management flaws, corrected access control in document services, integrated with the Redox API for secure EHR data exchange between external health systems.

**Result:** Resolved all critical vulnerabilities. Platform passed security review. Secured structured clinical data flows across external systems.

**One-liner:** "I resolved critical security vulnerabilities in a HIPAA-adjacent SaaS — built the JWT auth module end-to-end in Angular and ASP.NET."

---

## 3. Reusable Architecture / API Design
*Use for: "Clean code / maintainability", "cross-team collaboration", "design decisions"*

**Situation:** At Energy Exemplar, multiple product teams were writing custom scripts independently to handle PTI Raw file transformation for PLEXOS Cloud integrations — duplicated effort across teams.

**Task:** Package core logic into a reusable, shareable service that any team could consume.

**Action:** Designed and delivered the Import/Export integration subsystem as a NuGet service library with a clean interface contract. Built a cross-platform CLI on top of the same library so teams could run integration workflows without any custom scripting.

**Result:** Adopted by multiple product teams as a shared service pattern — eliminated duplicate effort across the org. Vue.js Excel Add-In built on the same library for power-user data analysis.

**One-liner:** "I packaged core integration logic as a NuGet service library at Energy Exemplar — multiple product teams adopted it as a shared service pattern, eliminating duplication."

---

## 4. AWS Lambda / Automation
*Use for: "AWS Lambda experience", "serverless", "automated workflows"*

**Situation:** At eGain, several operational workflows were manual and error-prone — triggered by schedules or data events.

**Task:** Automate these workflows reliably with proper validation.

**Action:** Automated workflows using AWS Lambda, EC2, and Secrets Manager. Embedded unit-level tests and validation scripts within the Lambda functions to guarantee reliability across asynchronous event flows.

**Result:** Reliable, tested automation — reduced manual ops burden and eliminated a class of human-error incidents.

**One-liner:** "At eGain I automated operational workflows with AWS Lambda and EC2, with unit tests baked into the Lambda handlers for reliability guarantees."

---

## 5. CI/CD & Code Quality
*Use for: "How do you ensure quality", "CI/CD experience", "Agile"*

**Situation:** At eGain, defect leakage into production was higher than acceptable.

**Task:** Improve code quality and reduce defects escaping to production.

**Action:** Championed Agile best practices — introduced CI/CD pipelines (GitHub Actions), enforced peer code reviews as a gate before merge, added automated testing. Used GitHub Copilot and Claude Code at Energy Exemplar to enforce consistent style and catch issues during development.

**Result:** 25% reduction in defect leakage at eGain. Faster, more consistent delivery cycles.

**One-liner:** "I championed CI/CD and peer reviews at eGain — reduced defect leakage by 25%."

---

## 6. Cross-Functional Collaboration / Impact
*Use for: "Working with other teams", "stakeholder communication"*

**Situation:** At IKS Health, operations teams were spending significant time on manual CSV/Excel data entry.

**Task:** Reduce manual effort and improve data integrity.

**Action:** Developed a generic CSV/Excel bulk-upload integration — designed it generically so it could handle multiple data shapes rather than being purpose-built for one use case.

**Result:** 40% reduction in manual data-entry effort. Improved data integrity across operations workflows.

**One-liner:** "Built a generic bulk-upload integration at IKS Health that cut manual data entry by 40%."

---

## Gap Handling (S3, RDS, Angular recency)

**If asked about S3 specifically:**
> "My direct AWS experience at eGain was heavily Lambda, EC2, Redshift, and Secrets Manager. S3 I've worked with for storing artifacts and static assets — the SDK pattern is very similar to the rest of the AWS SDK I've used. I'm comfortable picking it up immediately."

**If asked about RDS specifically:**
> "At eGain I worked with AWS Redshift as our cloud data store — managed database, connection pooling, query optimization, all within AWS. For transactional relational databases I have deep SQL Server experience. RDS is essentially managed SQL Server or PostgreSQL — the .NET connection string and EF Core patterns are identical."

**If asked about Angular vs Vue.js (you've done both):**
> "My Angular work was at IKS Health — TypeScript, component architecture, RxJS observables, JWT integration. More recently at Energy Exemplar I worked with Vue.js on a similar component model. The concepts transfer directly — I'm comfortable picking Angular back up immediately, especially with v10+ improvements I've been following."

---

## Your Strongest Selling Points for This Role

1. **You've done a real AWS migration** — SSAS to Redshift, Lambda, EC2, Secrets Manager in production
2. **You've built security-critical auth** — JWT module on Angular + ASP.NET for HIPAA-adjacent healthcare
3. **6+ years of C#/.NET** across all three roles — APIs, NuGet libraries, CLI tools
4. **Reusable architecture mindset** — NuGet library adopted cross-team is a great engineering story
5. **You use AI tooling professionally** — GitHub Copilot + Claude Code — relevant for a modern team
