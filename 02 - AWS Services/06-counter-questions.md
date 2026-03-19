# Counter Questions & Tough Answers — Interview Prep

These are questions designed to probe gaps or test your depth. Answer honestly but confidently.

---

## Gap Questions (Your Weak Spots)

### "Your resume shows Vue.js and Angular — not React. How will you handle React?"

**Answer:**
> "You're right — my frontend framework experience is Vue.js and Angular, not React. However, all three are component-based SPA frameworks sharing the same mental model: virtual DOM, component lifecycle, props and state, reactive rendering. Vue's Options API and React Hooks solve the same problems differently. I've intentionally learned both Vue.js and Angular to understand different approaches. Getting productive in React — especially with my TypeScript background — is straightforward. I'd estimate a few weeks to be comfortable, a couple of months to be proficient."

---

### "You don't have VB.NET on your resume. Is that a problem?"

**Answer:**
> "My .NET experience is in C#, which runs on the same CLR as VB.NET. The language features and libraries are identical — only syntax differs. I can read and understand VB.NET code, and context like this role usually involves maintaining or migrating legacy VB.NET apps. If anything, this is a ramp-up situation, not a blocker. More importantly, I understand what modernization looks like — moving those apps to containers using App2Container or transforming them with Amazon Q — which is where the real value lies."

---

### "Have you actually used Amazon Q in your work?"

**Answer:**
> "Not Amazon Q specifically. But I've been a heavy user of GitHub Copilot and Claude Code — both of which are directly comparable AI coding assistants. I understand the workflow shift they enable: faster boilerplate generation, context-aware suggestions, and AI-assisted code reviews. Amazon Q Developer is the AWS-native equivalent. I've researched Q's specific capabilities — code transformation for .NET Framework modernization, security scanning, and AWS service integration — and given my existing AI-assisted development experience, I'd ramp up quickly."

---

### "Have you used App2Container in a real project?"

**Answer:**
> "I haven't used App2Container directly in production yet. However, I've studied how it works — it discovers running .NET apps on IIS, analyzes dependencies, generates Dockerfiles and container images, and produces CloudFormation templates for ECS/EKS deployment. The reason I find it compelling is that it solves a real problem I understand: containerizing legacy ASP.NET apps without needing deep Docker expertise or source code access. I'd be comfortable setting up a proof-of-concept with it."

---

### "Have you done CAST analysis?"

**Answer:**
> "I haven't directly run CAST Highlight or CAST Imaging on a project. But I understand the role it plays: CAST is the assessment layer before you start migrating — it gives you portfolio-level data on cloud readiness, technical debt, and migration risk so you can prioritize and plan. In my experience with the SSAS to Redshift migration, we did a similar manual assessment phase to understand dependencies and data model complexity before starting. CAST automates and scales that analysis. I understand its position in the modernization workflow."

---

### "Your AWS experience is from eGain — which ended in May 2025. What have you been doing since?"

**Answer:**
> "Since June 2025, I've been at Energy Exemplar focused on .NET C# and Vue.js. The AWS skills don't atrophy — I've continued building my cloud knowledge independently. My learning repository covers system design, cloud architectures, and I've been preparing specifically for AWS modernization scenarios for this role. I'm actively studying AWS services, containerization, and tools like Amazon Q and App2Container."

---

## Deep Technical Counter-Questions

### "You mentioned Redshift — what distribution style did you use and why?"

**Answer:**
> "For our analytics workloads, we used KEY distribution on the primary join key — typically the customer/account ID — to co-locate related rows on the same compute node and reduce data shuffling during joins. For smaller dimension tables, we used ALL distribution so every node has a copy. EVEN distribution was the default we moved away from because it doesn't account for query patterns. The choice depends on your query patterns and data skew."

---

### "How did you manage secrets in Lambda functions?"

**Answer:**
> "We stored credentials in AWS Secrets Manager and retrieved them at Lambda startup using the SDK. The Lambda execution role had an IAM policy granting `secretsmanager:GetSecretValue` for specific secret ARNs — least privilege. We also used environment variables for non-sensitive config. Avoiding hardcoded credentials was a key part of our security posture."

---

### "What's the difference between a container image and a container?"

**Answer:**
> "An image is the blueprint — a read-only, layered filesystem built from a Dockerfile. A container is a running instance of that image. You can run multiple containers from the same image simultaneously. Images are stored in registries (ECR), containers run on hosts or orchestrators like ECS."

---

### "Describe a time your code caused a production issue."

**Tip:** Be honest. Interviewers respect self-awareness over perfection.

**Sample Answer:**
> "During the Redshift migration at eGain, an ETL Lambda function I wrote had a race condition in how it handled concurrent data loads — it occasionally caused duplicate records. We caught it in a data validation check before it impacted reports. I added idempotency checks using a staging table and a MERGE/UPSERT pattern. Lesson: ETL pipelines need explicit deduplication — don't assume single execution."

---

## Questions to Ask the Interviewer

These show initiative and help you assess the role:

1. "What is the current state of the modernization project — are you in the assessment phase with CAST, active containerization, or already deploying to ECS/EKS?"
2. "What percentage of the codebase is VB.NET vs C# today, and what's the roadmap for language modernization?"
3. "How is the team structured — dedicated cloud platform team, or full-stack engineers owning their own migrations?"
4. "What does the DevOps pipeline look like — are you using CodePipeline/CodeBuild, or GitHub Actions/Jenkins?"
5. "How are you using Amazon Q today — is it developer-facing (Q Developer in IDEs) or enterprise-facing (Q Business)?"
6. "What does success look like for this role in the first 90 days?"

---

## Salary / Offer Questions

If asked about expectations:
> "I'm looking for a compensation package aligned with the scope of this role and the AWS cloud modernization expertise you're hiring for. I'm flexible — can you share the budgeted range for this position?"

---

## Red Flags to Watch For

- Vague answers about project scope ("we're modernizing everything") — probe for specifics
- No clear ownership structure — who decides architecture?
- All three tools (Amazon Q, App2Container, CAST) in JD but no one on team has used them — you'd be the pioneer (opportunity or trap?)
- .NET Framework + VB.NET with no modernization plan = maintenance role, not cloud role
