# What Is an AI Agent?

## The One-Line Definition

An AI Agent is **software that perceives its environment, makes decisions, and takes actions autonomously to achieve a goal** — without being told step-by-step what to do.

> Key insight: You give an agent a *goal*, not *instructions*.

---

## Traditional Software vs. AI Agent

| Dimension | Traditional Software | AI Agent |
|---|---|---|
| Input | Explicit instructions | A goal or task |
| Decision-making | Predefined rules / code | Reasoning engine (LLM, RL, etc.) |
| Adaptability | Fixed behavior | Adapts to context |
| Tool use | Hard-coded calls | Dynamically selects tools |
| Error handling | Crashes or pre-coded fallback | Replans and retries |

**Example:**
- Traditional: `if user says "book flight" → call BookingAPI()`
- Agent: *"Book me the cheapest flight to NYC next Friday"* → agent searches, compares prices, picks seats, and confirms — all on its own.

---

## The Agent Loop (How It Works)

Every AI agent runs a core loop:

```
Perceive → Think → Act → Observe → (repeat)
```

1. **Perceive** — Takes in input (user message, tool result, sensor data)
2. **Think** — Reasons about what to do next (planning, chain-of-thought)
3. **Act** — Calls a tool, writes output, or makes a decision
4. **Observe** — Gets the result of the action
5. **Repeat** — Until the goal is achieved or it gets stuck

This is often called the **ReAct loop** (Reasoning + Acting).

---

## Is It Always an LLM?

**No — but LLMs are the most common reasoning engine today.**

An AI agent's "brain" can be:

| Brain Type | Description | Example |
|---|---|---|
| **LLM** (GPT, Claude, Gemini) | Understands language, reasons, plans | ChatGPT plugins, Claude Code, Copilot |
| **Reinforcement Learning (RL)** | Learns through trial and reward signals | AlphaGo, game-playing bots, robotics |
| **Rule-based / Expert System** | Follows a decision tree or logic rules | Older chatbots, fraud detection |
| **Symbolic AI** | Logic, ontologies, formal reasoning | IBM Watson (early), medical diagnosis |
| **Hybrid** | LLM + RL + rules | Autonomous vehicles, Waymo |

### Why LLMs dominate *today*:
- They understand natural language goals out of the box
- They can reason across diverse domains without retraining
- They can decide *which tool to call* just from a description
- They can handle ambiguity and ask clarifying questions

But LLMs alone are **not enough**. An agent also needs:
- **Memory** (what happened before?)
- **Tools** (what can I do in the world?)
- **Planning** (how do I break this goal into steps?)

---

## The Four Core Components

```
┌─────────────────────────────────────────────┐
│                  AI AGENT                   │
│                                             │
│  ┌─────────┐   ┌────────┐   ┌───────────┐  │
│  │ Memory  │   │  LLM   │   │   Tools   │  │
│  │         │◄──│(Brain) │──►│ (Actions) │  │
│  │ Short   │   │        │   │           │  │
│  │ Long    │   └────────┘   │ Web Search│  │
│  │ Episodic│       ▲        │ Code Exec │  │
│  └─────────┘       │        │ APIs      │  │
│                    │        └───────────┘  │
│              ┌─────────┐                   │
│              │Planning │                   │
│              │(Goals → │                   │
│              │  Steps) │                   │
│              └─────────┘                   │
└─────────────────────────────────────────────┘
```

### 1. Memory
- **Short-term (context window):** Current conversation, recent tool results
- **Long-term (vector DB / files):** Past interactions, user preferences, knowledge base
- **Episodic:** Specific past experiences ("last time this API failed, I used the backup")

### 2. Planning
- Breaking a complex goal into sub-tasks
- Deciding the order of steps
- Replanning when something fails
- Techniques: **Chain-of-Thought**, **Tree-of-Thought**, **ReAct**, **MCTS**

### 3. Tools (Actions)
What the agent can *do* in the world:
- Web search
- Code execution
- File read/write
- API calls (calendar, email, databases)
- Browser automation
- Spawning other agents

### 4. The Reasoning Engine (LLM or otherwise)
- Interprets the goal
- Decides which tool to use
- Synthesizes results
- Communicates back to the user

---

## Real-World Examples

### Example 1: Research Agent
**Goal:** *"Summarize the top 5 AI papers from last week"*

```
1. Search arxiv for AI papers (last 7 days)
2. Rank by citation/novelty signals
3. For each top-5: fetch abstract, extract key findings
4. Synthesize into a structured summary
5. Return to user
```
No step-by-step instructions needed — the agent figures it out.

---

### Example 2: Customer Support Agent
**Goal:** *"A customer's order hasn't arrived after 10 days"*

```
1. Look up order status in CRM
2. Check shipping carrier API
3. Determine if delay is carrier-side or warehouse-side
4. If carrier: file a claim automatically
5. Draft a personalized apology email with resolution timeline
6. Escalate to human if order > $500
```
The agent uses multiple tools and makes judgment calls.

---

### Example 3: Software Engineering Agent (like Claude Code)
**Goal:** *"Add user authentication to this Express app"*

```
1. Read existing codebase structure
2. Identify where routes, middleware, and models are
3. Plan the changes needed (JWT, bcrypt, routes)
4. Write the code changes file by file
5. Run tests
6. Fix any failing tests
7. Report what was done
```

---

### Example 4: Non-LLM Agent — AlphaGo (RL-based)
**Goal:** *Win at Go*

- No language understanding — pure board state perception
- Reinforcement Learning: millions of self-play games
- Policy network decides moves, value network evaluates positions
- Zero natural language involved

This is still an AI agent — it perceives, reasons, and acts autonomously.

---

## Multi-Agent Systems

Agents can orchestrate *other* agents:

```
Orchestrator Agent
    ├── Research Agent     → searches the web
    ├── Analysis Agent     → crunches data
    ├── Writing Agent      → drafts content
    └── Review Agent       → checks for errors
```

**Why?** Each agent specializes. The orchestrator delegates and synthesizes. This mirrors how human teams work.

---

## Common Frameworks for Building Agents

| Framework | Language | Key Feature |
|---|---|---|
| **LangChain / LangGraph** | Python | Flexible graph-based agent flows |
| **AutoGen** (Microsoft) | Python | Multi-agent conversations |
| **CrewAI** | Python | Role-based agent teams |
| **Anthropic Agent SDK** | Python | Claude-native, tool use built-in |
| **OpenAI Assistants API** | Python/JS | Threads, tools, file search |
| **Semantic Kernel** | C# / Python | Enterprise-grade, Microsoft stack |

---

## When Should You Use an Agent (vs. a simple LLM call)?

| Use an Agent when... | Use a plain LLM call when... |
|---|---|
| Task requires multiple steps | Single question/answer |
| Need to use external tools/APIs | No external data needed |
| Goal is dynamic/ambiguous | Input/output is well-defined |
| Needs memory across sessions | Stateless is fine |
| Requires replanning on failure | One-shot is sufficient |

---

## Key Risks & Challenges

- **Hallucination:** Agent may confidently take wrong actions
- **Infinite loops:** Gets stuck replanning the same failed step
- **Tool misuse:** Calls the wrong API or with wrong parameters
- **Cost:** Many LLM calls per task = expensive
- **Trust & safety:** Agent has real-world side effects (emails sent, money spent)

**Mitigation:** Human-in-the-loop checkpoints, tool sandboxing, output validation, budget limits.

---

## TL;DR

| Question | Answer |
|---|---|
| What is an AI Agent? | Software that autonomously pursues goals by reasoning and acting |
| Is it always an LLM? | No — but LLMs are the dominant brain today |
| What makes it an *agent* (not just LLM)? | Memory + Tools + Planning + a feedback loop |
| Real example? | Claude Code, GitHub Copilot Workspace, AutoGPT, AlphaGo |
| When to use? | Multi-step tasks, tool use, dynamic replanning |
