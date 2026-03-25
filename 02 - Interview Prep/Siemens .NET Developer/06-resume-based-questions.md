# Resume-Based Questions — Akash Soni

> These are questions the interviewer will likely ask based on your resume. Prepare crisp answers.

---

## About Your Current Role (Energy Exemplar)

**Q: Walk me through the Import/Export module you built for PLEXOS Cloud.**
- Built a .NET C# NuGet package consumed by a CLI project
- Converts PTI Raw (PSS/E raw format) files into PLEXOS-compatible SQLite database files
- Enables seamless data ingestion for energy modeling workflows
- *Key detail:* parsing complex industry-standard power system data formats, handling edge cases in file structures, writing to SQLite efficiently

**Q: Why did you choose to build it as a NuGet package consumed by a CLI?**
- Separation of concerns — core logic in the NuGet package is reusable by other consumers (APIs, tests)
- CLI is just one consumer — tomorrow a web API or another service could consume the same package
- Easy versioning and distribution via NuGet feed
- Testability — package can be unit tested independently

**Q: Tell me about the Excel Add-In you built with Vue.js.**
- Built UI layer for the Pre-Post feature using Vue.js inside Excel environment
- Enables interactive data analysis — users can analyze simulation results without leaving Excel
- *Be ready to explain:* how Vue.js runs inside Excel (Office.js / Office Add-In framework), challenges of building SPAs in constrained environments

**Q: What was your contribution to the EE Marketplace?**
- Frontend development using Vue.js
- Delivered responsive, intuitive interfaces for the energy software marketplace
- *Mention:* component-based architecture, state management, API integration

---

## About eGain Communications (3+ years)

**Q: Describe the SSAS to AWS Redshift migration you led.**
- **Why:** SSAS (SQL Server Analysis Services) had performance and scalability limitations
- **What:** Migrated the analytics product to AWS Redshift (columnar, cloud-native)
- **How:** ETL redesign, query rewriting (MDX → SQL), schema optimization for columnar storage
- **Result:** Improved data accessibility, performance, and scalability
- *Be ready for:* "What challenges did you face?" — schema differences, query translation, data validation, downtime planning

**Q: How did you design your RESTful APIs at eGain?**
- Followed REST conventions: proper HTTP methods, status codes, resource-based URLs
- Optimized JSON serialization/deserialization (likely using `System.Text.Json` or `Newtonsoft.Json`)
- *Mention:* pagination, filtering, versioning, error handling patterns
- *Be ready for:* "How did you handle large payloads?" — streaming, compression, pagination

**Q: Tell me about the AWS automation you built.**
- Used Lambda for serverless compute, EC2 for heavier workloads
- Secrets Manager for secure credential management
- *Example:* automated deployment pipelines, scheduled data processing jobs
- **Result:** reduced deployment time, minimized manual errors

**Q: What kind of SSRS reports did you customize?**
- Delivered actionable analytics dashboards for stakeholders
- *Mention:* parameterized reports, drill-through, subscriptions, data-driven layouts

---

## About IKS Health

**Q: Explain the secure login system you built with Angular and JWT.**
- Angular frontend + JWT-based authentication
- **Flow:** user logs in → server validates credentials → returns signed JWT → frontend stores token → sends in Authorization header on subsequent requests
- *Be ready to explain:* token expiration, refresh tokens, XSS prevention (HttpOnly cookies vs localStorage), CORS

**Q: What security vulnerabilities did you fix?**
- Strengthened authentication, session management, and data protection
- *Likely areas:* SQL injection, XSS, CSRF, insecure session handling, improper input validation
- *Mention:* OWASP Top 10 awareness

**Q: Tell me about the Redox API integration.**
- Redox = healthcare data integration platform (standardized EHR data exchange)
- Enabled real-time EHR data exchange for the Scribble WFM platform
- *Mention:* HL7/FHIR standards awareness, data mapping, error handling for external APIs

**Q: Describe the CSV/Excel upload feature.**
- Generic, reusable upload feature — reduced manual data entry
- *Implementation:* file validation, column mapping, bulk insert, error reporting
- Used libraries like EPPlus or NPOI for Excel parsing

---

## Technical Skills Deep-Dive

**Q: You mention AI-assisted development (Copilot, Claude Code). How do you use them?**
- Code generation for boilerplate, unit tests, and repetitive patterns
- Code review and refactoring suggestions
- Documentation generation
- *Key point:* always review AI-generated code — it accelerates but doesn't replace engineering judgment
- *Example:* "I used Claude Code to accelerate the import/export module, particularly for parsing logic and test case generation"

**Q: Compare Vue.js and Angular (you've used both).**

| Aspect | Vue.js | Angular |
|--------|--------|---------|
| Learning curve | Gentle | Steep |
| Size | Lightweight | Full framework |
| Data binding | Two-way + one-way | Two-way |
| State mgmt | Vuex/Pinia | NgRx/Services |
| When to use | Flexible, incremental adoption | Large enterprise apps |

**Q: You've worked with both SQLite and SQL Server/Redshift. When would you use each?**
- **SQLite:** embedded, file-based, zero-config — great for local apps, CLI tools, mobile (your PLEXOS use case)
- **SQL Server:** enterprise OLTP, stored procedures, strong tooling
- **Redshift:** columnar, cloud-native, optimized for analytics/OLAP at scale

**Q: Explain your experience with CI/CD pipelines.**
- Automated build, test, deploy workflows
- *Mention:* Git branching strategies (GitFlow/trunk-based), automated testing, environment promotion
- Link to your eGain achievement: "automated deployment of key infrastructure, significantly reducing deployment time"

---

## Education & Background

**Q: Tell me about your CDAC experience.**
- PG Diploma in Advanced Computing from CDAC ACTS, Pune (80.5%)
- Intensive program covering OS, networking, web tech, Java, .NET, databases

**Q: Why did you move from Mumbai to Pune?**
- Career growth opportunity at eGain (if applicable), Pune's strong IT ecosystem

---

## Situational Questions Based on Your Experience

**Q: You've transitioned between healthcare, communications, and energy sectors. How do you adapt?**
- Focus on understanding the domain quickly — talk to domain experts, read documentation
- Core engineering principles (clean code, SOLID, testing) transfer across domains
- Each domain taught something unique: healthcare (compliance, security), communications (analytics, scale), energy (data modeling, scientific computing)

**Q: What's the most complex technical problem you've solved?**
- Good candidates: SSAS → Redshift migration (data, scale, business impact) or PTI Raw parser (complex file format, domain-specific)

**Q: How do you approach learning a new technology?**
- Hands-on first — build something small (reference your learn-grow repo)
- Official docs → tutorials → real project
- AI tools to accelerate understanding
