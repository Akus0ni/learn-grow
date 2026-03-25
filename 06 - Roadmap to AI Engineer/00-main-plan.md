# 14-Day Roadmap to AI Engineer

A structured, sprint-based learning plan to go from programmer to AI Engineer in 14 days. Each sprint is 2 days (6-8 hours/day), ends with a working deliverable, and builds toward a capstone Knowledge Base Q&A system.

---

## Learner Profile

| Attribute | Detail |
|-----------|--------|
| Starting level | Beginner in AI/ML, knows programming |
| Daily commitment | 6-8 hours |
| Backend stack | Python |
| Frontend stack | TypeScript / Next.js |
| Framework philosophy | Provider-agnostic (LangChain / LlamaIndex abstractions) |
| Learning style | Project-based — every sprint ships something |
| Capstone | Full-stack Knowledge Base / Q&A RAG system |

---

## The 7 Sprints at a Glance

| Sprint | Days | Focus | Outcome | Deliverable |
|--------|------|-------|---------|-------------|
| [Sprint 1](sprint-01.md) | 1-2 | LLM Fundamentals & Advanced Prompting | Advanced Prompting & LLM Model Mastery | Multi-persona CLI chatbot with structured output |
| [Sprint 2](sprint-02.md) | 3-4 | Workflow Automation & API Integration | Workflow Automation & API Integrations | AI-powered research assistant with tool use |
| [Sprint 3](sprint-03.md) | 5-6 | Retrieval-Augmented Generation (RAG) | Build Advanced RAG Systems | RAG pipeline that answers questions over your own documents |
| [Sprint 4](sprint-04.md) | 7-8 | Multimodal AI in Full-Stack Apps | Multimodal AI Capabilities in Full-Stack Apps | Next.js app with image, audio, and text AI features |
| [Sprint 5](sprint-05.md) | 9-10 | Autonomous AI Agents | Building Autonomous AI Agents | Task-planning agent with tool execution and monitoring |
| [Sprint 6](sprint-06.md) | 11-12 | Multi-Agent Systems | Build Intelligent Multi-Agent AI Systems | Multi-agent research & writing team with shared memory |
| [Sprint 7](sprint-07.md) | 13-14 | Production Capstone | Build & Launch a Production-Ready GenAI Product | Full-stack Knowledge Base Q&A app, deployed to Vercel + Railway |

---

## How Sprints Build Toward the Capstone

```
Sprint 1: LLM basics, prompt engineering     --> Core LLM interaction layer
Sprint 2: Tool use, API chains, workflows    --> Backend service integrations
Sprint 3: RAG pipeline, embeddings, vectors  --> The heart of the capstone
Sprint 4: Multimodal input/output            --> Rich document support (images, PDFs)
Sprint 5: Agent planning & tool execution    --> Intelligent query routing
Sprint 6: Multi-agent collaboration          --> Advanced answer synthesis
Sprint 7: Assemble everything into one app   --> Ship it
```

---

## Environment & Prerequisites

### Accounts Needed (Free Tiers)
- **OpenAI Platform** — API key (or any LLM provider; LangChain abstracts this)
- **Pinecone / Qdrant / ChromaDB** — vector database (Chroma is local/free)
- **GitHub** — version control
- **Vercel** — frontend deployment
- **Railway** — backend deployment

### Local Tools
- Python 3.11+
- Node.js 20+ / pnpm
- VS Code with Python + ESLint extensions
- Git

### Python Libraries (installed progressively)
```
langchain, langchain-openai, langchain-community
chromadb, pinecone-client
openai, tiktoken
fastapi, uvicorn
python-dotenv
```

### Node/TypeScript Libraries (installed progressively)
```
next, react, tailwindcss
ai (Vercel AI SDK)
langchain (JS)
```

---

## Daily Time Blocks

Each day follows this structure:

| Block | Time | Focus |
|-------|------|-------|
| Morning (3-4 hrs) | Concept learning + guided exercises | Theory, docs, tutorials |
| Afternoon (3-4 hrs) | Build the sprint deliverable | Hands-on coding |

---

## Resources Strategy

For each sprint, use these resource types in order:
1. **Official documentation** — LangChain docs, OpenAI docs, library READMEs
2. **Guided tutorials** — YouTube walkthroughs, blog posts from the ecosystem
3. **Source code examples** — LangChain GitHub examples, Vercel AI SDK templates
4. **Reference architectures** — LangChain cookbooks, LlamaIndex guides

---

## Progress Tracking

After each sprint, check:
- [ ] Can I explain the key concepts in my own words?
- [ ] Did I ship the sprint deliverable?
- [ ] Did I push my code to GitHub?
- [ ] Do I understand how this sprint connects to the capstone?
