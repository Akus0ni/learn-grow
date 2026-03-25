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