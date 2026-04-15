# Agile, Behavioral & Situational Q&A

---

## Agile / Scrum

**Q: What is Agile? What are its core values?**

A: Agile is an iterative software development approach focused on delivering value incrementally and responding to change. The Agile Manifesto values:

1. **Individuals and interactions** over processes and tools
2. **Working software** over comprehensive documentation
3. **Customer collaboration** over contract negotiation
4. **Responding to change** over following a plan

---

**Q: What is Scrum? Explain the ceremonies.**

A: Scrum is an Agile framework for managing iterative development through **Sprints** (typically 2-week cycles).

| Ceremony | Purpose | Attendees |
|---|---|---|
| **Sprint Planning** | Define Sprint Goal, select backlog items, estimate | Dev team + Scrum Master + PO |
| **Daily Standup** | Sync on progress, blockers (15 min) | Dev team |
| **Sprint Review** | Demo working software to stakeholders | All + Stakeholders |
| **Sprint Retrospective** | Reflect on process improvements | Dev team |
| **Backlog Refinement** | Clarify, estimate, prioritize upcoming items | Dev team + PO |

---

**Q: What are the Scrum roles?**

- **Product Owner (PO)** — Owns the product backlog, prioritizes features, represents stakeholders.
- **Scrum Master** — Facilitates Scrum, removes blockers, coaches team on Agile practices.
- **Development Team** — Self-organizing, cross-functional team that delivers the sprint increment.

---

**Q: What is a user story? How do you write one?**

A: A user story is a short description of a feature from the user's perspective.

**Format:** `As a [user], I want [goal] so that [benefit].`

```
As a fund manager, I want to view a dashboard of NAV calculations 
so that I can monitor fund performance in real time.

Acceptance Criteria:
- Dashboard loads within 2 seconds
- Shows current NAV, % change, and last calculation timestamp
- Data refreshes every 5 minutes automatically
```

**Definition of Done (DoD):** Code written, unit tests passing, code reviewed, deployed to staging.

---

**Q: What is velocity in Scrum?**

A: Velocity is the average number of story points a team completes per sprint. Used for sprint planning and release forecasting. Measured over several sprints to get a reliable average.

---

**Q: What is the difference between Scrum and Kanban?**

| | Scrum | Kanban |
|---|---|---|
| Iterations | Fixed sprints | Continuous flow |
| Roles | Defined (PO, SM, Dev) | No prescribed roles |
| WIP limits | Implicit (sprint capacity) | Explicit per column |
| Planning | Sprint planning | On-demand |
| Change during iteration | Not encouraged | Allowed anytime |
| Best for | Feature development teams | Support/maintenance, ops |

---

## Behavioral Questions (STAR Format)

> **STAR = Situation → Task → Action → Result**

---

**Q: Tell me about yourself.**

> "I'm a full-stack .NET engineer with 6 years of experience building API-first backend services and enterprise UIs.
>
> At Energy Exemplar, my most recent role, I designed a reusable NuGet service library for PLEXOS Cloud that was adopted by multiple product teams as a shared service pattern — which is exactly the kind of architecture work I enjoy.
>
> Before that, at eGain, I led the modernization of their analytics platform from SSAS to AWS Redshift, improving query performance by over 30% with no data loss.
>
> I've worked with C#, .NET Core, SQL Server, REST APIs, and Angular/Vue.js on the frontend. I'm comfortable with Agile delivery, code reviews, and clean architecture principles.
>
> NAV's focus on technology-driven fund administration is a great fit — I'm excited about the challenge of building reliable, high-throughput financial systems."

---

**Q: Describe a challenging technical problem you solved.**

> **Situation:** At eGain, the analytics platform was running on SSAS which was becoming a bottleneck — query times were degrading and it couldn't scale for our growing client data.
>
> **Task:** I was tasked with leading the migration to AWS Redshift while ensuring no data loss and minimal disruption.
>
> **Action:** I redesigned the ETL pipelines using Apache NiFi, refactored the data-access layer in C#/.NET to support the new schema, and introduced automated validation scripts to verify data integrity at each stage. I ran parallel systems for a period to compare outputs before cutting over.
>
> **Result:** Query performance improved by over 30%, the system could now scale horizontally on AWS, and we achieved zero data loss during migration. The new architecture also reduced operational overhead for the team.

---

**Q: Tell me about a time you worked in an Agile team.**

> **Situation:** At Energy Exemplar, we used 2-week Scrum sprints to deliver the PLEXOS Cloud integration features.
>
> **Task:** As a developer, I was involved in sprint planning, daily standups, and code reviews.
>
> **Action:** I actively participated in backlog refinement — breaking down large features into estimable stories. I also championed peer code reviews which helped catch defects early and maintain code quality.
>
> **Result:** Our team maintained consistent velocity and reduced defect leakage by 25% compared to the prior release cycle. The integration library was delivered on time and adopted by multiple teams.

---

**Q: Describe a time you had to learn something quickly.**

> "At IKS Health, I was brought in to resolve security vulnerabilities in the Scribble WFM platform — a healthcare SaaS system I had never worked with before. The codebase had critical JWT authentication flaws that were putting patient data at risk.
>
> I quickly ramped up on the platform's architecture, the Redox API for EHR integration, and the specific security vulnerabilities. Within the first two weeks, I designed and implemented a proper JWT-based authentication module, fixed session management flaws, and secured the document-management services.
>
> I learned that combining focused reading with hands-on exploration of the codebase is the fastest way I learn new systems."

---

**Q: How do you handle disagreements with teammates about technical decisions?**

> "I believe technical decisions should be driven by data and clear criteria, not opinions. When I disagree with a teammate, I first make sure I understand their perspective fully before presenting mine.
>
> I try to frame the discussion around trade-offs — what are the pros/cons of each approach given our constraints (performance, maintainability, timeline)?
>
> If we're still stuck, I'll propose a small proof-of-concept or look for relevant precedents. Ultimately, I'm comfortable deferring to the team's decision once everyone's been heard — and I commit fully to whatever path we choose."

---

**Q: Why do you want to join NAV?**

> "NAV's position as one of the top 5 fund administrators globally — combined with its strong technology focus and the 'Best Administrator - Technology Award' — shows this is a company that invests seriously in engineering quality.
>
> The Transfer Agency system role is compelling because it's high-stakes, data-intensive work — exactly the kind of system where good architecture and reliable APIs matter most.
>
> I'm also drawn to the full-stack nature of the role. I genuinely enjoy working across the stack — from designing backend service patterns in .NET to building clean frontend experiences — and this role seems to value that range."

---

**Q: Where do you see yourself in 3 years?**

> "In 3 years, I'd like to have grown into a senior or lead engineering role. I enjoy architecture discussions and mentoring, so I'd love to be in a position where I'm helping shape system design decisions and supporting junior developers.
>
> In the nearer term, I want to deepen my expertise in financial domain systems — understanding the Transfer Agency workflows, NAV calculation pipelines, and how to build resilient, high-throughput systems for that domain."

---

## Questions to Ask NAV

> Asking good questions signals genuine interest and curiosity.

1. "What does the Transfer Agency system currently look like — what's the tech stack and what are the biggest engineering challenges the team is tackling?"
2. "How does the engineering team collaborate with the business/operations side?"
3. "What does the onboarding process look like for a new developer?"
4. "How are code reviews and technical standards managed across the team?"
5. "What's the roadmap for the system over the next 6-12 months?"
