# Sprint 7: Production Capstone — Knowledge Base Q&A System (Days 13-14)

> **Outcome:** Build & Launch a Production-Ready GenAI Product
> **Deliverable:** Full-stack Knowledge Base Q&A application with RAG, deployed to Vercel + Railway

---

## Key Concepts

- Production architecture: separating frontend, backend, and vector DB concerns
- API design: clean REST endpoints for ingestion, querying, and management
- Authentication: API keys or simple auth for protecting endpoints
- Error handling: graceful degradation, user-friendly error messages
- Deployment: containerization, environment variables, CI/CD basics
- Performance: caching embeddings, streaming responses, background ingestion
- Cost management: token budgets, model selection, caching strategies

## Tools & Libraries

### Backend (Python — deployed to Railway)

| Tool | Purpose |
|------|---------|
| `fastapi` | REST API framework |
| `langchain` | RAG pipeline, agent orchestration |
| `chromadb` | Vector database (persistent) |
| `python-multipart` | File upload handling |
| `uvicorn` | ASGI server |
| `python-dotenv` | Environment management |

### Frontend (Next.js — deployed to Vercel)

| Tool | Purpose |
|------|---------|
| `next` | React framework |
| `ai` (Vercel AI SDK) | Streaming AI responses |
| `tailwindcss` | Styling |
| `lucide-react` | Icons |
| `react-dropzone` | File upload UX |

---

## Day 13: Assemble the Capstone

### Morning (3-4 hrs) — Backend Architecture

**Tasks:**
1. **Project structure (1 hr)**
   ```
   capstone/
   ├── backend/
   │   ├── app/
   │   │   ├── main.py           # FastAPI app
   │   │   ├── config.py         # Settings and env vars
   │   │   ├── models.py         # Pydantic request/response models
   │   │   ├── services/
   │   │   │   ├── ingestion.py  # Document loading, chunking, embedding
   │   │   │   ├── retrieval.py  # RAG query pipeline
   │   │   │   └── agents.py     # Optional: agent-based query routing
   │   │   └── routers/
   │   │       ├── documents.py  # Upload, list, delete documents
   │   │       └── query.py      # Ask questions, get answers
   │   ├── requirements.txt
   │   ├── Dockerfile
   │   └── .env
   └── frontend/
       ├── app/
       │   ├── page.tsx          # Main chat interface
       │   ├── upload/page.tsx   # Document upload page
       │   ├── api/chat/route.ts # Proxy to backend for streaming
       │   └── layout.tsx
       ├── components/
       │   ├── ChatInterface.tsx
       │   ├── DocumentUploader.tsx
       │   ├── SourceCitation.tsx
       │   └── Sidebar.tsx
       └── package.json
   ```

2. **Backend API endpoints (2 hrs)**
   - `POST /api/documents/upload` — upload a file (PDF, TXT, MD, CSV), chunk it, embed it, store in ChromaDB
   - `GET /api/documents` — list all uploaded documents with metadata
   - `DELETE /api/documents/{id}` — remove a document and its vectors
   - `POST /api/query` — ask a question, returns `{answer, sources[], chat_history}`
   - `POST /api/query/stream` — same but streams the answer token by token
   - Build all endpoints using code from Sprint 3 (RAG pipeline) and Sprint 2 (FastAPI patterns)

3. **Ingestion pipeline (1 hr)**
   - Reuse Sprint 3 code: load -> split -> embed -> store
   - Add: file type detection, metadata extraction (filename, upload date, file type)
   - Add: background processing for large files (return 202 Accepted, process async)
   - Add: duplicate detection (don't re-ingest the same file)

### Afternoon (3-4 hrs) — Frontend Build

**Tasks:**
1. **Next.js frontend (2 hrs)**
   - Chat interface: main page, message list, input box, streaming responses
   - Reuse Sprint 4 patterns: `useChat()` hook, streaming display
   - Source citations: after each answer, show which documents were used (clickable)
   - Conversation history: sidebar showing past conversations

2. **Document management UI (1 hr)**
   - Upload page: drag-and-drop file upload (react-dropzone)
   - Document list: show uploaded files, file type icons, upload date
   - Delete button: remove documents from the knowledge base
   - Upload progress indicator

3. **Polish and integration (1 hr)**
   - Connect frontend to backend API (all endpoints)
   - Error handling: show user-friendly messages for API failures
   - Loading states: skeleton loaders while waiting for responses
   - Responsive design: works on mobile and desktop
   - Empty states: what to show when no documents are uploaded yet

---

## Day 14: Deploy & Launch

### Morning (3-4 hrs) — Production Hardening

**Tasks:**
1. **Backend production prep (1.5 hrs)**
   - Create `Dockerfile` for the Python backend:
     ```dockerfile
     FROM python:3.11-slim
     WORKDIR /app
     COPY requirements.txt .
     RUN pip install --no-cache-dir -r requirements.txt
     COPY . .
     CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
     ```
   - Add CORS configuration (allow Vercel frontend domain)
   - Add basic API key authentication (header-based)
   - Add rate limiting (simple in-memory, or use `slowapi`)
   - Health check endpoint: `GET /health`
   - Environment variables: all secrets in `.env`, never in code

2. **Error handling and edge cases (1 hr)**
   - What if the user uploads a 100MB file? (file size limit)
   - What if the question has no relevant documents? (graceful "I don't know")
   - What if the LLM API is down? (fallback message)
   - What if ChromaDB is empty? (prompt user to upload documents first)
   - Token limit management: truncate context if too many chunks retrieved

3. **Testing (0.5 hrs)**
   - Test the full flow locally: upload 5 documents, ask 10 questions
   - Verify source citations are accurate
   - Verify streaming works end-to-end
   - Test error cases: upload invalid file, ask question with empty DB

### Afternoon (3-4 hrs) — Deploy & Launch

**Tasks:**
1. **Deploy backend to Railway (1.5 hrs)**
   - Create a Railway account (free tier: 500 hours/month)
   - Connect GitHub repo to Railway
   - Railway auto-detects the Dockerfile
   - Set environment variables in Railway dashboard: `OPENAI_API_KEY`, `API_KEY`, etc.
   - Verify: `curl https://your-app.railway.app/health`
   - Note: ChromaDB data is ephemeral on Railway free tier (acceptable for demo; production would use Pinecone or managed Qdrant)

2. **Deploy frontend to Vercel (1 hr)**
   - Push frontend to GitHub (or same monorepo)
   - Connect to Vercel (free tier)
   - Set environment variable: `NEXT_PUBLIC_API_URL=https://your-app.railway.app`
   - Deploy and verify the full flow works

3. **Documentation and portfolio (0.5 hrs)**
   - Write a `README.md` for the capstone project:
     - What it does
     - Architecture diagram (text-based)
     - How to run locally
     - How to deploy
     - Screenshots
   - Add to your GitHub profile / portfolio

4. **Stretch goals if time permits (optional)**
   - Add the agent-based query routing from Sprint 5 (classify question type -> route to best retrieval strategy)
   - Add the multi-agent debate pattern from Sprint 6 for complex questions
   - Add user authentication with NextAuth.js
   - Add analytics: track popular questions, answer quality feedback (thumbs up/down)

---

## Final Architecture

```
┌─────────────────────────────────────────────────┐
│                   Vercel (Frontend)              │
│  Next.js + Tailwind + Vercel AI SDK             │
│  ┌─────────────┐  ┌──────────────┐  ┌────────┐ │
│  │ Chat UI     │  │ Doc Upload   │  │Sidebar │ │
│  │ (streaming) │  │ (drag&drop)  │  │(history)│ │
│  └──────┬──────┘  └──────┬───────┘  └────────┘ │
└─────────┼────────────────┼──────────────────────┘
          │                │
          ▼                ▼
┌─────────────────────────────────────────────────┐
│              Railway (Backend)                   │
│  FastAPI + LangChain + ChromaDB                 │
│  ┌──────────────┐  ┌───────────────────────┐   │
│  │ /api/query   │  │ /api/documents/upload │   │
│  │ (RAG + stream)│  │ (ingest pipeline)     │   │
│  └──────┬───────┘  └──────────┬────────────┘   │
│         │                     │                  │
│  ┌──────▼─────────────────────▼──────┐          │
│  │         ChromaDB (vectors)        │          │
│  └──────────────┬────────────────────┘          │
│                 │                                │
│  ┌──────────────▼────────────────────┐          │
│  │    LLM API (OpenAI / any provider)│          │
│  └───────────────────────────────────┘          │
└─────────────────────────────────────────────────┘
```

---

## Sprint 7 Checklist

- [ ] Backend API: upload, list, delete documents + RAG query with streaming
- [ ] Frontend: chat interface with streaming, document upload, source citations
- [ ] Backend deployed to Railway
- [ ] Frontend deployed to Vercel
- [ ] Full flow working: upload docs -> ask questions -> get sourced answers
- [ ] Error handling for all edge cases
- [ ] README with architecture, setup instructions, and screenshots
- [ ] All code pushed to GitHub
- [ ] Capstone complete — you are now an AI Engineer

## Resources to Reference

- Railway deployment documentation
- Vercel deployment documentation
- FastAPI deployment guide (Docker)
- Next.js deployment documentation
- LangChain production deployment tips
- Docker documentation (Dockerfile reference)
