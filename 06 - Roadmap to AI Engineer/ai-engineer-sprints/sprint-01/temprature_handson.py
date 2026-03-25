"""
Temperature controls the randomness of the model's output. It scales the probability distribution over possible next tokens before sampling.
"""
from helpers import get_model
from langchain_core.messages import HumanMessage

prompt = "Give me a one-sentence description of a sunset."
for temp in [0.0, 0.0, 0.0, 1.0, 1.0, 1.0]:
    model = get_model(temperature=temp)
    response = model.invoke([HumanMessage(content=prompt)])
    print(f"[temp={temp}] {response.content}")

"""
Observation: temperature=0 produces identical outputs on repeated calls. temperature=1 produces different outputs each time.
[temp=0.0] The sun descends below the horizon in a blaze of orange, pink, and purple hues, casting a warm glow across the sky before fading into twilight.
[temp=0.0] The sun descends below the horizon in a blaze of orange, pink, and purple hues, casting a warm glow across the sky before fading into twilight.
[temp=0.0] The sun descends below the horizon in a blaze of orange, pink, and purple hues, casting a warm glow across the sky before fading into twilight.
[temp=1.0] The sun dips below the horizon in a blaze of orange, pink, and purple hues, casting a warm glow across the sky before fading into twilight.
[temp=1.0] The sun dips below the horizon, painting the sky in brilliant shades of orange, pink, and purple before fading into darkness.
[temp=1.0] The sun descends toward the horizon, painting the sky in shades of orange, pink, and purple before disappearing into darkness.
"""
