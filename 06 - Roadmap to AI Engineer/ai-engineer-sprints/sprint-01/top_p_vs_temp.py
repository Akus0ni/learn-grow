"""
Top-p (Nucleus Sampling)

Top-p is an alternative (or complement) to temperature for controlling output randomness.

How it works:
- Instead of scaling all probabilities (temperature), top-p considers only the smallest set of tokens whose cumulative probability exceeds `p`
- `top_p=0.1` → only considers tokens making up the top 10% of probability mass
- `top_p=1.0` → considers all tokens (no filtering)

Temperature vs Top-p:
- Use one or the other, not both simultaneously (OpenAI recommends this)
- Temperature: scales the whole distribution
- Top-p: cuts off the tail of low-probability tokens

Common practice:
- For deterministic outputs: `temperature=0` (top-p doesn't matter)
- For creative outputs: adjust `temperature`, leave `top_p=1.0`
"""
from helpers import get_model
from langchain_core.messages import HumanMessage

temp = 0.7
top_p = 0.9
prompt = "Give me a one-sentence description of a sunset."
model = get_model(
    temperature=temp,
    model_kwargs={"top_p": top_p},
)
response = model.invoke([HumanMessage(content=prompt)])
print(f"[temp={temp}, top_p={top_p}] {response.content}")
