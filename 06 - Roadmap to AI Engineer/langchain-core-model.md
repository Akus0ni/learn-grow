# LangChain Core Model

> **Sprint 1, Day 1 — Topic 3 (1 hr)**
> Covers: provider abstraction, invocation methods, message types, and the provider swap exercise.

---

## Why LangChain's Model Abstraction Exists

Every LLM provider (OpenAI, Anthropic, Google, Meta via Ollama, etc.) has its own SDK, authentication scheme, request format, and response shape. Without an abstraction layer, switching providers means rewriting your integration code.

LangChain solves this with a **unified chat model interface**. Every chat model class — regardless of the underlying provider — exposes the same methods and accepts the same message types. You write your application logic once and swap the model with a one-line change.

---

## 1. Chat Model Classes: `ChatOpenAI` vs `ChatAnthropic` vs `ChatOllama`

All three are subclasses of `BaseChatModel`. They share the same public interface but connect to different backends.

### `ChatOpenAI`

Connects to OpenAI's API (GPT-4o, GPT-4o-mini, GPT-3.5-turbo, etc.).

```python
from langchain_openai import ChatOpenAI

llm = ChatOpenAI(
    model="gpt-4o-mini",   # model name
    temperature=0.7,        # randomness: 0 = deterministic, 2 = very random
    max_tokens=1024,        # max tokens in the response
    # api_key is read from OPENAI_API_KEY env var by default
)
```

**When to use:** Best default choice. Strong instruction following, wide model range (cheap mini to capable 4o), and well-documented.

---

### `ChatAnthropic`

Connects to Anthropic's API (Claude 3.5 Sonnet, Claude 3 Haiku, etc.).

```python
from langchain_anthropic import ChatAnthropic

llm = ChatAnthropic(
    model="claude-3-5-sonnet-20241022",
    temperature=0.7,
    max_tokens=1024,
    # api_key is read from ANTHROPIC_API_KEY env var by default
)
```

**When to use:** Preferred for long-context tasks (200k token window), nuanced reasoning, and when you need strong safety/refusal behavior. Often outperforms GPT-4o on coding and analysis tasks.

---

### `ChatOllama`

Connects to a **locally running** Ollama instance. No API key needed — everything runs on your machine.

```python
from langchain_ollama import ChatOllama

llm = ChatOllama(
    model="llama3.2",       # must be pulled first: `ollama pull llama3.2`
    temperature=0.7,
    # base_url defaults to http://localhost:11434
)
```

**When to use:** Privacy-sensitive data, offline development, zero API cost, or experimenting with open-source models (Llama, Mistral, Gemma, Phi, etc.).

---

### The Key Insight: Same Interface

```python
# All three are drop-in replacements for each other:
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic
from langchain_ollama import ChatOllama

llm_openai    = ChatOpenAI(model="gpt-4o-mini")
llm_anthropic = ChatAnthropic(model="claude-3-5-sonnet-20241022")
llm_ollama    = ChatOllama(model="llama3.2")

# Every line below works identically for all three:
response = llm_openai.invoke("What is 2 + 2?")
response = llm_anthropic.invoke("What is 2 + 2?")
response = llm_ollama.invoke("What is 2 + 2?")
```

---

## 2. Invocation Methods: `invoke()`, `stream()`, `batch()`

These are the three core ways to call a chat model. They all accept the same input types (a string, a list of messages, or a prompt value) and differ only in how they return results.

---

### `invoke()` — Single synchronous call

The simplest method. Send input, wait, get back a single `AIMessage`.

```python
from langchain_openai import ChatOpenAI

llm = ChatOpenAI(model="gpt-4o-mini")

response = llm.invoke("Explain recursion in one sentence.")
print(response.content)         # the text response
print(response.response_metadata)  # token usage, model name, finish reason
```

**Use when:** You need one response and the latency is acceptable. Most common method in learning and prototyping.

**Under the hood:** Makes a single POST request to the chat completions endpoint, waits for the full response, returns an `AIMessage` object.

---

### `stream()` — Token-by-token streaming

Returns a generator that yields `AIMessageChunk` objects as tokens arrive, rather than waiting for the full response.

```python
llm = ChatOpenAI(model="gpt-4o-mini")

for chunk in llm.stream("Write a haiku about Python."):
    print(chunk.content, end="", flush=True)

print()  # newline after stream ends
```

**Use when:**
- Building chat UIs where users should see text appear progressively (like ChatGPT's typing effect)
- Long responses where waiting feels slow
- You want to process content before the full response is ready

**Key difference from `invoke()`:** You get partial results immediately. The total time to completion is the same, but perceived latency is much lower.

```python
# Collecting the full streamed response into one string:
full_response = ""
for chunk in llm.stream("Tell me a short story."):
    full_response += chunk.content
    print(chunk.content, end="", flush=True)
```

---

### `batch()` — Multiple calls in parallel

Sends a list of inputs and returns a list of `AIMessage` responses. LangChain runs these concurrently under the hood.

```python
llm = ChatOpenAI(model="gpt-4o-mini")

questions = [
    "What is the capital of France?",
    "What is the capital of Japan?",
    "What is the capital of Brazil?",
]

responses = llm.batch(questions)

for q, r in zip(questions, responses):
    print(f"Q: {q}")
    print(f"A: {r.content}\n")
```

**Use when:**
- Processing many items at once (e.g., classifying a list of emails, summarizing multiple documents)
- You want to maximize throughput without writing your own async/threading code

**Performance note:** `batch()` respects rate limits automatically and is significantly faster than calling `invoke()` in a loop.

---

### Async Variants

All three methods have async counterparts for use in async applications:

```python
import asyncio
from langchain_openai import ChatOpenAI

llm = ChatOpenAI(model="gpt-4o-mini")

async def main():
    # Async invoke
    response = await llm.ainvoke("Hello!")

    # Async stream
    async for chunk in llm.astream("Tell me a joke."):
        print(chunk.content, end="", flush=True)

    # Async batch
    responses = await llm.abatch(["Question 1", "Question 2"])

asyncio.run(main())
```

---

### Quick Comparison

| Method    | Returns           | Best for                              |
|-----------|-------------------|---------------------------------------|
| `invoke()`  | Single `AIMessage` | Simple single-turn calls              |
| `stream()`  | Generator of `AIMessageChunk` | Chat UIs, long responses         |
| `batch()`   | List of `AIMessage` | Processing multiple inputs in parallel |

---

## 3. Message Types: `SystemMessage`, `HumanMessage`, `AIMessage`

LangChain's message objects map directly to the roles in a chat completion API call. Instead of passing raw strings, you pass structured objects that explicitly declare the speaker's role.

### Why typed messages matter

The LLM treats messages differently based on their role:
- **System:** Sets context, persona, constraints, and behavior. The LLM follows this as its operating instructions.
- **Human:** The user's input. What the LLM is responding to.
- **AI:** Previous responses from the model. Used to maintain conversation history.

---

### `SystemMessage`

Sets the model's behavior, persona, and constraints for the entire conversation. Typically the first message in a conversation.

```python
from langchain_core.messages import SystemMessage

system = SystemMessage(content="""
You are a senior Python engineer with 10 years of experience.
You give concise, precise answers with code examples.
You always mention edge cases and potential pitfalls.
""")
```

**Best practices:**
- Be specific. Vague system prompts produce inconsistent behavior.
- Define the persona, the output format, and any constraints.
- Keep it focused — don't cram too many instructions in.

---

### `HumanMessage`

Represents the user's input. This is what the LLM responds to.

```python
from langchain_core.messages import HumanMessage

human = HumanMessage(content="How do I read a file in Python safely?")
```

---

### `AIMessage`

Represents a previous response from the model. Used when you're constructing a conversation history manually.

```python
from langchain_core.messages import AIMessage

ai = AIMessage(content="Use a `with open(path, 'r') as f:` block — it auto-closes the file even if an exception occurs.")
```

---

### Using Messages Together

You pass a list of messages to `invoke()`, `stream()`, or `batch()`. The model sees the full conversation history and responds accordingly.

```python
from langchain_openai import ChatOpenAI
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

llm = ChatOpenAI(model="gpt-4o-mini")

messages = [
    SystemMessage(content="You are a helpful Python tutor. Be concise."),
    HumanMessage(content="What is a list comprehension?"),
    AIMessage(content="A list comprehension is a compact way to create a list: `[expr for item in iterable if condition]`."),
    HumanMessage(content="Can you show me an example with a condition?"),
]

response = llm.invoke(messages)
print(response.content)
```

The model sees the conversation so far and continues it naturally — it knows you already explained the basics and moves on to the example.

---

### `ChatPromptTemplate` — The Preferred Way to Build Messages

In practice, you rarely construct message objects manually. `ChatPromptTemplate` lets you define message templates with placeholders:

```python
from langchain_core.prompts import ChatPromptTemplate

prompt = ChatPromptTemplate.from_messages([
    ("system", "You are a {role}. Respond in {language}."),
    ("human", "{user_input}"),
])

# Fill in the placeholders:
filled = prompt.invoke({
    "role": "senior Python engineer",
    "language": "English",
    "user_input": "Explain list comprehensions.",
})

# filled is a ChatPromptValue — pass it directly to invoke():
response = llm.invoke(filled)
```

This is the building block of LCEL chains covered later in the sprint.

---

## 4. Exercise: Swap the Model Provider with One Line

This exercise makes the abstraction concrete. You write a function that works with any provider, then prove it by swapping without touching the logic.

```python
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic
from langchain_ollama import ChatOllama
from langchain_core.messages import SystemMessage, HumanMessage


def ask_model(llm, question: str) -> str:
    """Works with any LangChain chat model."""
    messages = [
        SystemMessage(content="You are a concise assistant. Answer in one sentence."),
        HumanMessage(content=question),
    ]
    response = llm.invoke(messages)
    return response.content


# --- Swap the provider by changing ONE line ---

# Option A: OpenAI
llm = ChatOpenAI(model="gpt-4o-mini", temperature=0)

# Option B: Anthropic (comment out A, uncomment B)
# llm = ChatAnthropic(model="claude-3-haiku-20240307", temperature=0)

# Option C: Local Ollama (comment out A, uncomment C)
# llm = ChatOllama(model="llama3.2", temperature=0)

# The rest of the code is identical regardless of provider:
question = "What is the difference between a list and a tuple in Python?"
answer = ask_model(llm, question)
print(f"Answer: {answer}")
```

**What to observe:**
- The `ask_model` function doesn't know or care which provider is being used
- Response quality and phrasing will differ between models — that's expected
- Latency will differ: Ollama is local (no network), OpenAI/Anthropic depend on API response time
- This is the core value proposition of LangChain's abstraction

---

### Extending the Exercise: Compare All Three

```python
import os
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic
from langchain_core.messages import SystemMessage, HumanMessage

question = "What is a Python decorator? Explain in two sentences."

providers = {
    "GPT-4o-mini": ChatOpenAI(model="gpt-4o-mini", temperature=0),
    "Claude Haiku": ChatAnthropic(model="claude-3-haiku-20240307", temperature=0),
}

# Uncomment if Ollama is running locally:
# from langchain_ollama import ChatOllama
# providers["Llama 3.2"] = ChatOllama(model="llama3.2", temperature=0)

messages = [
    SystemMessage(content="Be concise. Two sentences max."),
    HumanMessage(content=question),
]

for name, llm in providers.items():
    response = llm.invoke(messages)
    print(f"\n--- {name} ---")
    print(response.content)
```

---

## Key Takeaways

1. **Provider abstraction is the core value of LangChain's chat model layer.** Write once, swap freely.

2. **`invoke()` is your default.** Use `stream()` when building UIs or handling long outputs. Use `batch()` when processing many inputs at once.

3. **Messages have roles for a reason.** `SystemMessage` controls behavior, `HumanMessage` is user input, `AIMessage` is conversation history. The model uses role context to produce coherent responses.

4. **`ChatPromptTemplate` is the practical tool** for building reusable, parameterized prompts — it generates the message list for you.

5. **The one-line swap exercise** is the most important thing to internalize from this section: your application logic should be decoupled from the provider choice.

---

## What's Next

- **Prompt Engineering** (Day 1 Afternoon): zero-shot, few-shot, chain-of-thought using `ChatPromptTemplate`
- **Structured Output** (Day 1 Afternoon): `PydanticOutputParser` to enforce JSON schema on responses
- **LCEL** (Day 2): the `prompt | model | parser` pipe syntax that chains these components together
