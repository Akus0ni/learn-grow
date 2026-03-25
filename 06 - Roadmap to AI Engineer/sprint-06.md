# Sprint 6: Multi-Agent Systems (Days 11-12)

> **Outcome:** Build Intelligent Multi-Agent AI Systems
> **Deliverable:** Multi-agent research & writing team with shared memory and collaboration

---

## Key Concepts

- Multi-agent architectures: supervisor, hierarchical, collaborative, debate
- Agent specialization: each agent has a focused role and toolset
- Shared memory: agents read from and write to a common knowledge base
- Inter-agent communication: how agents pass messages and delegate tasks
- Supervisor pattern: a "manager" agent routes tasks to specialist agents
- Conflict resolution: when agents disagree on an answer
- LangGraph for multi-agent: subgraphs, message passing, shared state

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `langgraph` | Multi-agent orchestration |
| `langchain` | Individual agent construction |
| `chromadb` | Shared memory / knowledge base |
| `langsmith` | Multi-agent tracing |
| `fastapi` | API backend |

---

## Day 11: Multi-Agent Architecture

### Morning (3-4 hrs) — Multi-Agent Patterns

**Tasks:**
1. **Multi-agent architectures (1.5 hrs)**
   - **Supervisor**: one agent delegates to specialists ("manager" model)
   - **Hierarchical**: supervisors manage sub-supervisors (org chart model)
   - **Collaborative**: agents discuss and reach consensus (roundtable model)
   - **Debate**: agents argue opposing viewpoints, a judge decides
   - When to use each: task routing (supervisor), complex organizations (hierarchical), quality (debate)
   - Draw architecture diagrams for each pattern

2. **Building specialist agents (1.5 hrs)**
   - Design 3 specialist agents:
     - `researcher` — uses web search to find information
     - `writer` — synthesizes information into well-written content
     - `reviewer` — evaluates quality, checks facts, suggests improvements
   - Each agent has its own system prompt, tools, and model (can use cheaper models for simpler agents)
   - Build each as a standalone LangGraph `StateGraph`

3. **Supervisor agent (1 hr)**
   - Build a supervisor that:
     - Receives a user task
     - Decides which specialist to invoke
     - Routes the task to the right agent
     - Collects the result
     - Decides: is the task done, or does another agent need to work on it?
   - Use LangGraph conditional edges for routing

### Afternoon (3-4 hrs) — Shared Memory & Communication

**Tasks:**
1. **Shared memory with ChromaDB (1.5 hrs)**
   - Create a shared ChromaDB collection all agents can read/write
   - Researcher writes findings to shared memory
   - Writer reads from shared memory to compose content
   - Reviewer reads the draft and writes feedback to shared memory
   - This is a persistent knowledge base that accumulates across conversations

2. **Inter-agent messaging (1 hr)**
   - LangGraph message passing: agents communicate via the shared state
   - Define message types: `task_assignment`, `result`, `feedback`, `approval`
   - Build a message log in the state that shows the full conversation between agents
   - Trace the multi-agent conversation in LangSmith

3. **Agent collaboration workflow (0.5 hrs)**
   - End-to-end flow:
     1. User: "Write a technical blog post about RAG systems"
     2. Supervisor assigns to Researcher
     3. Researcher searches, stores findings in shared memory
     4. Supervisor assigns to Writer
     5. Writer reads shared memory, writes a draft
     6. Supervisor assigns to Reviewer
     7. Reviewer reads draft, provides feedback
     8. Supervisor routes back to Writer for revision (if needed)
     9. Supervisor returns final output to user

---

## Day 12: Advanced Patterns & Sprint Deliverable

### Morning (3-4 hrs) — Advanced Multi-Agent Patterns

**Tasks:**
1. **Debate pattern (1 hr)**
   - Build 2 agents with opposing viewpoints on a topic
   - A judge agent evaluates their arguments
   - Useful for: reducing hallucination, exploring multiple perspectives
   - Example: "Should we use microservices?" — Agent A argues for, Agent B argues against, Judge synthesizes

2. **Subgraphs and composition (1 hr)**
   - LangGraph subgraphs: nest one graph inside another
   - Each specialist agent is a subgraph within the supervisor graph
   - Benefits: modularity, reusability, independent testing
   - Refactor the supervisor system to use subgraphs

3. **Error handling in multi-agent systems (1 hr)**
   - What happens when one agent fails? Fallback strategies
   - Timeout handling: if an agent takes too long
   - Retry with different approach: supervisor can re-prompt with adjusted instructions
   - Circuit breaker: after N failures, escalate to human

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: Multi-Agent Research & Writing Team**

A LangGraph multi-agent system that:
- Has a supervisor agent routing tasks to 3 specialists (researcher, writer, reviewer)
- Agents share memory via ChromaDB
- Produces high-quality written content through iterative collaboration
- Supports the debate pattern for controversial topics
- Full conversation between agents is visible in the output
- Traced in LangSmith for observability
- Exposed via FastAPI

**Build steps:**
1. Define shared state: `task`, `agent_messages`, `shared_memory_refs`, `current_agent`, `draft`, `feedback`, `final_output`
2. Build 3 specialist agent subgraphs
3. Build supervisor graph that orchestrates the specialists
4. Integrate ChromaDB as shared memory
5. Add the debate pattern as an optional mode (activated by user flag)
6. Build FastAPI endpoint: `POST /generate` with `{topic, mode: "collaborative" | "debate"}`
7. Add LangSmith tracing for the full multi-agent conversation
8. Test with 3 topics: a technical blog post, a comparison article, and a controversial topic (debate mode)

---

## Sprint 6 Checklist

- [ ] Can explain supervisor, hierarchical, collaborative, and debate multi-agent patterns
- [ ] Built specialist agents with distinct roles and toolsets
- [ ] Implemented a supervisor agent that routes tasks
- [ ] Set up shared memory with ChromaDB for inter-agent knowledge sharing
- [ ] Used LangGraph subgraphs for modular agent composition
- [ ] Implemented error handling and fallback strategies
- [ ] Deliverable: Multi-agent research & writing team
- [ ] Code pushed to GitHub

## Resources to Reference

- LangGraph docs — Multi-agent patterns, subgraphs, supervisor tutorial
- LangChain blog — multi-agent architectures
- Research papers: "Communicative Agents for Software Development" (ChatDev), "AutoGen" concepts
- LangSmith docs — tracing multi-agent workflows
