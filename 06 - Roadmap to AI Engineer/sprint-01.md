# Sprint 1: LLM Fundamentals & Advanced Prompting (Days 1-2)

> **Outcome:** Advanced Prompting & LLM Model Mastery
> **Deliverable:** Multi-persona CLI chatbot with structured JSON output and prompt chaining

---

## Key Concepts

- What LLMs are: transformers, tokens, context windows, temperature, top-p
- API anatomy: chat completions, system/user/assistant roles, streaming
- Provider abstraction: why LangChain exists (swap OpenAI for Anthropic/Ollama in one line)
- Prompt engineering: zero-shot, few-shot, chain-of-thought, self-consistency
- Structured output: forcing JSON responses, output parsers
- Prompt chaining: sequential calls where output of one feeds the next
- Token economics: counting tokens, managing costs, context window limits

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `langchain` | Provider-agnostic LLM abstraction |
| `langchain-openai` | OpenAI chat model binding |
| `python-dotenv` | Environment variable management |
| `tiktoken` | Token counting |
| `pydantic` | Structured output validation |

---

## Day 1: Foundations & Environment Setup

### Morning (3-4 hrs) — Concepts & Setup

**Tasks:**
1. **Environment setup (1 hr)**
   - Install Python 3.11+, create a virtual environment
   - `pip install langchain langchain-openai python-dotenv tiktoken pydantic`
   - Create `.env` with your API key (`OPENAI_API_KEY=sk-...`)
   - Create project folder: `ai-engineer-sprints/sprint-01/`
   - Test a basic API call: "Hello, world" via LangChain's `ChatOpenAI`

2. **[LLM fundamentals](llm-fundamentals.md) (2 hrs)**
   - Read: What are transformers (conceptual, not math-heavy)
   - Understand: tokens, context windows, temperature, top-p, frequency penalty
   - Hands-on: Experiment with temperature 0 vs 1 on the same prompt
   - Hands-on: Use `tiktoken` to count tokens in different prompts
   - Understand: Chat message roles — system, user, assistant

3. **[LangChain core model](langchain-core-model.md) (1 hr)**
   - `ChatOpenAI` vs `ChatAnthropic` vs `ChatOllama` — same interface
   - `invoke()`, `stream()`, `batch()` methods
   - `SystemMessage`, `HumanMessage`, `AIMessage` objects
   - Exercise: swap the model provider with a single line change

### Afternoon (3-4 hrs) — Prompt Engineering Deep Dive

**Tasks:**
1. **Prompt patterns (2 hrs)**
   - Zero-shot: direct instruction ("Translate this to French")
   - Few-shot: provide examples before the question
   - Chain-of-thought: "Let's think step by step"
   - Role prompting: "You are a senior Python developer..."
   - Build a prompt template library for each pattern using `ChatPromptTemplate`

2. **Structured output (1.5 hrs)**
   - LangChain `PydanticOutputParser` — force JSON output matching a schema
   - Define a Pydantic model for the expected response
   - Build a prompt that instructs the LLM to output valid JSON
   - Parse and validate the response programmatically
   - Handle parsing errors gracefully

3. **Experimentation notebook (0.5 hrs)**
   - Create a script that runs the same question through 3 different prompt styles
   - Log results side-by-side for comparison
   - Note which patterns produce the best results

---

## Day 2: Prompt Chaining & Sprint Deliverable

### Morning (3-4 hrs) — Advanced Techniques

**Tasks:**
1. **Prompt chaining with LangChain (1.5 hrs)**
   - LCEL (LangChain Expression Language): `prompt | model | parser` pipe syntax
   - Sequential chains: output of chain A feeds into chain B
   - Example: Generate an outline -> Expand each section -> Summarize
   - Error handling in chains

2. **Conversation memory (1.5 hrs)**
   - `ConversationBufferMemory` — store full history
   - `ConversationSummaryMemory` — summarize older messages
   - `ConversationBufferWindowMemory` — sliding window of recent messages
   - When to use each: cost vs. context quality tradeoffs
   - Build a simple chat loop with memory

3. **Model comparison exercise (1 hr)**
   - If budget allows: run same prompts through GPT-4o-mini, GPT-4o
   - Compare: quality, latency, token usage, cost
   - Understand when to use small vs. large models

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: Multi-Persona CLI Chatbot**

A command-line chatbot that:
- Lets the user pick a persona (tutor, code reviewer, creative writer)
- Each persona has a tailored system prompt
- Maintains conversation memory across turns
- Returns structured JSON for code review responses (with fields: issues, suggestions, corrected_code)
- Uses prompt chaining: classify user intent -> route to appropriate handler -> format response

**Build steps:**
1. Define 3 persona system prompts in a config dict
2. Build LCEL chains for each persona
3. Add an intent classifier chain that picks the right persona
4. Wire up `ConversationBufferWindowMemory` (last 10 turns)
5. Build the CLI loop with `input()`, display formatted responses
6. Add structured output for the code reviewer persona using Pydantic
7. Test with at least 5 different conversation flows

---

## Sprint 1 Checklist

- [ ] Python environment working with LangChain
- [ ] Can explain: tokens, temperature, context windows, chat roles
- [ ] Built prompt templates for: zero-shot, few-shot, chain-of-thought
- [ ] Used `PydanticOutputParser` for structured JSON output
- [ ] Built LCEL chains with the pipe syntax
- [ ] Conversation memory working across turns
- [ ] Deliverable: Multi-persona CLI chatbot running locally
- [ ] Code pushed to GitHub

## Resources to Reference

- LangChain Python documentation — Chat Models, Prompts, Output Parsers, LCEL
- OpenAI API reference — Chat Completions
- Prompt engineering guides from OpenAI and Anthropic
- LangChain GitHub examples directory
