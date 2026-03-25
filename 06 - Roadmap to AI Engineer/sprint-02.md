# Sprint 2: Workflow Automation & API Integration (Days 3-4)

> **Outcome:** Workflow Automation & API Integrations
> **Deliverable:** AI-powered research assistant that searches the web, summarizes findings, and saves reports

---

## Key Concepts

- Function calling / tool use: letting the LLM decide which function to call
- LangChain Tools: wrapping Python functions as tools the LLM can invoke
- Agents vs. chains: chains are fixed pipelines, agents decide their own path
- ReAct pattern: Reasoning + Acting in a loop
- API integration: connecting LLMs to external services
- Output handling: parsing tool results back into the conversation
- Error handling and fallbacks in automated workflows

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `langchain` | Agent and tool framework |
| `langchain-community` | Community tool integrations |
| `tavily-python` | Web search API (free tier) |
| `requests` | HTTP API calls |
| `fastapi` | Expose workflow as an API endpoint |
| `uvicorn` | ASGI server for FastAPI |

---

## Day 3: Tool Use & Function Calling

### Morning (3-4 hrs) — Concepts & Guided Exercises

**Tasks:**
1. **Function calling fundamentals (1.5 hrs)**
   - How LLMs "call functions": they output structured JSON, your code executes it
   - OpenAI function calling format vs. LangChain's abstraction
   - Define a simple tool: `get_weather(city: str) -> str`
   - Bind it to the model with `model.bind_tools([tool])`
   - Parse the `AIMessage.tool_calls` response

2. **LangChain Tools framework (1.5 hrs)**
   - `@tool` decorator: turn any Python function into a LangChain tool
   - Tool schemas: name, description, args_schema (Pydantic model)
   - Why descriptions matter: the LLM reads them to decide which tool to use
   - Build 3 custom tools:
     - `calculator(expression: str)` — evaluates math
     - `get_current_time(timezone: str)` — returns current time
     - `save_to_file(filename: str, content: str)` — writes to disk

3. **ReAct agents (1 hr)**
   - `create_react_agent()` — LangChain's agent constructor
   - The agent loop: think -> pick a tool -> call it -> observe -> think again
   - Trace through an agent run: see every reasoning step
   - `AgentExecutor` — runs the agent with max iterations and error handling

### Afternoon (3-4 hrs) — API Integration

**Tasks:**
1. **Web search integration (1.5 hrs)**
   - Sign up for Tavily API (free tier: 1000 searches/month)
   - Use `TavilySearchResults` tool from LangChain
   - Build an agent that can search the web and summarize findings
   - Experiment: ask it factual questions, see it search and synthesize

2. **Chaining multiple tools (1.5 hrs)**
   - Give the agent access to: web search + calculator + save-to-file
   - Test complex queries: "Search for NVIDIA stock price, calculate 10% growth, save a report"
   - Observe how the agent plans and sequences tool calls
   - Handle cases where the agent gets stuck (max iterations, parsing errors)

3. **Building a FastAPI wrapper (1 hr)**
   - Create a `/query` POST endpoint that accepts a user question
   - Run the agent inside the endpoint
   - Return the agent's final answer as JSON
   - Test with curl or Postman

---

## Day 4: Complex Workflows & Sprint Deliverable

### Morning (3-4 hrs) — Advanced Patterns

**Tasks:**
1. **Sequential workflow chains (1.5 hrs)**
   - Design a multi-step workflow:
     - Step 1: User provides a research topic
     - Step 2: Agent searches for 3 sources
     - Step 3: Agent summarizes each source
     - Step 4: Agent writes a combined report
     - Step 5: Agent saves the report to a file
   - Implement using LCEL `RunnableSequence` with tool-calling at each step

2. **Error handling and fallbacks (1 hr)**
   - `model.with_fallbacks([backup_model])` — use a cheaper model if primary fails
   - Retry logic with `max_iterations` on the agent
   - Handling rate limits, API timeouts, malformed tool outputs
   - Logging: capture every agent step for debugging

3. **Streaming and callbacks (0.5 hrs)**
   - Stream agent thinking in real-time to the console
   - LangChain callbacks: log token usage, latency, tool calls
   - Useful for monitoring costs during development

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: AI Research Assistant**

A Python CLI + FastAPI app that:
- Accepts a research topic from the user
- Searches the web using Tavily for relevant sources
- Summarizes each source individually
- Generates a structured research report (title, summary, key findings, sources)
- Saves the report as a markdown file
- Exposes the workflow as a REST API via FastAPI

**Build steps:**
1. Define tools: `web_search`, `summarize_text`, `save_report`
2. Build a ReAct agent with all tools bound
3. Create a prompt that instructs the agent to follow the research workflow
4. Add `ConversationBufferMemory` so follow-up questions reference prior research
5. Wrap in a CLI interface for interactive use
6. Wrap in a FastAPI `/research` endpoint for API access
7. Test with 3 different research topics
8. Save example reports to demonstrate the output

---

## Sprint 2 Checklist

- [ ] Understand function calling / tool use pattern
- [ ] Built custom tools with `@tool` decorator
- [ ] Built a ReAct agent with `create_react_agent()`
- [ ] Integrated web search via Tavily
- [ ] Built multi-step workflows with error handling
- [ ] Created a FastAPI endpoint wrapping the agent
- [ ] Deliverable: AI Research Assistant (CLI + API)
- [ ] Code pushed to GitHub

## Resources to Reference

- LangChain docs — Tools, Agents, AgentExecutor, LCEL
- OpenAI docs — Function Calling
- Tavily API documentation
- FastAPI official tutorial
- LangChain cookbook — agent examples
