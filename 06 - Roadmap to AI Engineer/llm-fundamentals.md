# LLM Fundamentals

> Reference for Sprint 1 — Task 2: LLM Fundamentals (2 hrs)

---

## 1. What are Transformers?

Transformers are the neural network architecture that powers modern LLMs (GPT, Claude, Gemini, etc.). You don't need to understand the math, but the conceptual model matters.

**Key ideas:**

- **Attention mechanism** — the core innovation. Instead of reading text left-to-right like older models (RNNs), transformers look at *all* words simultaneously and learn which words "attend to" (are relevant to) each other.
  - Example: In "The animal didn't cross the street because *it* was too tired", attention helps the model learn that "it" refers to "animal", not "street".
- **Encoder / Decoder / Encoder-Decoder** — Three architectures:
  - *Encoder-only* (e.g., BERT): good for understanding/classification tasks
  - *Decoder-only* (e.g., GPT, Claude): good for text generation — predicts the next token
  - *Encoder-Decoder* (e.g., T5): good for translation, summarization
- **Pre-training + Fine-tuning** — LLMs are first pre-trained on massive text corpora (trillions of tokens) to learn language, then fine-tuned on instruction/conversation data to become helpful assistants (RLHF).
- **Emergent abilities** — as models scale, unexpected capabilities appear (multi-step reasoning, code generation) that weren't explicitly trained for.

**Mental model:** An LLM is a very sophisticated autocomplete engine. Given a sequence of tokens, it predicts the most likely next token — but at a scale and quality that produces coherent reasoning, code, and conversation.

---

## 2. Tokens

Tokens are the atomic unit LLMs process. They are *not* the same as words.

**How tokenization works:**
- Text is split into sub-word chunks using algorithms like BPE (Byte Pair Encoding)
- Common words are usually 1 token; rare/long words split into multiple tokens
- Spaces, punctuation, and capitalization are part of the token

**Rough estimates (OpenAI tokenizer):**
| Text | Approximate Tokens |
|------|--------------------|
| 1 average English word | ~1.3 tokens |
| 100 words | ~75 tokens |
| 1 page of text (~500 words) | ~375 tokens |
| 1,000 tokens | ~750 words |

**Examples:**
- `"hello"` → 1 token
- `"Hello,"` → 2 tokens (capital + comma matters)
- `"ChatGPT"` → 3 tokens: `Chat`, `G`, `PT`
- `"unbelievable"` → 3 tokens: `un`, `believ`, `able`

**Why tokens matter:**
- API costs are priced per token (input + output separately)
- Context window limits are measured in tokens
- Prompt length directly impacts latency and cost

**Hands-on — Count tokens with `tiktoken`:**
```python
import tiktoken

enc = tiktoken.encoding_for_model("gpt-4o")

texts = [
    "Hello, world!",
    "The quick brown fox jumps over the lazy dog.",
    "Explain quantum entanglement in simple terms.",
]

for text in texts:
    tokens = enc.encode(text)
    print(f"Text: {text!r}")
    print(f"Token count: {len(tokens)}")
    print(f"Tokens: {tokens}")
    print(f"Decoded tokens: {[enc.decode([t]) for t in tokens]}")
    print()
```

---

## 3. Context Window

The context window is the maximum number of tokens the model can "see" in a single call — both input and output combined.

**Current model limits (approximate):**
| Model | Context Window |
|-------|---------------|
| GPT-4o-mini | 128k tokens |
| GPT-4o | 128k tokens |
| Claude 3.5 Sonnet | 200k tokens |
| Claude 3 Haiku | 200k tokens |
| Gemini 1.5 Pro | 1M tokens |

**What counts toward the context:**
- System prompt
- All previous messages in the conversation (history)
- The current user message
- The model's response (output tokens)

**Context window implications:**
- Long conversations eventually exceed the window → you must truncate, summarize, or use memory strategies
- Larger context = higher cost per call
- "Lost in the middle" problem: models pay less attention to content in the middle of very long contexts — front-load important instructions

**Managing context:**
```python
# Naive approach — hits limit eventually
messages = []
messages.append({"role": "user", "content": user_input})
messages.append({"role": "assistant", "content": response})

# Sliding window approach — keep only last N turns
MAX_HISTORY = 10
messages = messages[-MAX_HISTORY * 2:]  # each turn = 2 messages
```

---

## 4. Temperature

Temperature controls the **randomness** of the model's output. It scales the probability distribution over possible next tokens before sampling.

| Temperature | Behavior | Use Case |
|-------------|----------|----------|
| `0.0` | Deterministic (always picks highest probability token) | Code generation, data extraction, factual Q&A |
| `0.1–0.3` | Very focused, minimal variation | Classification, structured output |
| `0.7` (default) | Balanced creativity and coherence | General conversation, writing |
| `1.0` | High creativity, more surprising outputs | Brainstorming, creative writing |
| `>1.0` | Very random, often incoherent | Rarely useful |

**Hands-on — Compare temperature 0 vs 1:**
```python
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage

prompt = "Give me a one-sentence description of a sunset."

for temp in [0.0, 0.0, 0.0, 1.0, 1.0, 1.0]:
    model = ChatOpenAI(model="gpt-4o-mini", temperature=temp)
    response = model.invoke([HumanMessage(content=prompt)])
    print(f"[temp={temp}] {response.content}")
```

Observation: `temperature=0` produces identical outputs on repeated calls. `temperature=1` produces different outputs each time.

---

## 5. Top-p (Nucleus Sampling)

Top-p is an alternative (or complement) to temperature for controlling output randomness.

**How it works:**
- Instead of scaling all probabilities (temperature), top-p considers only the smallest set of tokens whose cumulative probability exceeds `p`
- `top_p=0.1` → only considers tokens making up the top 10% of probability mass
- `top_p=1.0` → considers all tokens (no filtering)

**Temperature vs Top-p:**
- Use one or the other, not both simultaneously (OpenAI recommends this)
- Temperature: scales the whole distribution
- Top-p: cuts off the tail of low-probability tokens

**Common practice:**
- For deterministic outputs: `temperature=0` (top-p doesn't matter)
- For creative outputs: adjust `temperature`, leave `top_p=1.0`

```python
model = ChatOpenAI(
    model="gpt-4o-mini",
    temperature=0.7,
    model_kwargs={"top_p": 0.9}  # pass via model_kwargs in LangChain
)
```

---

## 6. Frequency Penalty & Presence Penalty

These parameters reduce repetition in generated text.

**Frequency penalty** (`-2.0` to `2.0`, default `0`):
- Penalizes tokens proportional to *how many times* they've already appeared
- Higher value → model avoids repeating words it has used a lot
- Use case: long-form content generation where you don't want repetitive phrasing

**Presence penalty** (`-2.0` to `2.0`, default `0`):
- Penalizes tokens based on *whether* they've appeared at all (binary)
- Higher value → model introduces new topics/vocabulary
- Use case: brainstorming, encouraging diverse ideas

```python
model = ChatOpenAI(
    model="gpt-4o-mini",
    frequency_penalty=0.5,   # reduce repetition
    presence_penalty=0.3,    # encourage topic variety
)
```

---

## 7. Chat Message Roles

Modern LLM APIs use a structured conversation format with three roles:

### System
- Sets the model's behavior, persona, constraints, and context
- Processed first, before user messages
- Not visible to the end user in most chat interfaces
- Think of it as the "director's instructions to an actor"

```python
system = """You are a senior Python developer with 10 years of experience.
You write clean, idiomatic Python. You always include type hints.
You explain your reasoning before showing code."""
```

### User
- The human's input — questions, instructions, data to process
- Can include anything: text, code, structured data

```python
user = "Write a function that validates an email address."
```

### Assistant
- The model's previous responses
- Used to maintain conversation history — you include prior turns so the model has context

```python
# Full conversation structure:
messages = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "What is 2+2?"},
    {"role": "assistant", "content": "4"},
    {"role": "user", "content": "Multiply that by 10."},  # model knows "that" = 4
]
```

**In LangChain:**
```python
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

messages = [
    SystemMessage(content="You are a helpful assistant."),
    HumanMessage(content="What is 2+2?"),
    AIMessage(content="4"),
    HumanMessage(content="Multiply that by 10."),
]

response = model.invoke(messages)
```

---

## 8. API Anatomy — Chat Completions

The Chat Completions API is the standard interface for LLMs. Understanding the raw request helps when working above it (LangChain, etc.).

**Raw OpenAI API request:**
```python
from openai import OpenAI

client = OpenAI()

response = client.chat.completions.create(
    model="gpt-4o-mini",
    messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Explain REST APIs in one paragraph."},
    ],
    temperature=0.7,
    max_tokens=300,      # max output tokens
    stream=False,        # set True for streaming
)

print(response.choices[0].message.content)
print(f"Input tokens: {response.usage.prompt_tokens}")
print(f"Output tokens: {response.usage.completion_tokens}")
print(f"Total tokens: {response.usage.total_tokens}")
```

**Streaming — receive tokens as they generate:**
```python
stream = client.chat.completions.create(
    model="gpt-4o-mini",
    messages=[{"role": "user", "content": "Write a haiku."}],
    stream=True,
)

for chunk in stream:
    delta = chunk.choices[0].delta.content
    if delta:
        print(delta, end="", flush=True)
```

**In LangChain:**
```python
from langchain_openai import ChatOpenAI

model = ChatOpenAI(model="gpt-4o-mini", streaming=True)

# invoke() — blocking, returns full response
response = model.invoke("Write a haiku.")

# stream() — yields chunks
for chunk in model.stream("Write a haiku."):
    print(chunk.content, end="", flush=True)
```

---

## 9. Token Economics

Understanding cost and limits prevents surprises in production.

> **Deep Dive:** For comprehensive pricing tables, rate limits, cost optimization strategies, and real-world cost scenarios across Anthropic and OpenAI APIs, see [Token Economics Deep Dive](token-economics-deep-dive.md).

**Pricing model (typical):**
- Charged separately for input tokens and output tokens
- Output tokens are usually 2–4x more expensive than input tokens
- Example: GPT-4o-mini at ~$0.15/1M input, $0.60/1M output

**Cost estimation:**
```python
import tiktoken

def estimate_cost(prompt: str, model: str = "gpt-4o-mini") -> dict:
    enc = tiktoken.encoding_for_model(model)
    input_tokens = len(enc.encode(prompt))

    # Rough pricing (check OpenAI for current rates)
    prices = {
        "gpt-4o-mini": {"input": 0.15, "output": 0.60},   # per 1M tokens
        "gpt-4o":      {"input": 2.50, "output": 10.00},
    }

    price = prices.get(model, prices["gpt-4o-mini"])
    input_cost = (input_tokens / 1_000_000) * price["input"]

    return {
        "input_tokens": input_tokens,
        "estimated_input_cost_usd": round(input_cost, 6),
    }

print(estimate_cost("Explain machine learning in detail " * 100))
```

**Context window management strategies:**

| Strategy | How | Tradeoff |
|----------|-----|----------|
| Sliding window | Keep last N turns | Loses early context |
| Summarization | Compress old turns into summary | Lossy, adds latency |
| Vector retrieval (RAG) | Fetch only relevant history | More complex setup |
| Token budget | Track tokens, truncate when near limit | Requires counting |

```python
import tiktoken

def fits_in_context(messages: list, model: str = "gpt-4o-mini", max_tokens: int = 128000) -> bool:
    enc = tiktoken.encoding_for_model(model)
    total = sum(len(enc.encode(m["content"])) for m in messages)
    total += len(messages) * 4  # overhead per message
    return total < max_tokens * 0.9  # leave 10% buffer for response
```

---

## 10. Provider Abstraction — Why LangChain Exists

Different providers (OpenAI, Anthropic, Google, Ollama) have similar but slightly different APIs. LangChain provides a unified interface.

**The problem without abstraction:**
```python
# OpenAI SDK
from openai import OpenAI
client = OpenAI()
response = client.chat.completions.create(model="gpt-4o-mini", messages=[...])
text = response.choices[0].message.content

# Anthropic SDK
import anthropic
client = anthropic.Anthropic()
response = client.messages.create(model="claude-3-haiku-20240307", messages=[...])
text = response.content[0].text
```

**With LangChain — one interface for all:**
```python
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic
from langchain_community.chat_models import ChatOllama

# Swap providers with a single line — everything else stays the same
# model = ChatOpenAI(model="gpt-4o-mini")
# model = ChatAnthropic(model="claude-3-haiku-20240307")
model = ChatOllama(model="llama3")

response = model.invoke("What is 2+2?")
print(response.content)
```

**When to use LangChain vs raw SDK:**
- Use LangChain when: building chains, using memory, output parsers, or planning to swap providers
- Use raw SDK when: simple one-off calls, or you need fine-grained control over the request

---

## Quick Reference Cheat Sheet

| Parameter | Range | Lower = | Higher = |
|-----------|-------|---------|----------|
| `temperature` | 0–2 | Deterministic | Creative/random |
| `top_p` | 0–1 | Focused | Diverse |
| `frequency_penalty` | -2–2 | Allow repetition | Reduce repetition |
| `presence_penalty` | -2–2 | Stay on topic | Explore new topics |
| `max_tokens` | 1–model limit | Short response | Long response |

| Role | Purpose |
|------|---------|
| `system` | Model behavior, persona, constraints |
| `user` | Human input |
| `assistant` | Previous model responses (conversation history) |

---

*Next: [Sprint 1 — Day 1 Afternoon: Prompt Engineering](sprint-01.md)*
