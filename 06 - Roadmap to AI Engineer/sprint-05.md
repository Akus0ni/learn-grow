# Sprint 5: Autonomous AI Agents (Days 9-10)

> **Outcome:** Building Autonomous AI Agents
> **Deliverable:** Task-planning agent with tool execution, state management, and monitoring dashboard

---

## Key Concepts

- Agent architectures: ReAct, Plan-and-Execute, Reflexion
- LangGraph: graph-based agent orchestration (successor to AgentExecutor)
- State machines: modeling agent workflows as graphs with nodes and edges
- Tool execution: reliable tool calling with validation and error recovery
- Human-in-the-loop: agent pauses for approval before critical actions
- Agent observability: tracing, logging, monitoring agent decisions
- Guardrails: preventing agents from going off-track

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `langgraph` | Graph-based agent framework |
| `langchain` | Tools, prompts, models |
| `langsmith` | Agent tracing and observability (free tier) |
| `tavily-python` | Web search tool |
| `fastapi` | Agent API backend |

---

## Day 9: Agent Architectures & LangGraph

### Morning (3-4 hrs) — Agent Theory & LangGraph Basics

**Tasks:**
1. **Agent architectures compared (1 hr)**
   - ReAct: think-act-observe loop (what you built in Sprint 2)
   - Plan-and-Execute: make a full plan first, then execute each step
   - Reflexion: execute, evaluate, reflect, improve
   - When to use each: simple tasks (ReAct), complex multi-step (Plan-and-Execute), quality-critical (Reflexion)

2. **LangGraph fundamentals (2 hrs)**
   - Why LangGraph over AgentExecutor: more control, explicit state, better debugging
   - Core concepts: `StateGraph`, nodes, edges, conditional edges
   - `State` — a TypedDict that flows through the graph
   - Nodes — functions that read state, do work, return updated state
   - Edges — connect nodes, can be conditional (route based on state)
   - Build a simple 3-node graph: `input -> process -> output`
   - Visualize the graph with `.get_graph().draw_mermaid()`

3. **ReAct agent in LangGraph (1 hr)**
   - `create_react_agent()` from `langgraph.prebuilt`
   - Compare to the Sprint 2 AgentExecutor approach
   - Key difference: LangGraph gives you the state at every step
   - Exercise: rebuild the Sprint 2 research agent using LangGraph

### Afternoon (3-4 hrs) — Advanced Agent Patterns

**Tasks:**
1. **Plan-and-Execute agent (1.5 hrs)**
   - Build a custom LangGraph agent:
     - Node 1: `planner` — given a task, output a list of steps
     - Node 2: `executor` — execute one step using tools
     - Node 3: `replanner` — check progress, update plan if needed
     - Conditional edge: if all steps done -> end, else -> executor
   - Test with: "Research 3 AI frameworks, compare them, and write a summary report"

2. **Human-in-the-loop (1 hr)**
   - Add an `approval` node: before executing dangerous tools, ask the user
   - LangGraph `interrupt_before` — pause graph execution at a specific node
   - Resume execution after user confirms
   - Use case: agent wants to send an email or delete a file -> ask first

3. **State persistence (0.5 hrs)**
   - `MemorySaver` checkpointer: save agent state to memory
   - Resume an agent from a checkpoint (useful for long-running tasks)
   - Thread-based conversations: each user gets their own agent state

---

## Day 10: Observability & Sprint Deliverable

### Morning (3-4 hrs) — Monitoring & Production Patterns

**Tasks:**
1. **LangSmith tracing (1.5 hrs)**
   - Sign up for LangSmith (free tier)
   - Set environment variables: `LANGCHAIN_TRACING_V2=true`, `LANGCHAIN_API_KEY=...`
   - Run your agent — every step is traced automatically
   - Explore the LangSmith UI: see reasoning, tool calls, latency, tokens
   - Debug a failed agent run by examining the trace

2. **Agent guardrails (1 hr)**
   - Max iterations: prevent infinite loops
   - Output validation: check agent output against a schema
   - Token budget: track cumulative token usage, stop if too expensive
   - Tool restrictions: only allow certain tools in certain contexts
   - Build a wrapper that enforces these guardrails

3. **Streaming agent output (0.5 hrs)**
   - `agent.astream_events()` — stream every event from the agent graph
   - Events include: LLM tokens, tool calls, tool results, state updates
   - Useful for building real-time monitoring UIs

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: Task-Planning Agent with Monitoring**

A LangGraph-based agent that:
- Accepts a complex task from the user
- Creates a multi-step plan
- Executes each step using tools (web search, calculator, file writer)
- Can replan if a step fails
- Supports human-in-the-loop approval for critical steps
- Traced via LangSmith for full observability
- Exposed via FastAPI with streaming responses

**Build steps:**
1. Define the state schema: `task`, `plan`, `current_step`, `results`, `status`
2. Build nodes: `planner`, `executor`, `replanner`, `human_approval`, `reporter`
3. Define edges: planner -> executor -> replanner -> (conditional: done or executor)
4. Add tools: web search, calculator, save-to-file
5. Add human-in-the-loop before file-write operations
6. Add LangSmith tracing
7. Build FastAPI endpoint with streaming events
8. Build a simple CLI that shows plan progress in real-time
9. Test with 3 complex tasks, verify traces in LangSmith

---

## Sprint 5 Checklist

- [ ] Can explain ReAct vs. Plan-and-Execute vs. Reflexion architectures
- [ ] Built a LangGraph `StateGraph` with nodes, edges, and conditional routing
- [ ] Implemented a Plan-and-Execute agent pattern
- [ ] Added human-in-the-loop approval flow
- [ ] Set up LangSmith tracing and explored the dashboard
- [ ] Implemented agent guardrails (max iterations, token budget)
- [ ] Deliverable: Task-planning agent with monitoring
- [ ] Code pushed to GitHub

## Resources to Reference

- LangGraph documentation — StateGraph, nodes, edges, checkpointing
- LangGraph tutorials — ReAct agent, Plan-and-Execute
- LangSmith documentation — setup, tracing, evaluation
- LangChain blog posts on agent architectures
