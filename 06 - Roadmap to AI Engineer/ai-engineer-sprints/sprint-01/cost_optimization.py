"""
**Pricing model (typical):**
- Charged separately for input tokens and output tokens
- Output tokens are usually 2–4x more expensive than input tokens
- Example: GPT-4o-mini at ~$0.15/1M input, $0.60/1M output
"""
import tiktoken
from helpers.llm import DEFAULT_MODEL

"""
Cost Estimation
"""
def estimate_cost(prompt: str, model: str = DEFAULT_MODEL) -> dict:
    try:
        enc = tiktoken.encoding_for_model(model)
    except KeyError:
        enc = tiktoken.get_encoding("cl100k_base")
    input_tokens = len(enc.encode(prompt))

    # Rough pricing (check OpenAI for current rates)
    prices = {
        DEFAULT_MODEL: {"input": 1, "output": 5},   # per 1M tokens
        "gpt-4o":      {"input": 2.50, "output": 10.00},
    }

    price = prices.get(model, prices[DEFAULT_MODEL])
    input_cost = (input_tokens / 1_000_000) * price["input"]

    return {
        "input_tokens": input_tokens,
        "estimated_input_cost_usd": f"${input_cost:.6f}",
    }

print(estimate_cost("Explain machine learning in detail " * 100))

"""
"""

def fits_in_context(messages: list, model: str = DEFAULT_MODEL, max_tokens: int = 128000) -> bool:
    try:
        enc = tiktoken.encoding_for_model(model)
    except KeyError:
        enc = tiktoken.get_encoding("cl100k_base")
    total = sum(len(enc.encode(m["content"])) for m in messages)
    total += len(messages) * 4  # overhead per message
    return total < max_tokens * 0.9  # leave 10% buffer for response

print(fits_in_context([{"content": "Explain machine learning in detail " * 100}]))