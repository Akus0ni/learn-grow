# Sprint 4: Multimodal AI in Full-Stack Apps (Days 7-8)

> **Outcome:** Multimodal AI Capabilities in Full-Stack Apps
> **Deliverable:** Next.js full-stack app with image analysis, audio transcription, and text generation

---

## Key Concepts

- Multimodal models: GPT-4o, Claude 3.5, Gemini — accept text + images
- Vision capabilities: image description, OCR, chart analysis, visual Q&A
- Audio: speech-to-text (Whisper), text-to-speech
- Image generation: DALL-E, Stable Diffusion APIs
- Vercel AI SDK: streaming AI responses in Next.js with minimal code
- Full-stack architecture: Next.js frontend + Python API backend
- Streaming UX: real-time token streaming for responsive interfaces

## Tools & Libraries

| Tool | Purpose |
|------|---------|
| `next` (14+) | React framework with App Router |
| `ai` (Vercel AI SDK) | Streaming AI responses in React |
| `@ai-sdk/openai` | OpenAI provider for Vercel AI SDK |
| `tailwindcss` | Styling |
| `openai` (Python) | Whisper, DALL-E, Vision APIs |
| `fastapi` | Python backend for audio/image processing |

---

## Day 7: Multimodal APIs & Next.js Setup

### Morning (3-4 hrs) — Multimodal AI Concepts + APIs

**Tasks:**
1. **Vision models (1.5 hrs)**
   - How multimodal models process images: base64 encoding, URL references
   - LangChain `HumanMessage` with `image_url` content type
   - Exercise: send an image to GPT-4o and ask questions about it
   - Try: describe an image, extract text from a screenshot, analyze a chart
   - Understand limitations: hallucination on fine details, no reliable OCR for complex layouts

2. **Audio models (1 hr)**
   - Whisper API: speech-to-text, supports 50+ languages
   - `openai.audio.transcriptions.create()` — transcribe an audio file
   - Text-to-speech: `openai.audio.speech.create()` — generate spoken audio
   - Exercise: record a voice memo, transcribe it, then generate speech from text

3. **Image generation (0.5 hrs)**
   - DALL-E 3 API: text to image
   - Parameters: size, quality, style
   - Exercise: generate an image from a text prompt
   - Note: image gen is optional for the capstone but good to understand

### Afternoon (3-4 hrs) — Next.js Full-Stack Setup

**Tasks:**
1. **Next.js project setup (1.5 hrs)**
   - `npx create-next-app@latest sprint-04-multimodal --typescript --tailwind --app`
   - Install: `pnpm add ai @ai-sdk/openai`
   - Understand App Router: `app/` directory, `page.tsx`, `route.ts` (API routes)
   - Create `.env.local` with `OPENAI_API_KEY`
   - Build a basic chat page with a text input and response display

2. **Vercel AI SDK streaming (1.5 hrs)**
   - Server: `app/api/chat/route.ts` — use `streamText()` from AI SDK
   - Client: `useChat()` hook — handles streaming, message state, loading
   - Build: a streaming chat interface that shows tokens arriving in real-time
   - This is the UI pattern you will reuse in the capstone

3. **Connect to Python backend (1 hr)**
   - Set up FastAPI backend with CORS enabled
   - Create endpoints: `/transcribe` (audio), `/analyze-image` (vision)
   - Next.js calls Python backend for heavy AI tasks
   - Test the round-trip: upload image from Next.js -> Python processes -> result displayed

---

## Day 8: Full Integration & Sprint Deliverable

### Morning (3-4 hrs) — Building the Features

**Tasks:**
1. **Image analysis feature (1.5 hrs)**
   - Next.js: file upload component (drag & drop or click to upload)
   - Convert uploaded image to base64
   - Send to Python backend `/analyze-image` endpoint
   - Backend sends to GPT-4o with vision, returns description
   - Display result in the UI with the original image

2. **Audio transcription feature (1 hr)**
   - Next.js: audio file upload or record-from-microphone component
   - Send audio to Python backend `/transcribe` endpoint
   - Backend uses Whisper API, returns transcription text
   - Display transcription with a "summarize" button that sends text to the chat

3. **Connecting features together (0.5 hrs)**
   - Tab-based UI: Chat | Image Analysis | Audio Transcription
   - Shared conversation context: results from image/audio analysis can feed into the chat
   - Example flow: upload an image -> get description -> ask follow-up questions about it

### Afternoon (3-4 hrs) — Build Sprint Deliverable

**Deliverable: Multimodal AI Dashboard**

A Next.js app with a Python backend that offers:
- Streaming text chat (Vercel AI SDK)
- Image upload and analysis (GPT-4o vision)
- Audio file transcription (Whisper)
- Tab-based navigation between features
- Results from one feature can be used in another (e.g., transcription -> summarize in chat)

**Build steps:**
1. Finalize the Next.js layout with 3 tabs: Chat, Vision, Audio
2. Chat tab: streaming conversation with `useChat()`
3. Vision tab: image upload -> base64 -> Python API -> GPT-4o -> display analysis
4. Audio tab: audio upload -> Python API -> Whisper -> display transcription
5. Add a "Send to Chat" button on Vision and Audio results
6. Style with Tailwind: clean, minimal dashboard
7. Test all 3 features end-to-end
8. Take screenshots for your portfolio

---

## Sprint 4 Checklist

- [ ] Sent images to a vision model and received descriptions
- [ ] Transcribed audio using Whisper API
- [ ] Set up a Next.js project with App Router and Tailwind
- [ ] Implemented streaming chat with Vercel AI SDK `useChat()`
- [ ] Connected Next.js frontend to Python FastAPI backend
- [ ] Deliverable: Multimodal AI Dashboard (Next.js + FastAPI)
- [ ] Code pushed to GitHub

## Resources to Reference

- Vercel AI SDK documentation — `useChat`, `streamText`, providers
- Next.js App Router documentation
- OpenAI Vision guide, Whisper API reference, DALL-E API reference
- LangChain docs — Multimodal messages
- Tailwind CSS documentation
