# Object-Oriented Analysis & Design and UML - Interview Q&A

> Sr. Software Engineer (6+ YoE) | C#/.NET Focus

---

### Q1: What is Object-Oriented Analysis and Design (OOAD), and how do the analysis and design phases differ?

**Answer:** OOAD is a methodology for building software systems by modeling them as collections of interacting objects. It proceeds through two major phases:

- **Object-Oriented Analysis (OOA)** -- Focuses on **what** the system must do. You identify the problem domain, discover key entities (objects), their attributes, behaviors, and relationships. The output is a **conceptual model** that is technology-agnostic. You are asking: "What are the real-world things we need to represent, and what are the business rules?"

- **Object-Oriented Design (OOD)** -- Focuses on **how** the system will do it. You take the analysis model and refine it into a solution that can be implemented: you assign responsibilities to classes, define interfaces, choose design patterns, specify data structures, and plan the architecture. Technology constraints (language, framework, persistence) are considered here.

**Key distinction for interviews:** Analysis models the problem; design models the solution. A common mistake is jumping straight to design without properly understanding the domain -- this leads to systems that are technically sound but solve the wrong problem.

A typical OOAD workflow:

```
Requirements --> OOA (Domain Model, Use Cases)
                   |
                   v
              OOD (Class Diagrams, Sequence Diagrams, Component Diagrams)
                   |
                   v
              Implementation (Code)
```

---

### Q2: What are the key OOAD principles -- abstraction, encapsulation, modularity, and hierarchy -- and how do they guide design decisions?

**Answer:** These four principles are the conceptual foundation of OOAD:

- **Abstraction** -- Focus on essential characteristics of an object while ignoring irrelevant details. In analysis, you abstract a real-world "Customer" down to just the attributes and behaviors relevant to the system. In design, you create interfaces or abstract classes that expose only what consumers need.

- **Encapsulation** -- Bundle data and behavior together and restrict direct access to internal state. This protects invariants and reduces coupling. In OOAD, you decide which operations are public vs. private and how objects communicate through well-defined interfaces.

- **Modularity** -- Decompose the system into cohesive, loosely-coupled modules (packages, assemblies, microservices). Each module has a clear boundary and responsibility. Modularity enables parallel development, independent deployment, and easier testing.

- **Hierarchy** -- Organize classes and modules into layered structures. This includes both **inheritance hierarchies** (is-a relationships) and **composition hierarchies** (has-a relationships). Hierarchy reduces complexity by letting you reason about things at different levels of abstraction.

**Senior-level insight:** These principles often create tension. For instance, deep inheritance hierarchies (hierarchy) can violate encapsulation if subclasses depend on superclass internals. A strong OOAD practitioner balances these principles and knows when to favor composition over inheritance, or when to trade some abstraction for performance.

---

### Q3: What is use-case-driven development, and how does it connect analysis to design?

**Answer:** Use-case-driven development means that **use cases are the primary artifact** that drives the entire development lifecycle -- from requirements elicitation through analysis, design, implementation, and testing.

**How it works in practice:**

1. **Elicit use cases** from stakeholders -- each use case describes a goal an actor wants to achieve with the system.
2. **Analyze each use case** to identify the domain objects (nouns), their responsibilities (verbs), and collaborations.
3. **Design interaction models** (sequence or collaboration diagrams) that show how objects work together to realize each use case.
4. **Derive the class diagram** by consolidating the objects, attributes, and methods discovered across all use cases.
5. **Implement and test** against the use case scenarios.

**Example use case (brief format):**

```
Use Case: Place Order
Actor: Customer
Precondition: Customer is logged in and has items in cart

Main Flow:
  1. Customer selects "Checkout"
  2. System displays order summary with total
  3. Customer enters shipping address
  4. Customer selects payment method
  5. System validates payment
  6. System creates order and sends confirmation

Alternative Flows:
  3a. Saved address exists --> System pre-fills address
  5a. Payment fails --> System notifies customer, returns to step 4
```

From this single use case you can extract candidate classes: `Customer`, `Cart`, `Order`, `OrderItem`, `ShippingAddress`, `PaymentMethod`, `PaymentService`. This is the bridge from analysis to design.

---

### Q4: What are the two main categories of UML diagrams, and when do you use each?

**Answer:** UML 2.5 defines **14 diagram types** organized into two categories:

**Structural Diagrams** -- Model the static architecture of the system (what things exist and how they relate):

| Diagram | Purpose |
|---------|---------|
| Class Diagram | Classes, attributes, methods, relationships |
| Object Diagram | Snapshot of instances at a point in time |
| Component Diagram | High-level modules and their dependencies |
| Package Diagram | Grouping of classes into packages/namespaces |
| Deployment Diagram | Physical nodes and artifact deployment |
| Composite Structure Diagram | Internal structure of a class |
| Profile Diagram | Extensions to the UML metamodel |

**Behavioral Diagrams** -- Model the dynamic behavior (what happens over time):

| Diagram | Purpose |
|---------|---------|
| Use Case Diagram | System functionality from the actor's perspective |
| Activity Diagram | Workflows and business processes (flowchart-like) |
| State Machine Diagram | Lifecycle states of an object |
| Sequence Diagram | Object interactions over time (ordered messages) |
| Communication Diagram | Object interactions with emphasis on links |
| Timing Diagram | Timing constraints on state changes |
| Interaction Overview Diagram | High-level flow of interaction fragments |

**Senior-level guidance:** You do not draw all 14 diagrams for every project. For most enterprise systems, the **core four** are: class diagrams, sequence diagrams, use case diagrams, and component diagrams. Choose diagrams based on what you need to communicate -- a state machine diagram is invaluable for modeling an `Order` lifecycle but overkill for a stateless utility class.

---

### Q5: Explain class diagram notation and the key types of relationships -- association, dependency, generalization, and realization.

**Answer:** A class in UML is drawn as a rectangle divided into three compartments:

```
+----------------------------+
|       <<stereotype>>       |
|        ClassName           |
+----------------------------+
| - privateField: Type       |
| # protectedField: Type     |
| + publicField: Type        |
+----------------------------+
| + publicMethod(): RetType  |
| - privateMethod(): void    |
| # protectedMethod(): int   |
+----------------------------+

Visibility: + public, - private, # protected, ~ package/internal
```

**Relationships:**

1. **Association** (solid line) -- A structural relationship where one class "knows about" another. Can be unidirectional or bidirectional, with multiplicity labels.

```
  Customer -------- Order
           1      0..*
  "A customer has zero or more orders"
```

2. **Aggregation** (open diamond) -- A "has-a" relationship where the part can exist independently of the whole.

```
  Department <>-------- Employee
             1         1..*
  "Employees can exist without the department"
```

3. **Composition** (filled diamond) -- A stronger "has-a" where the part cannot exist without the whole. If the whole is destroyed, the parts are destroyed.

```
  Order ◆-------- OrderLine
        1         1..*
  "OrderLines cannot exist without an Order"
```

4. **Dependency** (dashed arrow) -- A "uses" relationship, typically transient. One class uses another as a method parameter, local variable, or return type.

```
  OrderService - - - -> PaymentGateway
  "OrderService depends on PaymentGateway (uses it temporarily)"
```

5. **Generalization** (solid line with hollow triangle) -- Inheritance / "is-a" relationship.

```
  Shape
    ^
    |
  Circle
  "Circle is-a Shape"
```

6. **Realization** (dashed line with hollow triangle) -- A class implements an interface.

```
  <<interface>>
  IRepository
      ^
      :  (dashed)
      :
  SqlRepository
  "SqlRepository realizes IRepository"
```

**In C# terms:** Generalization maps to `: BaseClass`, realization maps to `: IInterface`, composition maps to a field that is created and owned within the constructor, and dependency maps to a parameter or local variable usage.

---

### Q6: How do you read and create sequence diagrams? Explain lifelines, messages, and activation bars.

**Answer:** A sequence diagram shows **how objects interact over time** to accomplish a specific scenario (often one use case flow).

**Key elements:**

- **Lifelines** -- Vertical dashed lines representing an object's existence over time. Each lifeline has a rectangle at the top showing the object name and class (e.g., `:OrderService`).

- **Messages** -- Horizontal arrows between lifelines representing method calls or signals:
  - **Synchronous** (solid arrow with filled head): caller waits for return.
  - **Asynchronous** (solid arrow with open head): caller does not wait.
  - **Return** (dashed arrow): the response coming back.
  - **Self-message** (arrow looping back to same lifeline): an object calling its own method.

- **Activation bars** -- Thin rectangles on lifelines showing when an object is active (executing a method).

**ASCII example -- Place Order scenario:**

```
  :Customer    :OrderController   :OrderService   :PaymentGateway    :OrderRepo
      |              |                  |                 |                |
      |--checkout()->|                  |                 |                |
      |              |--placeOrder()--->|                 |                |
      |              |                  |--validate()     |                |
      |              |                  |<--return ok--   |                |
      |              |                  |                 |                |
      |              |                  |--charge()------>|                |
      |              |                  |                 |---process()    |
      |              |                  |<--result--------|                |
      |              |                  |                 |                |
      |              |                  |--save()------------------------->|
      |              |                  |<--orderId------------------------|
      |              |<--confirmation---|                 |                |
      |<--200 OK-----|                  |                 |                |
```

**Senior-level tips:**

- Sequence diagrams are excellent for documenting the **happy path** and key **alternative flows** of complex use cases.
- Do not model every getter/setter -- focus on architecturally significant interactions.
- Use **combined fragments** (alt, loop, opt, par) to show conditionals and loops:
  - `alt` = if/else, `loop` = iteration, `opt` = optional, `par` = parallel execution.

---

### Q7: What are activity diagrams, and when are they preferred over sequence diagrams?

**Answer:** Activity diagrams model **workflows, business processes, and algorithmic logic** as a flow of activities. They are the UML equivalent of a flowchart but with support for concurrency (fork/join bars) and object flows.

**Key notation:**

- **Initial node** (filled circle): where the flow starts.
- **Activity/Action nodes** (rounded rectangles): steps in the process.
- **Decision nodes** (diamonds): branching based on conditions.
- **Merge nodes** (diamonds): converging branches.
- **Fork/Join bars** (thick horizontal bars): parallel execution.
- **Final node** (filled circle inside a circle): where the flow ends.
- **Swimlanes** (vertical or horizontal partitions): assign activities to actors or systems.

**ASCII example -- Order Fulfillment Process:**

```
         (*)  <-- initial node
          |
    [Receive Order]
          |
       <decision>
      /          \
  [In Stock]    [Out of Stock]
     |               |
  [Pick Items]  [Notify Customer]
     |               |
  [Pack Order]    (end)
     |
  ===FORK===          <-- parallel execution
  /         \
[Ship]    [Send Email]
  \         /
  ===JOIN===
     |
  [Mark Complete]
     |
    (O)  <-- final node
```

**When to use activity diagrams over sequence diagrams:**

| Activity Diagram | Sequence Diagram |
|------------------|------------------|
| Focus on **workflow/process** logic | Focus on **object interactions** |
| Shows **branching and parallelism** well | Shows **message ordering** and **timing** well |
| Good for business processes, algorithms | Good for API call flows, system integration |
| Actor-centric (swimlanes) | Object-centric (lifelines) |

Use activity diagrams when stakeholders need to understand the business process; use sequence diagrams when developers need to understand how objects collaborate.

---

### Q8: Explain state machine diagrams with a practical example.

**Answer:** A state machine diagram models the **lifecycle of a single object** -- the states it can be in, the events that trigger transitions, and any guard conditions or actions associated with those transitions.

**Key notation:**

- **State** (rounded rectangle): a condition the object is in.
- **Transition** (arrow): `event [guard] / action` -- what triggers the change, optional condition, and optional side effect.
- **Initial pseudo-state** (filled circle): entry point.
- **Final state** (filled circle inside circle): terminal state.
- **Composite state**: a state that contains sub-states (nested state machine).

**ASCII example -- Order Lifecycle:**

```
     (*)
      |
      v
  +----------+   pay()        +-----------+   ship()       +---------+
  |  Created  |-------------->|   Paid     |-------------->| Shipped  |
  +----------+                +-----------+                +---------+
      |                           |                            |
      | cancel()                  | cancel() / issueRefund()   | deliver()
      v                          v                             v
  +-----------+            +-----------+                 +------------+
  | Cancelled |            | Cancelled |                 | Delivered  |
  +-----------+            +-----------+                 +------------+
                                                              |
                                                              | return() [within 30 days]
                                                              v
                                                         +-----------+
                                                         | Returned  |
                                                         +-----------+
                                                              |
                                                              v
                                                             (O)
```

**C# implementation pattern -- State machines often map to the State pattern or a simple enum + switch:**

```csharp
public enum OrderState { Created, Paid, Shipped, Delivered, Cancelled, Returned }

public class Order
{
    public OrderState State { get; private set; } = OrderState.Created;

    public void Pay()
    {
        if (State != OrderState.Created)
            throw new InvalidOperationException($"Cannot pay in state {State}");
        State = OrderState.Paid;
    }

    public void Cancel()
    {
        if (State is not (OrderState.Created or OrderState.Paid))
            throw new InvalidOperationException($"Cannot cancel in state {State}");
        if (State == OrderState.Paid) IssueRefund();
        State = OrderState.Cancelled;
    }
}
```

**Senior-level insight:** State machine diagrams are invaluable for objects with complex lifecycle rules (orders, claims, tickets, workflows). If you find yourself writing deeply nested `if/else` chains on a status field, that is a strong signal to model it as an explicit state machine.

---

### Q9: What are component diagrams, and how do they fit into software architecture?

**Answer:** Component diagrams show the **high-level modular structure** of a system -- the major components (assemblies, services, libraries) and their dependencies. They sit at a higher abstraction level than class diagrams.

**Key notation:**

- **Component** (rectangle with the component icon or `<<component>>` stereotype): a modular, replaceable unit with well-defined interfaces.
- **Provided interface** (lollipop): an interface the component exposes to consumers.
- **Required interface** (socket): an interface the component depends on.
- **Dependency arrow** (dashed arrow): one component depends on another.
- **Port** (small square on component boundary): a specific interaction point.

**ASCII example -- E-Commerce System:**

```
+------------------+       +-------------------+       +------------------+
|  <<component>>   |       |   <<component>>   |       |  <<component>>   |
|   Web UI (SPA)   |------>| Order Service API  |------>| Payment Service  |
|                  |       |                   |       |                  |
+------------------+       +-------------------+       +------------------+
                                   |        |
                                   |        |
                                   v        v
                           +---------+  +------------------+
                           |   DB    |  |  <<component>>   |
                           | (SQL)   |  | Notification Svc |
                           +---------+  +------------------+

Provided Interfaces:
  Order Service API --O IOrderService, IInventoryQuery
  Payment Service   --O IPaymentGateway

Required Interfaces:
  Order Service API --C IPaymentGateway (from Payment Service)
  Order Service API --C INotification  (from Notification Svc)
```

**When to use component diagrams:**

- During **high-level design** to define system boundaries and integration points.
- When planning **microservice decomposition** -- each microservice is a component.
- To communicate architecture to stakeholders who do not need class-level detail.
- To document **provided and required interfaces** so teams can develop in parallel.

---

### Q10: How do you model use case diagrams, and what are include, extend, and generalization relationships?

**Answer:** A use case diagram captures **what the system does from the actors' perspective**. It does not show implementation details.

**Key elements:**

- **Actor** (stick figure): a person, system, or device that interacts with the system.
- **Use case** (oval): a goal an actor can accomplish.
- **System boundary** (rectangle): the scope of the system.

**Relationships between use cases:**

1. **Include** (`<<include>>`, dashed arrow from base to included) -- The base use case **always** invokes the included use case. Use this to extract common, mandatory behavior.

2. **Extend** (`<<extend>>`, dashed arrow from extending to base) -- The extending use case **optionally** adds behavior to the base use case under certain conditions. The base use case works fine without it.

3. **Generalization** (solid arrow with hollow triangle) -- One use case or actor is a specialized version of another.

**ASCII example -- Banking System:**

```
+-----------------------------------------------------------+
|                    Banking System                          |
|                                                           |
|   (Withdraw Cash)----<<include>>--->(Authenticate)        |
|        |                                ^                 |
|   (Deposit Cash)----<<include>>---------|                 |
|                                                           |
|   (Transfer Funds)---<<include>>--------|                 |
|        ^                                                  |
|        :                                                  |
|   <<extend>>                                              |
|        :                                                  |
|   (Send Transfer                                          |
|    Notification)                                          |
|        [condition: amount > $1000]                        |
|                                                           |
+-----------------------------------------------------------+
          |
     Customer
     (stick figure)

  Actor Generalization:
      Customer
         ^
         |
    Premium Customer  (inherits all use cases, may have additional ones)
```

**Common mistakes to avoid:**
- Do not use `<<include>>` for optional behavior -- that is `<<extend>>`.
- Do not decompose use cases into implementation steps (that is a functional decomposition anti-pattern).
- Keep use cases at the **user goal** level, not the sub-function level.

---

### Q11: How do you translate requirements into a class diagram?

**Answer:** This is a systematic process that bridges analysis and design:

**Step 1: Identify Candidate Classes (Noun Extraction)**
Read through requirements and use cases. Extract nouns as candidate classes, noun phrases as attributes, and verbs as methods or relationships.

```
Requirement: "A customer can place an order containing one or more products.
              Each order has a shipping address and a payment method."

Nouns:  Customer, Order, Product, ShippingAddress, PaymentMethod
Verbs:  place (method on Customer or OrderService)
```

**Step 2: Filter Candidates**
Remove nouns that are:
- Too vague (e.g., "system", "data")
- Attributes rather than classes (e.g., "name", "price" -- these go inside a class)
- Duplicates or synonyms
- Outside the system boundary

**Step 3: Identify Attributes and Methods**
For each surviving class, assign attributes (properties) and methods (responsibilities).

**Step 4: Establish Relationships and Multiplicity**

```
+-------------+        places         +----------+    contains     +----------+
|  Customer   |---------------------->|  Order   |<>-------------->| OrderLine|
+-------------+  1              0..*  +----------+  1        1..* +----------+
| - name      |                       | - date   |                | - qty    |
| - email     |                       | - status |                | - price  |
+-------------+                       +----------+                +----------+
                                      | 1    | 1                       | 1
                                      |      |                         |
                                      v      v                         v
                               +----------+ +----------+         +----------+
                               | Shipping | | Payment  |         | Product  |
                               | Address  | | Method   |         +----------+
                               +----------+ +----------+         | - name   |
                                                                 | - sku    |
                                                                 +----------+
```

**Step 5: Apply Design Principles**
- Is there duplication? Extract a base class or interface.
- Are responsibilities balanced? Apply SOLID principles.
- Is the coupling reasonable? Minimize dependencies.

**Step 6: Iterate**
Class diagrams are living documents. Refine as you build sequence diagrams and discover new interactions.

---

### Q12: How do you create a Low-Level Design (LLD) document?

**Answer:** An LLD is a detailed, developer-focused document that specifies **how** each module/component will be implemented. It should be detailed enough that any competent developer on the team can implement the feature correctly.

**Structure of a strong LLD:**

1. **Overview** -- Brief summary of the feature/module being designed and its scope.

2. **Class Diagram** -- Detailed class structure showing:
   - All classes with attributes, methods, visibility, and types
   - Relationships with multiplicity
   - Interfaces and abstract classes
   - Design patterns being applied (with rationale)

3. **Sequence Diagrams** -- For each significant operation:
   - Happy path flow
   - Key alternative and error flows
   - Show every layer: Controller -> Service -> Repository -> DB

4. **Data Model** -- Database schema:
   - Table definitions with columns, types, constraints
   - Foreign key relationships
   - Indexes (with justification)

5. **API Contracts** -- For each endpoint:
   - HTTP method, URL, request/response schemas
   - Error codes and messages
   - Authentication/authorization requirements

6. **State Diagrams** -- If any entity has a complex lifecycle.

7. **Error Handling Strategy** -- How exceptions propagate, retry policies, circuit breakers.

8. **Key Algorithms** -- Pseudocode or activity diagrams for non-trivial logic.

**Example -- LLD snippet for "Place Order":**

```
API:
  POST /api/orders
  Request:  { customerId, items: [{productId, qty}], shippingAddressId, paymentMethodId }
  Response: { orderId, status, total, estimatedDelivery }
  Errors:   400 (validation), 402 (payment failed), 404 (product not found), 409 (out of stock)

Classes:
  OrderController      --> receives HTTP request, delegates to service
  OrderService         --> orchestrates: validate, calculate, charge, persist, notify
  OrderValidator       --> checks stock, validates address
  PricingCalculator    --> computes subtotal, tax, shipping, discounts
  IPaymentGateway      --> interface for payment processing (Stripe, PayPal)
  IOrderRepository     --> interface for persistence
  OrderCreatedEvent    --> domain event published after successful creation
```

**Senior-level tip:** The best LLDs are concise but precise. Do not pad them with boilerplate. Focus on decisions that are non-obvious and could lead to bugs or inconsistencies if left ambiguous.

---

### Q13: How do you create a High-Level Design (HLD) document?

**Answer:** An HLD describes the system architecture at a level suitable for tech leads, architects, and cross-team stakeholders. It answers **what are the major building blocks** and **how do they interact**, without diving into class-level detail.

**Structure of a strong HLD:**

1. **Problem Statement & Goals** -- What business problem does this solve? What are the non-functional requirements (scalability, availability, latency targets)?

2. **Architecture Overview** -- A system context diagram or component diagram showing all major services, data stores, and external dependencies.

```
                    +-------------+
                    |   CDN /     |
   Users --------->| Load Balancer|
                    +------+------+
                           |
              +------------+------------+
              |                         |
      +-------v-------+        +-------v-------+
      |  Web App (UI) |        | Mobile BFF    |
      +-------+-------+        +-------+-------+
              |                         |
              +------------+------------+
                           |
                    +------v------+
                    |  API Gateway |
                    +------+------+
                           |
          +--------+-------+-------+--------+
          |        |               |        |
     +----v---+ +--v------+ +-----v--+ +---v-------+
     | Order  | | Payment | | Notif. | | Inventory |
     | Service| | Service | | Service| | Service   |
     +----+---+ +--+------+ +--------+ +---+-------+
          |        |                        |
     +----v---+ +--v------+           +----v------+
     |Order DB| |Payment  |           |Inventory  |
     |(SQL)   | |DB (SQL) |           |DB (NoSQL) |
     +--------+ +---------+           +-----------+
```

3. **Technology Stack** -- Language, framework, database, messaging, caching, deployment platform, with brief justification for each choice.

4. **Data Flow** -- How does data move through the system for key scenarios? Sequence diagrams at the service level.

5. **Data Storage Strategy** -- SQL vs. NoSQL decisions, sharding strategy, replication, backup/recovery.

6. **Integration Points** -- Third-party APIs, message queues, event buses. Protocols and data formats.

7. **Security Architecture** -- Authentication (OAuth2/JWT), authorization (RBAC/ABAC), encryption at rest and in transit, secrets management.

8. **Scalability & Reliability** -- Horizontal scaling strategy, caching layers, circuit breakers, retry policies, failover mechanisms.

9. **Deployment Architecture** -- CI/CD pipeline overview, deployment diagram (Kubernetes, cloud services).

10. **Key Trade-offs & Decisions** -- Document the alternatives considered and why specific choices were made (this is often the most valuable section for reviewers).

**Senior-level insight:** An HLD should be a decision log as much as a description. Future engineers will want to know **why** you chose SQL over NoSQL, not just that you did.

---

### Q14: What are CRC cards, and how are they used in object-oriented design?

**Answer:** CRC stands for **Class-Responsibility-Collaborator**. It is a brainstorming technique where each index card represents a class, listing what it does (responsibilities) and who it works with (collaborators).

**Card format:**

```
+------------------------------------------+
| Class Name: Order                        |
+------------------------------------------+
| Responsibilities:        | Collaborators:|
| - Calculate total        | OrderLine     |
| - Apply discount         | DiscountPolicy|
| - Validate items in stock| InventoryService|
| - Track order status     | OrderStatus   |
| - Process payment        | PaymentGateway|
+------------------------------------------+
```

**How to run a CRC session:**

1. **Gather the team** around a table with blank index cards.
2. **Pick a use case** and walk through it step by step.
3. **When a responsibility is identified**, write it on the appropriate class card. If no class owns it, create a new card.
4. **When a class needs help** from another, note the collaborator.
5. **Role-play**: Each person holds one or more cards and physically "passes messages" to act out the scenario.
6. **Refine**: Merge classes with overlapping responsibilities, split classes that have too many responsibilities (SRP violation).

**Why CRC cards matter for seniors:**

- They are **low-cost and high-collaboration** -- no tooling required, anyone can participate.
- They naturally surface **responsibility assignment** problems before you commit to a class diagram.
- They enforce thinking in terms of **behavior** (what does this class do?) rather than **data** (what fields does it have?), which leads to better object-oriented design.
- They are an excellent tool for onboarding junior developers into domain-driven thinking.

---

### Q15: What is domain modeling, and how do you approach it?

**Answer:** Domain modeling is the process of creating a **conceptual representation of the problem domain** -- the real-world entities, their attributes, relationships, and rules -- independent of any software implementation.

**Steps to build a domain model:**

1. **Identify domain concepts** by interviewing domain experts, reading requirements, and extracting nouns from use case descriptions.

2. **Establish associations** between concepts. Use verbs and verb phrases:
   - "Customer *places* Order"
   - "Order *contains* OrderLines"
   - "Product *belongs to* Category"

3. **Add attributes** to concepts (but not foreign keys or IDs -- those are implementation artifacts).

4. **Identify constraints and business rules** (e.g., "An order must have at least one item", "Discount cannot exceed 50%").

5. **Define multiplicity** on associations (1..1, 0..*, 1..*).

**Example domain model for a Library System:**

```
+----------+ borrows  +--------+ contains  +------+
| Member   |--------->| Loan   |---------->| Copy |
+----------+ 1   0..5 +--------+ 1      1  +------+
| name     |          | dueDate|            | cond.|
| memberNo |          | status |            +------+
+----------+          +--------+               |
                                            is-a-copy-of
                                               |
                                               v
                                          +--------+
                   +----------+  written  | Book   |
                   | Author   |---------->+--------+
                   +----------+ 1..*  1..*| title  |
                   | name     |           | isbn   |
                   +----------+           +--------+
                                              |
                                        belongs-to
                                              v
                                         +----------+
                                         | Category |
                                         +----------+

Business Rules:
  - A member can have at most 5 active loans
  - A loan period is 14 days, renewable once if no holds exist
  - A book can have multiple copies across branches
```

**Domain model vs. class diagram:**
- A domain model is a **conceptual** model -- no methods, no visibility modifiers, no implementation types.
- A class diagram is a **design** model -- includes methods, data types, interfaces, patterns.
- The domain model informs the class diagram but they are not the same artifact.

**Senior-level insight:** Domain modeling is the heart of Domain-Driven Design (DDD). Invest time building a **ubiquitous language** with your domain experts -- the terms in your domain model should be the exact terms used in code, conversations, and documentation. Misalignment here is a top source of bugs in complex systems.

---

### Q16: How do you make design trade-offs and justify architectural decisions?

**Answer:** Senior engineers are expected to evaluate competing concerns and make **reasoned, documented trade-offs**. Here is a structured approach:

**1. Identify the quality attributes in tension:**

Every design decision involves trade-offs between quality attributes:

| Attribute | Attribute | Tension |
|-----------|-----------|---------|
| Performance | Maintainability | Optimized code is often harder to read |
| Consistency | Availability | CAP theorem in distributed systems |
| Flexibility | Simplicity | Abstractions add indirection |
| Security | Usability | More auth steps slow the user down |
| Development Speed | Quality | Shortcuts create tech debt |

**2. Use a decision matrix when choices are complex:**

```
Decision: How to handle inter-service communication?

                | Sync (HTTP)  | Async (Message Queue) |
----------------|-------------|-----------------------|
Simplicity      | High        | Medium                |
Latency         | Low         | Higher (eventual)     |
Coupling        | Tight       | Loose                 |
Reliability     | Lower*      | Higher (retries/DLQ)  |
Debugging       | Easier      | Harder (distributed)  |
Scalability     | Limited     | High                  |

* If downstream service is down, caller fails immediately

Decision: Use async messaging for order processing (reliability + scalability
matter more), sync HTTP for user-facing reads (latency matters more).
```

**3. Document decisions using ADRs (Architecture Decision Records):**

```
ADR-007: Use Event Sourcing for Order Aggregate

Status: Accepted
Context: Orders go through many state transitions, and we need a complete
         audit trail for compliance. We also need to rebuild order state
         for analytics.
Decision: Implement event sourcing for the Order aggregate. Store events
          in an append-only event store. Maintain a read-optimized projection
          in SQL for queries.
Consequences:
  + Complete audit trail for free
  + Can rebuild state at any point in time
  + Natural fit for event-driven architecture
  - Higher complexity for developers unfamiliar with the pattern
  - Eventually consistent read model (acceptable for our SLA)
  - Need to handle event schema evolution
```

**Senior-level insight:** The sign of a strong engineer is not always picking the "best" technology -- it is picking the **right** technology for the constraints, documenting the rationale, and knowing when to revisit the decision. Trade-offs are context-dependent: what works for Netflix does not necessarily work for a three-person startup.

---

### Q17: How do you ensure traceability from requirements through design to implementation?

**Answer:** Traceability ensures that every requirement maps to design artifacts and ultimately to code and tests. Without it, you cannot confidently answer "have we built everything that was asked for?" or "why does this class exist?"

**Traceability chain:**

```
Requirement (User Story / FR)
    |
    v
Use Case (detailed scenario)
    |
    v
Analysis Model (domain classes, CRC cards)
    |
    v
Design Model (class diagrams, sequence diagrams)
    |
    v
Implementation (source code, namespaces, classes)
    |
    v
Test Cases (unit, integration, acceptance)
```

**Practical techniques:**

1. **Requirements Traceability Matrix (RTM):**

```
| Req ID  | Requirement         | Use Case     | Design Class      | Code Class          | Test Case  |
|---------|---------------------|--------------|-------------------|---------------------|------------|
| FR-001  | Customer places order | UC-PlaceOrder | OrderService      | OrderService.cs     | TC-001..005|
| FR-002  | Payment processing  | UC-PlaceOrder | PaymentGateway    | StripeGateway.cs    | TC-006..010|
| FR-003  | Order confirmation  | UC-PlaceOrder | NotificationService| EmailNotifier.cs   | TC-011..012|
```

2. **Naming alignment** -- Use the same terms from the domain model in class names, method names, and test names. If the use case says "Place Order," the method should be `PlaceOrder()`, not `CreateTransaction()`.

3. **Use case realization** -- For each use case, create a sequence diagram that shows which design classes participate. This is your bridge from "what" to "how."

4. **Tag or reference requirements in code:**
```csharp
/// <summary>
/// Handles order placement workflow.
/// Realizes: UC-PlaceOrder (FR-001, FR-002, FR-003)
/// </summary>
public class OrderService { ... }
```

**Senior-level insight:** In agile teams, heavy traceability matrices are often overkill. Instead, maintain lightweight traceability through well-named user stories, well-named classes, and acceptance tests that map one-to-one to acceptance criteria. The key is that someone can ask "where is requirement X implemented?" and get an answer in under a minute.

---

### Q18: How do you decide which UML diagrams to create for a given project, and how detailed should they be?

**Answer:** This is a judgment call that depends on the project's complexity, team size, and organizational culture. The goal of UML is **communication**, not compliance with a process.

**Decision framework:**

| Project Context | Recommended Diagrams | Level of Detail |
|----------------|---------------------|-----------------|
| Small feature, experienced team | Whiteboard class + sequence diagram (informal) | Just enough to align |
| New microservice | Component diagram, class diagram, sequence diagrams for key flows | Moderate -- focus on interfaces |
| Complex domain logic | Domain model, state machine, detailed class + sequence diagrams | High -- precision matters |
| System integration | Component diagram, deployment diagram, sequence diagrams across systems | High on boundaries, low on internals |
| Greenfield architecture | All of the above + use case diagram, activity diagrams | High -- this is the foundation |

**Principles for deciding:**

1. **Model with a purpose** -- Every diagram should answer a specific question. "How do services communicate?" -> component diagram. "What happens when a user places an order?" -> sequence diagram. If you cannot state the question, skip the diagram.

2. **Audience determines detail** -- CTO sees component diagrams. Developers see class and sequence diagrams. Business analysts see use case and activity diagrams.

3. **Iterate, do not waterfall** -- Start with rough sketches. Refine only the diagrams that prove useful as the design evolves. Discard diagrams that no one references.

4. **Minimize, do not maximize** -- The best design documentation is the smallest set of artifacts that prevents misunderstanding. Over-documentation is a maintenance burden and quickly becomes stale.

5. **Code is the ultimate truth** -- UML diagrams are communication tools, not contracts. If a diagram and the code disagree, the code wins. Keep diagrams at a level of abstraction where they remain stable despite minor code changes.

**Senior-level insight:** In practice, I typically create: (a) a component diagram for architectural context, (b) a class diagram for the core domain, (c) sequence diagrams for 2-3 critical workflows, and (d) a state machine diagram if any entity has a non-trivial lifecycle. Everything else is on-demand. The best documentation is the one that gets read.

---
