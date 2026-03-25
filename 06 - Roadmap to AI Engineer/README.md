# Roadmap to AI Engineer — Study Notes

A sprint-based learning plan to go from programmer to AI Engineer in 14 days. Each sprint is 2 days (6–8 hours/day) and ends with a working deliverable.

## Sprints at a Glance

| Sprint | Days | Focus | Deliverable |
|--------|------|-------|-------------|
| [Sprint 1](./sprint-01.md) | 1–2 | LLM Fundamentals & Advanced Prompting | Multi-persona CLI chatbot with structured JSON output |
| [Sprint 2](./sprint-02.md) | 3–4 | Workflow Automation & API Integration | AI-powered research assistant with tool use |
| [Sprint 3](./sprint-03.md) | 5–6 | Retrieval-Augmented Generation (RAG) | RAG pipeline that answers questions over your own documents |
| [Sprint 4](./sprint-04.md) | 7–8 | Multimodal AI in Full-Stack Apps | Next.js app with image, audio, and text AI features |
| [Sprint 5](./sprint-05.md) | 9–10 | Autonomous AI Agents | Task-planning agent with tool execution and monitoring |
| [Sprint 6](./sprint-06.md) | 11–12 | Multi-Agent Systems | Multi-agent research & writing team with shared memory |
| [Sprint 7](./sprint-07.md) | 13–14 | Production Capstone | Full-stack Knowledge Base Q&A app, deployed to Vercel + Railway |

## Reference Notes

| File | Contents |
|------|----------|
| [Main Plan](./00-main-plan.md) | Learner profile, sprint overview, and how sprints build toward the capstone |
| [LLM Fundamentals](./llm-fundamentals.md) | Transformers, tokenisation, attention, pre-training, fine-tuning, emergent abilities |
| [Token Economics Deep Dive](./token-economics-deep-dive.md) | Counting tokens, cost optimisation, context window management |
| [LangChain Core Model](./langchain-core-model.md) | LangChain abstractions, chains, and provider-agnostic patterns |

## Hands-On Code (`ai-engineer-sprints/`)

| Path | Contents |
|------|----------|
| [sprint-01/](./ai-engineer-sprints/sprint-01/) | Python scripts: hello LLM, chat message roles, streaming, temperature, top-p, token counting, cost optimisation, penalty |
| [helpers/](./ai-engineer-sprints/helpers/) | Shared LLM helper utilities |

Stack: Python, LangChain, LlamaIndex — provider-agnostic (OpenAI / Anthropic / Ollama).
