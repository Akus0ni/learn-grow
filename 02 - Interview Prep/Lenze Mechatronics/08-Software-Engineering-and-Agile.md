# Software Engineering, SDLC & Agile - Interview Q&A

> Sr. Software Engineer (6+ YoE) | C#/.NET Focus

---

### Q1: Detail the phases of the Software Development Life Cycle (SDLC) and their significance.

**Answer:** The SDLC is a structured framework spanning from inception to retirement of software.
1. **Requirement Analysis:** Understanding stakeholder needs, defining the scope, and capturing functional/non-functional requirements. Misalignment here guarantees failure regardless of code quality.
2. **Architecture & Design:** System architects and senior developers design the system topology (HLD) and the class/database structures (LLD), choosing the tech stack and defining API contracts.
3. **Implementation/Coding:** Developers translate the design into code. 
4. **Testing (QA):** Validating the implementation against requirements. This includes Unit, Integration, System, Performance, and Security testing.
5. **Deployment:** Releasing the software to staging/production environments, utilizing CI/CD pipelines.
6. **Maintenance & Operations:** Monitoring system health, fixing post-release bugs, and applying patches.

*Senior Perspective:* For a Senior Engineer, the SDLC is not linear. Modern systems utilize continuous SDLC micro-cycles. Senior engineers participate heavily in the Design and Operations phases (DevOps mindset), not just Coding.

---

### Q2: Contrast the traditional Waterfall model with Agile methodologies. When might Waterfall actually be preferred?

**Answer:**
- **Waterfall:** A rigid, linear sequential design process. You must sign off on Requirements before moving to Design, and finish Design before Coding. 
  - *Drawbacks:* Extremely inflexible. Stakeholders don't see working software until the very end. Changes in requirements late in the cycle are disastrously expensive.
- **Agile:** An iterative, incremental approach. Software is built in small time-boxed phases (sprints). The focus is on rapid delivery of working software and adapting continuously to changing requirements. Cross-functional teams collaborate dynamically.

**When Waterfall is preferred:**
While Agile is the default for most modern software, Waterfall is ideal for systems where requirements are perfectly known upfront, and the cost of failure/change is astronomically high. Examples include:
- Aerospace avionics software.
- Medical device firmware governed by strict FDA regulatory compliance frameworks.
- Heavy infrastructure defense contracts requiring massive upfront specification documents.

---

### Q3: Explain the Scrum framework, detailing its three core Roles and primary Artifacts.

**Answer:** Scrum is the most widely adopted Agile framework, executing work in iterations (Sprints) usually 2 to 4 weeks long.

**Three Core Roles:**
1. **Product Owner (PO):** The voice of the customer. Responsible for maximizing the value of the product, owning the Product Backlog, and writing/prioritizing User Stories.
2. **Scrum Master:** A servant-leader acting as a coach. They do not manage the developers; instead, they ensure Scrum rules are understood, facilitate ceremonies, and aggressively remove impediments (blockers) hindering the team.
3. **Developers/Development Team:** A self-organizing, cross-functional group of professionals directly building the software.

**Three Primary Artifacts:**
1. **Product Backlog:** The master, ordered list of everything needed in the product. It is dynamic and constantly refined.
2. **Sprint Backlog:** The set of Product Backlog items selected for the current Sprint, plus a plan for delivering them.
3. **Increment (Working Software):** The sum of all Backlog items completed during a Sprint, which must be in a wholly usable state meeting the Definition of Done.

---

### Q4: Walk me through the four vital Scrum Ceremonies (Events).

**Answer:**
1. **Sprint Planning:** At the start of the Sprint, the team collaborates to answer two questions: *What* can be delivered in the Increment? And *How* will the work get done? Items are pulled from the Product backlog into the Sprint backlog.
2. **Daily Scrum (Stand-up):** A 15-minute timeboxed daily meeting for the Developers to inspect progress toward the Sprint Goal and adapt their plan. Usually answers: What did I do yesterday? What will I do today? Am I blocked?
3. **Sprint Review:** At the end of the Sprint, the team demonstrates the working Increment to stakeholders. Feedback is gathered to adapt the Product Backlog. It is not just a presentation, but a collaborative working session.
4. **Sprint Retrospective:** The final event inside a Sprint. The Scrum team inspects *how* they worked (processes, tools, relationships) and identifies at least one actionable improvement to implement in the very next Sprint.

---

### Q5: What is the hierarchy of Themes, Epics, Features, and User Stories? How do you write a good User Story?

**Answer:**
This hierarchy breaks massive organizational goals into actionable developer tasks.
- **Theme:** A large focus area spanning the organization (e.g., "Improve Application Security").
- **Epic:** A large body of work that cannot be completed in one Sprint. Must be broken down. (e.g., "Implement Multi-Factor Authentication").
- **Feature:** A distinct functionality providing business value.
- **User Story:** The smallest actionable piece of work. Designed to be completed by one or two developers within a single Sprint.

**Writing a good User Story:**
It should focus on the *User's value*, not the technical implementation. The standard template is:
*As a [persona/role], I want to [action/feature], so that [benefit/valuable outcome].*

*Example:* "As a *Mobile App User*, I want to *log in using FaceID*, so that *I can access my account faster without typing passwords*."

---

### Q6: Explain the INVEST criteria for evaluating User Stories.

**Answer:** The INVEST acronym outlines the characteristics of a high-quality User Story:

- **I (Independent):** The story should be self-contained and not inherently dependent on another story, allowing it to be prioritized freely.
- **N (Negotiable):** A story is a placeholder for a conversation, not a rigid contract. Details are negotiated during implementation.
- **V (Valuable):** It must deliver tangible value to the end-user or business. (A story like "Update DB Schema" lacks user value; it should be framed by what requires the schema change).
- **E (Estimable):** The team must understand it enough to provide a relative size estimate.
- **S (Small):** It should comfortably fit within a single sprint. If it’s too large, split it.
- **T (Testable):** There must be clear Acceptance Criteria allowing QA to verify if the story works.

---

### Q7: How does estimation work in Agile? Discuss Story Points and Planning Poker.

**Answer:** Agile shuns estimating in absolute time (hours/days) because humans are terrible at predicting time for complex, unknown tasks. Instead, Agile uses **relative estimation** via Story Points.

**Story Points** account for three factors:
1. Volume/Amount of work.
2. Complexity of the logic.
3. Risk or Uncertainty. 

Teams often use the modified Fibonacci sequence (1, 2, 3, 5, 8, 13) to reflect that larger tasks possess exponentially higher uncertainty. If a simple text update is a '1', creating an entirely new payment API might be an '8'.

**Planning Poker:**
A gamified consensus-based technique to reach an estimate.
1. The PO reads a user story and answers questions.
2. Each developer privately selects a physical or virtual card representing their estimate.
3. All cards are revealed simultaneously to prevent anchoring (senior devs influencing juniors).
4. Outliers (e.g., one person chose 2, another chose 13) discuss their perspectives. Maybe the '13' knows about a legacy database trigger the '2' doesn't.
5. They re-vote until consensus is reached.

---

### Q8: What is the difference between the Definition of Ready (DoR) and the Definition of Done (DoD)?

**Answer:** They act as the entry and exit gates for a Sprint.

**Definition of Ready (DoR) - The Entry Gate:**
The agreed-upon checklist a User Story must meet *before* it is allowed to be pulled into a sprint.
- *Examples:* Acceptance criteria are clearly written, UX designs are attached, dependencies are resolved, the team has estimated it.

**Definition of Done (DoD) - The Exit Gate:**
The comprehensive checklist a User Story must meet *before* it can be considered complete and ready to ship to production.
- *Examples:* Code pushed and reviewed via PR, Unit test coverage > 80%, CI build passes, integrated into the main branch, PO has accepted the functionality, security scan passes.

---

### Q9: Discuss Velocity and how it is used for forecasting. What is a "Velocity Anti-Pattern"?

**Answer:** 
**Velocity** is the total number of Story Points a team successfully completes (meets DoD) during a Sprint. 

**Forecasting:** By averaging the velocity over the last 3-4 sprints, a Product Owner can realistically predict how much work the team can consume in future sprints. If the backlog has 100 points of work and the team averages 20 points per sprint, the PO can forecast it will take 5 sprints to release.

**Velocity Anti-Patterns (Crucial for Seniors to know):**
1. **Using Velocity as a performance metric:** Velocity is a capacity planning tool, not a measure of productivity. If management pressures a team to "increase velocity," the team will simply inflate their estimates (point inflation), rendering the metric useless.
2. **Comparing Velocity across teams:** Team A's "5 points" is incredibly different from Team B's "5 points". They are relative isolated scales. You cannot compare them.

---

### Q10: How does Scrum differ from Kanban? Can they be combined?

**Answer:**
**Scrum:**
- Time-boxed into rigid Sprints.
- Roles are strictly defined (Scrum Master, PO).
- Work is locked in during Sprint Planning; adding scope mid-sprint is frowned upon.
- Velocity is the primary metric.

**Kanban:**
- Continuous flow of work; no fixed Sprints.
- Uses a visual board with columns representing states (To Do, Dev, Code Review, QA, Done).
- Enforces strict **Work In Progress (WIP) limits** per column. If the "Code Review" column hits its WIP limit, developers must stop coding new things and review code to unblock the flow.
- Cycle Time and Lead Time are the primary metrics.
- Highly adaptable; new urgent tasks can be prioritized instantly as soon as capacity opens up.

**Combination (Scrumban):** Teams often combine them, using Scrum's ceremonies for planning and reflection, but using Kanban's flow and WIP limits to manage daily execution.

---

### Q11: What is Technical Debt? How should a team identify, track, and manage it?

**Answer:** Coined by Ward Cunningham, Technical Debt is the implied cost of future rework caused by choosing an easy, fast (and suboptimal) solution now instead of a better, longer approach. Like financial debt, it accrues "interest"—the longer you leave it, the harder it becomes to add new features or fix bugs in that area of code.

**Management strategies for Seniors:**
1. **Identify & Track:** Do not let tech debt be an invisible complaint among developers. Make it visible. Create "Tech Debt" tickets in Jira and put them in the Product Backlog alongside feature requests.
2. **The Boy Scout Rule:** Leave the code cleaner than you found it. Do minor refactorings dynamically as part of standard feature work.
3. **Dedicated Capacity:** Negotiate with the Product Owner to allocate a percentage of every sprint's capacity (e.g., 20%) specifically for paying down Technical Debt. 
4. **Link to Business Value:** Stop saying "This code is ugly." Say "Migrating this legacy module will reduce average bug-fixing time by 30% and improve response latency, saving server costs." That gets PO buy-in.

---

### Q12: Explain CI/CD and its importance in an Agile environment.

**Answer:** Continuous Integration (CI) and Continuous Delivery/Deployment (CD) are the technical lifelines that make Agile's rapid delivery possible.

**Continuous Integration (CI):** 
The practice of developers merging their code changes into the central repository (main branch) very frequently (daily). Every merge triggers an automated pipeline that compiles the code and runs all unit/integration tests. 
*Importance:* Prevents "Integration Hell", where developers code in silos for weeks and fail miserably when trying to merge.

**Continuous Delivery (CDel):**
Extends CI by ensuring the codebase is always in a mathematically deployable state. It automatically pushes the verified code to testing/staging environments. Releasing to production requires a manual approval click.

**Continuous Deployment (CDep):**
Takes it further—every change that passes the automated pipeline is automatically pushed to production without human intervention.
*Importance:* Shrinks release cycles from months to minutes, enabling rapid feedback loops with customers.

---

### Q13: What does the term "Shift-Left Testing" mean?

**Answer:** Traditionally, in a waterfall model, testing happens at the very end of the cycle (on the "right" side of the timeline). This is dangerous because discovering structural bugs late is incredibly expensive.

**Shift-Left** means moving the testing activities earlier in the lifecycle to the "left."
- Developers write unit tests alongside code (TDD).
- QA engineers are involved during Requirement Analysis to highlight edge cases before code is even written.
- Security checks and static code analyzers (SonarQube) are integrated into the developers' IDEs and CI pipelines.
*Result:* Bugs are found when they cost pennies to fix, rather than thousands of dollars in production.

---

### Q14: How do you balance feature delivery requests from Product Owners with Non-Functional Requirements (Performance, Security, Scaling)?

**Answer:** This is a classic Senior Developer dilemma. Product Owners generally push for visible features; Non-Functional Requirements (NFRs) are invisible until they fail catastrophically.

**Strategy:**
1. **Define NFRs in the DoD:** Treat NFRs as first-class citizens. "Response time under 200ms" and "Zero critical SonarQube vulnerabilities" should be part of the Definition of Done. A feature isn't done until it performs well and is secure.
2. **Architecture Enablers:** Architect the system so that security and scaling are handled at the framework/infrastructure level (e.g., API Gateways for throttling, centralized Auth middleware) rather than requiring developers to reinvent it per feature.
3. **Data-Driven Compromise:** If a PO demands a feature instantly, explain the trade-offs using business risks. "We can hit the Friday deadline, but the system won't handle more than 50 concurrent users. Is the marketing blast small enough that this is acceptable? If not, we need until Wednesday."

---

### Q15: What is the role of a Senior Developer in an Agile team regarding mentoring and conflict resolution?

**Answer:** A Senior Developer in Agile is not a dictator; they are a multiplier.

**Mentoring:**
- Moving beyond "just giving the answer". Using Pair Programming and thoughtful Code Reviews to teach architectural thinking and design patterns.
- Encouraging juniors to lead Refinement sessions or present during Reviews to build their confidence.
- Allowing juniors to struggle safely.

**Conflict Resolution (Technical):**
Agile teams often disagree on implementation details. A Senior Developer resolves this by removing emotion and utilizing data. 
- If Dev A wants SQL and Dev B wants NoSQL, the Senior Developer doesn't vote based on tenure. They organize a time-boxed technical Spike to build a quick prototype of both, evaluate them against system requirements, and let the benchmark data dictate the decision. They foster a culture of "Strong opinions, loosely held."
