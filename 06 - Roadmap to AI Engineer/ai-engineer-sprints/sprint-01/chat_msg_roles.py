"""
System
- Sets the model's behavior, persona, constraints, and context
- Processed first, before user messages
- Not visible to the end user in most chat interfaces
- Think of it as the "director's instructions to an actor"

User
- The human's input — questions, instructions, data to process
- Can include anything: text, code, structured data

Assistant
- The model's previous responses
- Used to maintain conversation history — you include prior turns so the model has context
"""
from helpers import get_model
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

model = get_model()

messages = [
    SystemMessage(content="You are a helpful assistant."),
    HumanMessage(content="What is 2+2?"),
    AIMessage(content="4"),
    HumanMessage(content="Multiply that by 10."),
]

response = model.invoke(messages)
print(response.content)
