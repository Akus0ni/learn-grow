"""
API Anatomy — Chat Completions

The Chat Completions API is the standard interface for LLMs. Understanding the raw request helps when working above it (LangChain, etc.).
"""
from helpers import get_model

# invoke() — blocking, returns full response
# model = get_model()
# response = model.invoke("Write a haiku.")
# print(response.content)

# stream() — yields chunks
model = get_model(streaming=True)
for chunk in model.stream("Write a haiku."):
    print(chunk.content, end="", flush=True)
