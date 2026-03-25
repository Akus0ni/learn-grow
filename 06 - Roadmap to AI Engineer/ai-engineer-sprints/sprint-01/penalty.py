"""
Frequency Penalty & Presence Penalty

These parameters reduce repetition in generated text.

**Frequency penalty** (`-2.0` to `2.0`, default `0`):
- Penalizes tokens proportional to *how many times* they've already appeared
- Higher value → model avoids repeating words it has used a lot
- Use case: long-form content generation where you don't want repetitive phrasing

**Presence penalty** (`-2.0` to `2.0`, default `0`):
- Penalizes tokens based on *whether* they've appeared at all (binary)
- Higher value → model introduces new topics/vocabulary
- Use case: brainstorming, encouraging diverse ideas
"""
from helpers import get_model
from langchain_core.messages import HumanMessage

prompt = "Explain how AI works in a few words."
freq_penalty = 0.5
presence_penalty = 0.3
model = get_model(
    frequency_penalty=freq_penalty,
    presence_penalty=presence_penalty,
)
response = model.invoke([HumanMessage(content=prompt)])
print(f"[freq={freq_penalty}, presence={presence_penalty}] {response.content}")
