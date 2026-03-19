# Behavioral & Leadership — STAR Stories from Your Resume

---

## The STAR Format

- **S**ituation — context, what was happening
- **T**ask — your responsibility in it
- **A**ction — what you specifically did
- **R**esult — measurable outcome

---

## Story 1: Cloud Migration Leadership (SSAS → AWS Redshift)

**Best for:** "Tell me about a cloud migration you led" / "Describe a complex technical project"

**S:** At eGain, our analytics product ran on Microsoft SSAS — an on-premises solution with scaling limitations, high maintenance overhead, and restricted accessibility for a growing distributed team.

**T:** I was responsible for migrating the analytics data infrastructure from SSAS to AWS Redshift — keeping the product functional throughout the migration with no disruption to business reporting.

**A:**
- Analyzed the existing SSAS data models and translated them to Redshift-compatible schemas (columnar, MPP-optimized)
- Implemented Lambda functions to automate ETL workflows replacing SSAS processing jobs
- Used Secrets Manager to manage database credentials securely — replacing hardcoded config
- Automated deployment pipelines for the new infrastructure, significantly reducing manual effort
- Collaborated cross-functionally with the analytics team to validate data accuracy post-migration

**R:** The migration improved data processing speeds and overall system performance. Infrastructure deployment was automated, cutting deployment time significantly. The analytics product became more scalable and accessible for the team.

---

## Story 2: Workflow Automation (Lambda + EC2)

**Best for:** "How have you improved operational efficiency?" / "DevOps/automation experience?"

**S:** Several manual operational workflows at eGain — deployments, data processing jobs, infrastructure management — were consuming significant engineering time and were error-prone.

**T:** Automate these workflows using AWS services to reduce manual effort and minimize errors.

**A:**
- Built Lambda functions to trigger automated workflows on events (scheduled, S3 events, API calls)
- Used EC2 for processes requiring persistent compute
- Centralized credential management using Secrets Manager — removed hardcoded secrets from code
- Built deployment automation that reduced the manual steps in our release process

**R:** Automated deployment of key infrastructure, significantly reducing deployment time. Teams could focus on product development instead of operational overhead.

---

## Story 3: RESTful API Design & Optimization (eGain)

**Best for:** "Describe your API design experience" / "Technical design decisions"

**S:** The analytics product at eGain required seamless integration with other internal systems and third-party tools via APIs. Existing data exchange had performance bottlenecks due to unoptimized serialization.

**T:** Design and implement efficient RESTful APIs that could handle high-frequency data exchange reliably.

**A:**
- Designed RESTful endpoints following REST principles (proper HTTP verbs, status codes, stateless design)
- Optimized JSON serialization/deserialization to reduce payload processing time
- Ensured APIs aligned with business requirements through stakeholder collaboration
- Maintained API documentation and versioning practices

**R:** Seamless system integration with improved data exchange performance. Reduced serialization overhead contributed to overall system responsiveness.

---

## Story 4: Security Vulnerability Resolution (IKS Health)

**Best for:** "Tell me about a time you handled a critical security issue"

**S:** At IKS Health, critical security vulnerabilities were discovered in Document Management and Abstraction 2.0 — affecting authentication, session management, and data protection.

**T:** Investigate and resolve the vulnerabilities quickly to protect patient data without disrupting ongoing operations.

**A:**
- Conducted a security audit of the affected modules
- Patched authentication vulnerabilities and strengthened session management
- Implemented JWT-based secure login for the Scribble WFM platform using Angular
- Improved data protection protocols across the affected services

**R:** Vulnerabilities resolved without service disruption. Improved system security posture and maintained compliance requirements for healthcare data handling.

---

## Story 5: AI-Assisted Development (Energy Exemplar)

**Best for:** "How do you stay current with new tools?" / "Tell me about using AI in development" / Amazon Q angle

**S:** At Energy Exemplar, working on PLEXOS Cloud — a complex energy modeling platform — required building a NuGet package for file conversion and Vue.js UI components under tight delivery timelines.

**T:** Deliver high-quality features efficiently while maintaining code quality across backend (.NET C#) and frontend (Vue.js) simultaneously.

**A:**
- Adopted GitHub Copilot and Claude Code as AI development tools for code generation, review, and refactoring
- Used AI assistance to accelerate boilerplate code generation, identify edge cases, and improve code quality
- Built the Import/Export module (NuGet package) for PTI Raw format conversion to SQLite
- Developed Vue.js UI for the Excel Add-In Pre-Post feature

**R:** Accelerated code delivery without sacrificing quality. This experience directly maps to Amazon Q Developer — I understand how AI coding assistants change the development workflow and accelerate productivity.

---

## Leadership & Communication Scenarios

### "Tell me about a time you collaborated cross-functionally"
> "During the Redshift migration at eGain, I worked with the analytics team to validate data accuracy, the ops team to coordinate deployment windows, and stakeholders to ensure business reporting wasn't disrupted. I communicated migration progress regularly and adjusted timelines based on feedback. The migration completed without business disruption."

### "How do you handle technical disagreements with your team?"
> "I focus on data and trade-offs. During the cloud transformation at eGain, there were discussions about which AWS services to use. I'd document the options — cost, scalability, maintenance overhead — and facilitate a decision based on the team's long-term needs. I'm open to being wrong if someone has better context."

### "How do you keep up with new technologies?"
> "I maintain a personal learning repository where I document new concepts — I've covered system design, Azure, and now AWS services. I applied this at Energy Exemplar by quickly adopting GitHub Copilot and Claude Code. I find that hands-on learning — building something small with a new tool — is the fastest way to internalize it. Amazon Q is on my learning list given the JD."

### "Describe your experience with Agile"
> "I've worked in Agile Scrum environments across all three companies. At eGain, we ran 2-week sprints with daily standups, sprint planning, and retrospectives. I was involved in backlog grooming — prioritizing features and bug fixes based on business impact. I also contributed to cross-team dependency management during the migration project."
