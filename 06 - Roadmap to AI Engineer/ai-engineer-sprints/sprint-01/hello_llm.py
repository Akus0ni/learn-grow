"""
Task 1 - Environment setup test: Basic API call via LangChain's ChatAnthropic
"""
from helpers import get_model
from langchain_core.messages import HumanMessage, SystemMessage

model = get_model()

response = model.invoke([
    SystemMessage(content="You are a helpful assistant."),
    HumanMessage(content="Hello, world! Respond in one sentence."),
])

print("Response:", response.content)
print("Model:", response.response_metadata.get("model"))
print("Tokens used:", response.usage_metadata)
