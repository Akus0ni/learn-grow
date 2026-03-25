# Sprint 3: Retrieval-Augmented Generation / RAG (Days 5-6)

> **Outcome:** Build Advanced RAG Systems
> **Deliverable:** RAG pipeline that answers questions over your own document collection

---

## Key Concepts

- Why RAG: LLMs have knowledge cutoffs and hallucinate; RAG grounds them in your data
- The RAG pipeline: Load -> Split -> Embed -> Store -> Retrieve -> Generate
- Embeddings: turning text into numerical vectors that capture meaning
- Vector databases: storing and searching embeddings by similarity
- Chunking strategies: how you split documents matters enormously
- Retrieval strategies: similarity search, MMR (maximal marginal relevance), hybrid search
- Context window management: fitting retrieved chunks + question + prompt into the context

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `langchain` | RAG pipeline orchestration |
| `chromadb` | Local vector database (zero setup) |
| `langchain-community` | Document loaders (PDF, Markdown, CSV, web) |
| `tiktoken` | Token counting for chunk sizing |
| `unstructured` | Advanced document parsing |
| `pypdf` | PDF loading |

---

## Day 5: RAG Fundamentals & Pipeline

### Morning (3-4 hrs) — Concepts & Core Pipeline

**Tasks:**
1. **RAG architecture deep dive (1 hr)**
   - The problem: LLMs cannot access your private data
   - The solution: retrieve relevant context, inject into the prompt
   - RAG vs. fine-tuning: when to use each (RAG for most use cases)
   - The full pipeline diagram: Documents -> Chunks -> Embeddings -> Vector DB -> Query -> Retrieved Context -> LLM -> Answer

2. **Document loading (1 hr)**
   - `PyPDFLoader` — load PDF files
   - `TextLoader` — load plain text / markdown
   - `CSVLoader` — load structured data
   - `WebBaseLoader` — load web pages
   - `DirectoryLoader` — load all files in a folder
   - Exercise: load 5-10 documents of mixed types (use your own notes, READMEs, or sample PDFs)

3. **Chunking strategies (1.5 hrs)**
   - Why chunking matters: too big = noise, too small = missing context
   - `RecursiveCharacterTextSplitter` — the go-to splitter (splits by paragraphs, then sentences, then characters)
   - `chunk_size` and `chunk_overlap` — experiment with 500/50, 1000/100, 1500/200
   - Metadata preservation: keep source filename, page number with each chunk
   - Exercise: split the same document with different settings, compare chunk counts and quality

### Afternoon (3-4 hrs) — Embeddings & Vector Store

**Tasks:**
1. **Embeddings (1.5 hrs)**
   - What embeddings are: dense vector representations of text meaning
   - `OpenAIEmbeddings` — uses `text-embedding-3-small` (cheap, good quality)
   - Dimension sizes: 1536 (small) vs. 3072 (large) — tradeoffs
   - Exercise: embed 3 sentences, compute cosine similarity between them
   - See that "dog" is closer to "puppy" than to "database"

2. **ChromaDB setup and indexing (1.5 hrs)**
   - `Chroma.from_documents(docs, embeddings)` — create a vector store from chunks
   - Persistence: save to disk so you don't re-embed every time
   - Collection management: create, list, delete collections
   - Index your chunked documents into ChromaDB
   - Verify: query the store with `.similarity_search("your question", k=4)`

3. **Basic RAG chain (1 hr)**
   - Build the retrieval chain with LCEL:
     ```python
     retriever = vectorstore.as_retriever(search_kwargs={"k": 4})
     chain = (
       {"context": retriever, "question": RunnablePassthrough()}
       | prompt
       | model
       | StrOutputParser()
     )
     ```
   - Test with questions about your indexed documents
   - Observe: does it answer correctly? Does it cite sources?

---

## Day 6: Advanced RAG & Sprint Deliverable

### Morning (3-4 hrs) — Advanced Retrieval Techniques

**Tasks:**
1. **Retrieval strategies (1.5 hrs)**
   - Similarity search: pure cosine distance (default)
   - MMR (Maximal Marginal Relevance): balances relevance with diversity
   - Compare results: similarity vs. MMR on the same query
   - Metadata filtering: "only search documents from source X"
   - `search_type="mmr"` with `fetch_k=20, k=4`

2. **RAG evaluation and debugging (1 hr)**
   - Common failure modes:
     - Wrong chunks retrieved (embedding quality, chunk size issue)
     - Right chunks retrieved but LLM ignores them (prompt issue)
     - Answer hallucinated despite good context (model issue)
   - Debugging: print retrieved chunks before passing to the LLM
   - Add source citations: include document name + page number in the answer

3. **Conversational RAG (0.5 hrs)**
   - Add chat history to RAG: the user can ask follow-ups
   - `create_history_aware_retriever` — reformulates the question given chat history
   - Example: "What is X?" -> "Tell me more about that" -> system knows "that" = X

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: Document Q&A RAG Pipeline**

A Python application that:
- Loads documents from a folder (PDFs, markdown, text files)
- Chunks and embeds them into ChromaDB
- Answers questions using RAG with source citations
- Supports conversational follow-ups (chat history aware)
- Has a CLI interface for interactive Q&A
- Exposes a FastAPI `/ask` endpoint

**Build steps:**
1. Create a `documents/` folder with 10+ files (use your learn-grow notes, sample PDFs, etc.)
2. Build the ingestion pipeline: load -> split -> embed -> store in ChromaDB
3. Build the retrieval chain with MMR search
4. Add a prompt template that instructs: "Answer based only on the provided context. Cite your sources."
5. Add conversational memory with `create_history_aware_retriever`
6. Build CLI loop: user types question, sees answer + sources
7. Build FastAPI `/ask` endpoint: accepts `{question, chat_history}`, returns `{answer, sources}`
8. Test with at least 10 questions, verify source citations are accurate

**This is the core of your capstone.** Sprint 7 will wrap a full-stack UI around this pipeline.

---

## Sprint 3 Checklist

- [ ] Can explain the full RAG pipeline: load, split, embed, store, retrieve, generate
- [ ] Loaded documents from multiple formats (PDF, text, markdown)
- [ ] Experimented with chunking strategies and parameters
- [ ] Built embeddings and stored in ChromaDB
- [ ] Compared similarity search vs. MMR retrieval
- [ ] Built a conversational RAG chain with source citations
- [ ] Deliverable: Document Q&A RAG pipeline (CLI + API)
- [ ] Code pushed to GitHub

## Resources to Reference

- LangChain docs — RAG tutorial, Document Loaders, Text Splitters, Vector Stores, Retrievers
- ChromaDB documentation
- OpenAI Embeddings guide
- LangChain cookbook — RAG patterns
- Blog posts on chunking strategies and RAG evaluation
