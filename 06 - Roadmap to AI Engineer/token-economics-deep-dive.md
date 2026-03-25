# Token Economics Deep Dive

> In-depth guide to understanding costs, limits, and optimization strategies for Anthropic Claude and OpenAI APIs.

---

## 1. Why Token Economics Matter

Every API call to an LLM costs money. In production systems handling thousands of requests per day, small inefficiencies compound into significant costs. Understanding token economics helps you:

- **Budget accurately** before building
- **Architect efficiently** — choose the right model for each task
- **Avoid production surprises** — rate limits, unexpected bills, throttling
- **Optimize costs** — caching, batching, prompt engineering

---

## 2. How Pricing Works

Both Anthropic and OpenAI charge **per token**, with separate rates for:

| Component | Description |
|-----------|-------------|
| **Input tokens** | Everything you send: system prompt, conversation history, user message, tool definitions |
| **Output tokens** | Everything the model generates in response |

Output tokens are **always more expensive** than input tokens (typically 3–5x), because generation is more compute-intensive than reading.

---

## 3. Current Pricing Breakdown

### Anthropic Claude Models (March 2026)

| Model | Input (per 1M tokens) | Output (per 1M tokens) | Context Window | Best For |
|-------|----------------------|------------------------|----------------|----------|
| **Claude Opus 4.6** | $5.00 | $25.00 | 1M tokens | Complex reasoning, research, coding |
| **Claude Sonnet 4.6** | $3.00 | $15.00 | 1M tokens | Balanced performance/cost |
| **Claude Haiku 4.5** | $1.00 | $5.00 | 200k tokens | Fast, high-volume, simple tasks |

**Anthropic cost-saving features:**

| Feature | Discount | Details |
|---------|----------|---------|
| **Batch API** | 50% off input + output | Asynchronous processing, results within 24 hours |
| **Prompt Caching (5-min TTL)** | Cache write: 1.25x input price, Cache read: 0.1x input price | Pays off after just 1 cache read |
| **Prompt Caching (1-hr TTL)** | Cache write: 2x input price, Cache read: 0.1x input price | Pays off after 2 cache reads |

### OpenAI Models (March 2026)

| Model | Input (per 1M tokens) | Output (per 1M tokens) | Context Window | Best For |
|-------|----------------------|------------------------|----------------|----------|
| **GPT-4.1** | $2.00 | $8.00 | 1M tokens | Long-context tasks, coding |
| **GPT-4.1 mini** | $0.40 | $1.60 | 1M tokens | Cost-efficient for simple tasks |
| **GPT-4.1 nano** | $0.10 | $0.40 | 1M tokens | Cheapest, classification/extraction |
| **GPT-4o** | $2.50 | $10.00 | 128k tokens | Multimodal (text + vision + audio) |
| **GPT-4o-mini** | $0.15 | $0.60 | 128k tokens | Fast, cheap, multimodal |
| **o3** | $2.00 | $8.00 | 200k tokens | Advanced reasoning |
| **o3-mini** | $1.10 | $4.40 | 200k tokens | Cost-efficient reasoning |

**OpenAI cost-saving features:**

| Feature | Discount | Details |
|---------|----------|---------|
| **Batch API** | 50% off input + output | Async, results within 24 hours |
| **Cached Input** | 50% off input tokens | Automatic for repeated prefixes |

---

## 4. Head-to-Head Cost Comparison

### Cost per 1,000 requests (assuming 500 input + 500 output tokens each)

| Scenario | Claude Haiku 4.5 | GPT-4o-mini | Claude Sonnet 4.6 | GPT-4.1 | Claude Opus 4.6 | GPT-4o |
|----------|------------------|-------------|---------------------|---------|------------------|--------|
| **Input cost** | $0.0005 | $0.000075 | $0.0015 | $0.001 | $0.0025 | $0.00125 |
| **Output cost** | $0.0025 | $0.0003 | $0.0075 | $0.004 | $0.0125 | $0.005 |
| **Total per 1K requests** | $0.003 | $0.000375 | $0.009 | $0.005 | $0.015 | $0.00625 |

### Monthly cost estimate (100K requests/day, 500+500 tokens each)

| Model | Monthly Cost (approx.) |
|-------|----------------------|
| GPT-4o-mini | ~$11 |
| Claude Haiku 4.5 | ~$90 |
| GPT-4.1 | ~$150 |
| Claude Sonnet 4.6 | ~$270 |
| GPT-4o | ~$188 |
| Claude Opus 4.6 | ~$450 |

> These are rough estimates. Real-world costs depend heavily on actual prompt/response lengths, caching, and batching.

---

## 5. Rate Limits

Rate limits prevent any single user from overloading the API. Both providers use **tiered systems** — the more you spend, the higher your limits.

### Anthropic Claude Rate Limit Tiers

| Tier | Requirement | RPM | Input TPM | Output TPM |
|------|------------|-----|-----------|------------|
| **Tier 1** | $5 credit purchase | 50 | 30,000 | 10,000 |
| **Tier 2** | $40 cumulative | 1,000 | 80,000 | 40,000 |
| **Tier 3** | $200 cumulative | 2,000 | 400,000 | 80,000 |
| **Tier 4** | $400 cumulative | 4,000 | 2,000,000 | 400,000 |

**Key detail:** With prompt caching, only *uncached* input tokens count against your ITPM limit. An 80% cache hit rate effectively gives you 5x your stated ITPM capacity.

### OpenAI Rate Limit Tiers

| Tier | Requirement | RPM | TPM (varies by model) |
|------|------------|-----|-----------------------|
| **Free** | Default | 3–20 | 40,000–200,000 |
| **Tier 1** | $5 paid | 500–10,000 | 200,000–30,000,000 |
| **Tier 2** | $50 paid + 7 days | 5,000–10,000 | 2,000,000–150,000,000 |
| **Tier 3** | $100 paid + 7 days | 5,000–10,000 | 10,000,000–150,000,000 |
| **Tier 4** | $250 paid + 14 days | 10,000 | 300,000,000+ |
| **Tier 5** | $1,000 paid + 30 days | 10,000+ | 1,000,000,000+ |

> Exact limits vary by model. Check your account dashboard for your specific allocation.

### Handling Rate Limits with LangChain

LangChain provides `InMemoryRateLimiter` — a thread-safe, token-bucket rate limiter that works with any chat model. You attach it directly to the model, and LangChain handles the throttling for you.

```python
from langchain_core.rate_limiters import InMemoryRateLimiter
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic

# --- Rate limiter: 10 requests/second, burst up to 20 ---
rate_limiter = InMemoryRateLimiter(
    requests_per_second=10,     # sustained rate
    check_every_n_seconds=0.1,  # how often to check for available tokens
    max_bucket_size=20,         # max burst size
)

# Attach to any LangChain model — works identically for OpenAI and Anthropic
openai_model = ChatOpenAI(
    model="gpt-4.1",
    rate_limiter=rate_limiter,
)

anthropic_model = ChatAnthropic(
    model="claude-sonnet-4-6-20250514",
    rate_limiter=rate_limiter,
)

# Calls are automatically throttled — no manual retry logic needed
for i in range(50):
    response = openai_model.invoke(f"Question {i}: What is {i} + {i}?")
    print(response.content)
```

### Fallbacks for Rate Limit Errors

Use `with_fallbacks()` to automatically switch to a backup model when the primary model is rate-limited or unavailable.

```python
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic

# Primary model (powerful but rate-limited)
primary = ChatOpenAI(model="gpt-4o")

# Fallback chain: try each in order if the previous fails
model_with_fallbacks = primary.with_fallbacks([
    ChatAnthropic(model="claude-sonnet-4-6-20250514"),  # fallback 1
    ChatOpenAI(model="gpt-4o-mini"),                     # fallback 2 (cheap, high limits)
])

# If gpt-4o hits a rate limit, it automatically tries Claude, then gpt-4o-mini
response = model_with_fallbacks.invoke("Explain quantum computing.")
print(response.content)
```

---

## 6. Cost Optimization Strategies

### Strategy 1: Model Routing (Cheapest Model That Works)

Not every task needs the most powerful model. Use LangChain's `RunnableBranch` to route requests to the cheapest model that meets quality requirements.

```python
from langchain_core.runnables import RunnableBranch, RunnableLambda
from langchain_openai import ChatOpenAI
from langchain_anthropic import ChatAnthropic
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.output_parsers import StrOutputParser

# Define models at different price points
cheap_model = ChatOpenAI(model="gpt-4o-mini")              # $0.15/$0.60 per 1M
mid_model = ChatAnthropic(model="claude-haiku-4-5-20251001")  # $1/$5 per 1M
balanced_model = ChatAnthropic(model="claude-sonnet-4-6-20250514")  # $3/$15 per 1M
powerful_model = ChatAnthropic(model="claude-opus-4-6-20250605")    # $5/$25 per 1M

prompt = ChatPromptTemplate.from_messages([
    ("system", "You are a helpful assistant."),
    ("human", "{input}"),
])

# Route to the cheapest adequate model based on task type
model_router = RunnableBranch(
    (lambda x: x.get("task_type") == "classification", prompt | cheap_model),
    (lambda x: x.get("task_type") == "extraction", prompt | cheap_model),
    (lambda x: x.get("task_type") == "summarization", prompt | mid_model),
    (lambda x: x.get("task_type") == "code_generation", prompt | balanced_model),
    (lambda x: x.get("task_type") == "complex_reasoning", prompt | powerful_model),
    prompt | mid_model,  # default fallback
)

chain = model_router | StrOutputParser()

# Usage — automatically routes to gpt-4o-mini ($0.15/1M input)
result = chain.invoke({"task_type": "classification", "input": "Is this email spam? ..."})

# Usage — automatically routes to Claude Sonnet ($3/1M input)
result = chain.invoke({"task_type": "code_generation", "input": "Write a REST API..."})
```

### Strategy 2: Prompt Caching (Anthropic)

If your system prompt or few-shot examples are large and reused across requests, caching avoids re-processing them.

```python
import anthropic

client = anthropic.Anthropic()

# The large system prompt is cached after the first call
response = client.messages.create(
    model="claude-sonnet-4-6-20250514",
    max_tokens=1024,
    system=[
        {
            "type": "text",
            "text": "You are an expert code reviewer..." * 500,  # large prompt
            "cache_control": {"type": "ephemeral"},  # 5-min TTL cache
        }
    ],
    messages=[{"role": "user", "content": "Review this function..."}],
)

# Check cache performance
print(f"Cache creation tokens: {response.usage.cache_creation_input_tokens}")
print(f"Cache read tokens: {response.usage.cache_read_input_tokens}")
```

**Cost impact:** A 4,000-token system prompt called 100 times:
- Without caching: 4,000 × 100 × $3/1M = $1.20
- With caching: (4,000 × $3.75/1M) + (4,000 × 99 × $0.30/1M) = $0.13 — **~89% savings**

### Strategy 3: Batch API (Both Providers)

For non-time-sensitive workloads, batch processing gives a 50% discount.

```python
# Anthropic Batch API
import anthropic

client = anthropic.Anthropic()

batch = client.messages.batches.create(
    requests=[
        {
            "custom_id": f"request-{i}",
            "params": {
                "model": "claude-haiku-4-5-20251001",
                "max_tokens": 256,
                "messages": [{"role": "user", "content": f"Summarize: {text}"}],
            },
        }
        for i, text in enumerate(documents)
    ]
)
# Results delivered asynchronously — poll batch.id for status
```

### Strategy 4: Reduce Token Usage

```python
import tiktoken

def optimize_prompt(prompt: str, max_input_tokens: int = 2000) -> str:
    """Trim prompt to fit budget."""
    enc = tiktoken.encoding_for_model("gpt-4o")
    tokens = enc.encode(prompt)
    if len(tokens) > max_input_tokens:
        tokens = tokens[:max_input_tokens]
        return enc.decode(tokens) + "\n[TRUNCATED]"
    return prompt

# Other techniques:
# 1. Use concise system prompts — every token costs money
# 2. Set max_tokens to limit output length
# 3. Use structured output (JSON mode) to avoid verbose responses
# 4. Compress conversation history with summarization
```

### Strategy 5: Track and Monitor Costs

LangChain has a built-in `get_openai_callback` context manager for OpenAI cost tracking. For Anthropic and broader multi-provider tracking, use a custom callback handler.

**OpenAI — built-in callback:**
```python
from langchain_community.callbacks import get_openai_callback
from langchain_openai import ChatOpenAI

model = ChatOpenAI(model="gpt-4o-mini")

with get_openai_callback() as cb:
    response = model.invoke("Explain REST APIs in one paragraph.")
    response2 = model.invoke("Now explain GraphQL.")

print(f"Total tokens: {cb.total_tokens}")
print(f"Prompt tokens: {cb.prompt_tokens}")
print(f"Completion tokens: {cb.completion_tokens}")
print(f"Total cost (USD): ${cb.total_cost:.6f}")
print(f"Total calls: {cb.successful_requests}")
```

**Any provider — custom callback handler:**
```python
from langchain_core.callbacks import BaseCallbackHandler
from langchain_anthropic import ChatAnthropic

class CostTrackingCallback(BaseCallbackHandler):
    """Track token usage and costs across any LLM provider."""

    PRICING = {
        "claude-sonnet-4-6-20250514": {"input": 3.0, "output": 15.0},
        "claude-haiku-4-5-20251001": {"input": 1.0, "output": 5.0},
        "gpt-4o-mini": {"input": 0.15, "output": 0.60},
        "gpt-4.1": {"input": 2.0, "output": 8.0},
    }

    def __init__(self):
        self.total_cost = 0.0
        self.total_input_tokens = 0
        self.total_output_tokens = 0
        self.calls = 0

    def on_llm_end(self, response, **kwargs):
        usage = response.llm_output or {}
        token_usage = usage.get("token_usage", {})
        model = usage.get("model_name", "")

        input_tokens = token_usage.get("prompt_tokens", 0)
        output_tokens = token_usage.get("completion_tokens", 0)

        self.total_input_tokens += input_tokens
        self.total_output_tokens += output_tokens
        self.calls += 1

        prices = self.PRICING.get(model, {"input": 0, "output": 0})
        self.total_cost += (
            (input_tokens / 1_000_000) * prices["input"]
            + (output_tokens / 1_000_000) * prices["output"]
        )

    def report(self):
        return (
            f"Calls: {self.calls} | "
            f"Input: {self.total_input_tokens} | Output: {self.total_output_tokens} | "
            f"Cost: ${self.total_cost:.6f}"
        )

# Usage — pass the callback to any LangChain model
tracker = CostTrackingCallback()
model = ChatAnthropic(model="claude-sonnet-4-6-20250514", callbacks=[tracker])

model.invoke("Summarize the benefits of microservices.")
model.invoke("Compare REST vs GraphQL.")

print(tracker.report())
# Output: Calls: 2 | Input: 58 | Output: 412 | Cost: $0.006354
```

---

## 7. Real-World Cost Scenarios

### Scenario A: Customer Support Chatbot
- **Volume:** 10,000 conversations/day, avg 5 turns each
- **Tokens per turn:** ~200 input, ~300 output
- **Model:** Claude Haiku 4.5 (fast, cheap)

| Metric | Value |
|--------|-------|
| Daily input tokens | 10M |
| Daily output tokens | 15M |
| Daily cost | $10 + $75 = **$85** |
| Monthly cost | **~$2,550** |
| With Batch API (50% off) | **~$1,275** |

### Scenario B: Code Review Assistant
- **Volume:** 500 PRs/day, avg 2,000 tokens of code + 500 token response
- **Model:** Claude Sonnet 4.6

| Metric | Value |
|--------|-------|
| Daily input tokens | 1M |
| Daily output tokens | 250K |
| Daily cost | $3 + $3.75 = **$6.75** |
| Monthly cost | **~$203** |
| With prompt caching (80% hit) | **~$55** |

### Scenario C: RAG Pipeline
- **Volume:** 50,000 queries/day
- **Tokens per query:** 1,500 input (context + query), 400 output
- **Model:** GPT-4o-mini

| Metric | Value |
|--------|-------|
| Daily input tokens | 75M |
| Daily output tokens | 20M |
| Daily cost | $11.25 + $12 = **$23.25** |
| Monthly cost | **~$698** |

---

## 8. Common Pitfalls

### 1. Forgetting that conversation history compounds
Each turn in a multi-turn conversation resends all previous messages. Turn 10 of a conversation sends turns 1–9 as input.

```
Turn 1:  200 input tokens
Turn 2:  200 + 300 + 200 = 700 input tokens
Turn 3:  700 + 300 + 200 = 1,200 input tokens
Turn 10: ~5,000+ input tokens  ← 25x the cost of turn 1
```

### 2. Large system prompts multiply fast
A 2,000-token system prompt sent with every request at 100K requests/day = 200M input tokens/day. At GPT-4o rates ($2.50/1M), that's **$500/day just for the system prompt**.

**Fix:** Use prompt caching (Anthropic) or ensure cached input prefixes (OpenAI).

### 3. Not setting `max_tokens`
Without `max_tokens`, the model may generate unnecessarily long responses. Always set a reasonable limit.

### 4. Using the wrong model for simple tasks
Classification, extraction, and routing don't need GPT-4o or Opus. Use the cheapest model that achieves acceptable accuracy.

### 5. Ignoring the Batch API for async workloads
If you don't need real-time responses (reports, bulk processing, evaluations), the Batch API cuts costs by 50%.

---

## 9. Useful Pricing Resources

For up-to-date pricing that changes frequently, bookmark these:

| Resource | URL | What It Offers |
|----------|-----|----------------|
| **Official Anthropic Pricing** | [platform.claude.com/docs/en/about-claude/pricing](https://platform.claude.com/docs/en/about-claude/pricing) | Authoritative Claude pricing |
| **Official OpenAI Pricing** | [openai.com/api/pricing](https://openai.com/api/pricing) | Authoritative OpenAI pricing |
| **Price Per Token** | [pricepertoken.com](https://pricepertoken.com) | Compares 300+ models across providers, updated daily |
| **LLM Price Check** | [llmpricecheck.com](https://llmpricecheck.com) | Side-by-side cost calculator for all major providers |
| **Helicone LLM Cost** | [helicone.ai/llm-cost](https://www.helicone.ai/llm-cost) | 300+ model cost comparison with calculator |
| **CostGoat** | [costgoat.com/compare/llm-api](https://costgoat.com/compare/llm-api) | Per-token pricing across OpenAI, Anthropic, Google, DeepSeek, Mistral |
| **LLM Pricing.dev** | [llmpricing.dev](https://llmpricing.dev) | 77+ models with context length and feature comparison |

> **Recommended:** [pricepertoken.com](https://pricepertoken.com) — most comprehensive, updated daily, covers 300+ models with cost calculators.

---

## 10. Key Takeaways

1. **Output tokens cost 3–5x more than input tokens** — optimize for shorter, structured responses
2. **Use the cheapest model that meets quality requirements** — route by task complexity
3. **Prompt caching can save 80–90%** on repeated system prompts (Anthropic)
4. **Batch API gives 50% off** for non-real-time workloads (both providers)
5. **Conversation history compounds costs** — implement sliding windows or summarization
6. **Always set `max_tokens`** to prevent runaway output costs
7. **Monitor costs in production** — track per-request costs, set budget alerts
8. **Rate limits are tiered** — plan for your expected volume and upgrade tiers proactively

---

*Back to: [LLM Fundamentals](llm-fundamentals.md)*
