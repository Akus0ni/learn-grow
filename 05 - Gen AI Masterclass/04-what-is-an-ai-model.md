# What Is an AI Model?

> An AI model is a mathematical function trained on data to recognize patterns, make predictions, or generate outputs — without being explicitly programmed with rules.

Instead of writing `if email contains "Nigerian prince" → spam`, you show it 10 million emails labeled spam/not-spam, and it *learns* the pattern itself.

---

## The Mental Model

```
Traditional Software:
  Input + Rules (written by humans) → Output

AI Model:
  Input + Data (lots of it) → Model learns Rules → Output
```

Once trained, the model is just a big mathematical function:

```
f(input) → output

f("What is the capital of France?") → "Paris"
f(photo of cat) → "cat" (97.3% confidence)
f(audio clip) → "Hello, how are you?" (transcription)
```

---

## How a Model Gets Created

```
1. Collect Data
   └─ Text, images, audio, structured data — millions/billions of examples

2. Define Architecture
   └─ The "shape" of the model (how many layers, what type of neurons)

3. Train
   └─ Feed data through the model
   └─ Compare output to correct answer (calculate error/loss)
   └─ Adjust model weights to reduce error (backpropagation)
   └─ Repeat billions of times

4. Evaluate
   └─ Test on data the model has never seen
   └─ Measure accuracy, precision, recall, etc.

5. Deploy
   └─ Package as an API or embedded system
   └─ Receives new inputs, returns predictions
```

The result is a file of **weights** — billions of numbers that encode everything the model learned.

---

## Types of AI Models

AI models are not one thing. They come in many forms depending on the task and the type of data.

---

### 1. Language Models (LLMs)

**What they do:** Understand and generate human language

**How they work:** Trained to predict the next token (word/character) given all previous tokens. Through this simple objective on massive text data, they learn grammar, facts, reasoning, and more.

```
Input:  "The Eiffel Tower is located in"
Output: "Paris, France."
```

**Architecture:** Transformer (attention mechanism)

**Scale:** Billions of parameters
- GPT-3: 175 billion
- GPT-4: estimated ~1.8 trillion (mixture of experts)
- Claude 3.5 Sonnet: undisclosed, but frontier-scale

**Examples:**

| Model | Company | Strength |
|---|---|---|
| `GPT-4o` | OpenAI | Multimodal, broad capability |
| `Claude 3.5 / 4.x` | Anthropic | Long context, safety, reasoning |
| `Gemini 1.5 / 2.0` | Google DeepMind | Multimodal, 1M token context |
| `Llama 3` | Meta | Open-source, self-hostable |
| `Mistral / Mixtral` | Mistral AI | Efficient open-source |
| `Command R+` | Cohere | Enterprise RAG-optimized |
| `Grok` | xAI | Real-time web access |

**Used for:** Chatbots, coding assistants, summarization, translation, agents

---

### 2. Vision Models (Computer Vision)

**What they do:** Understand and process images and video

**Tasks:**
- **Image classification** — "What is this a photo of?"
- **Object detection** — "Where are all the cars in this image?"
- **Image segmentation** — Outline every pixel belonging to each object
- **Optical Character Recognition (OCR)** — Extract text from images
- **Pose estimation** — Detect body positions

**Architecture:** Convolutional Neural Networks (CNNs), Vision Transformers (ViT)

```
Input:  [photo of chest X-ray]
Output: "Pneumonia detected — 91% confidence, affected region: lower left lobe"
```

**Examples:**

| Model | Company | Use Case |
|---|---|---|
| `YOLO v8/v9` | Ultralytics | Real-time object detection |
| `ResNet / EfficientNet` | Google | Image classification |
| `SAM (Segment Anything)` | Meta | Segment any object in any image |
| `CLIP` | OpenAI | Match images to text descriptions |
| `Vision Transformer (ViT)` | Google | General image understanding |
| `EasyOCR` / `Tesseract` | Open-source | Text extraction from images |
| `DeepFace` | Open-source | Facial recognition & analysis |

**Real-world uses:** Self-driving cars, medical imaging, security cameras, photo tagging, quality control in manufacturing

---

### 3. Multimodal Models

**What they do:** Process *multiple types of data* — text + images, text + audio, text + video

**The shift:** Early AI models were siloed — one model per modality. Modern frontier models handle text, images, audio, and video in a single model.

```
Input:  [image of a broken pipe] + "What's wrong and how do I fix it?"
Output: "The image shows a cracked PVC pipe joint. To fix it:
         1. Turn off the water supply valve
         2. Cut out the damaged section...
         3. Apply PVC cement to the new joint..."
```

**Examples:**

| Model | Modalities | Company |
|---|---|---|
| `GPT-4o` | Text + Image + Audio + Video | OpenAI |
| `Claude 3.5 Sonnet` | Text + Image + Document | Anthropic |
| `Gemini 1.5 Pro` | Text + Image + Audio + Video + Code | Google |
| `LLaVA` | Text + Image | Open-source |
| `Flamingo` | Text + Image | Google DeepMind |
| `ImageBind` | Text + Image + Audio + Depth + IMU | Meta |
| `Whisper` | Audio → Text | OpenAI |

**Real-world uses:** Medical diagnosis (image + notes), video understanding, accessibility tools, document analysis, customer support with screenshots

---

### 4. Generative Models

**What they do:** Create new content — images, audio, video, 3D objects, code

#### Image Generation
```
Input:  "A photorealistic astronaut riding a horse on Mars at sunset"
Output: [generated image matching description]
```

**How they work:**
- **Diffusion Models** — start with pure noise, gradually denoise into a coherent image
- **GANs (Generative Adversarial Networks)** — generator creates images, discriminator judges them; both improve through competition
- **VAEs (Variational Autoencoders)** — compress to latent space, then decode to output

| Model | Type | Company | Best For |
|---|---|---|---|
| `DALL-E 3` | Diffusion | OpenAI | Text-to-image, follows instructions well |
| `Stable Diffusion` | Diffusion | Stability AI | Open-source, highly customizable |
| `Midjourney` | Diffusion | Midjourney | Artistic, high aesthetic quality |
| `Flux` | Diffusion | Black Forest Labs | Photorealistic, open-source |
| `Imagen 3` | Diffusion | Google | Photorealism |
| `Firefly` | Diffusion | Adobe | Commercially safe, Photoshop integration |

#### Audio & Music Generation
| Model | Company | Output |
|---|---|---|
| `Suno` | Suno AI | Full songs with lyrics and vocals |
| `Udio` | Udio | Music generation from text |
| `ElevenLabs` | ElevenLabs | Voice cloning and TTS |
| `Bark` | Suno AI | Open-source voice synthesis |
| `MusicGen` | Meta | Instrumental music generation |
| `AudioCraft` | Meta | General audio / sound effects |

#### Video Generation
| Model | Company | Output |
|---|---|---|
| `Sora` | OpenAI | Text-to-video, high fidelity |
| `Runway Gen-3` | Runway | Video generation + editing |
| `Pika` | Pika Labs | Short video clips from text/image |
| `Kling` | Kuaishou | Realistic motion video |
| `Veo 2` | Google | High-quality video generation |

#### Code Generation
| Model | Company | Output |
|---|---|---|
| `GitHub Copilot` | GitHub/OpenAI | In-IDE code completion |
| `Claude` (Sonnet/Opus) | Anthropic | Full-file code generation, debugging |
| `Codex` | OpenAI | Code from natural language |
| `StarCoder 2` | HuggingFace | Open-source code model |
| `DeepSeek Coder` | DeepSeek | Strong open-source code model |

---

### 5. Embedding Models

**What they do:** Convert text (or images) into dense vectors of numbers that capture *meaning*

```
"The dog barked"        → [0.21, -0.83, 0.44, 0.12, ...]  (1536 numbers)
"The canine made noise" → [0.22, -0.81, 0.43, 0.14, ...]  (very similar!)
"The stock market fell" → [-0.43, 0.21, -0.87, 0.33, ...] (very different)
```

Semantically similar text produces vectors that are *close together* in vector space — even if the words are different.

**Used for:**
- Semantic search (find documents by meaning, not keywords)
- RAG (retrieve relevant chunks for LLM context)
- Duplicate detection
- Recommendation systems
- Clustering and classification

**Examples:**

| Model | Company | Dimensions | Best For |
|---|---|---|---|
| `text-embedding-3-large` | OpenAI | 3072 | General purpose, high accuracy |
| `text-embedding-3-small` | OpenAI | 1536 | Faster, cheaper, still strong |
| `embed-english-v3` | Cohere | 1024 | English retrieval tasks |
| `embed-multilingual-v3` | Cohere | 1024 | 100+ languages |
| `all-MiniLM-L6-v2` | HuggingFace | 384 | Fast, open-source, local |
| `BGE-M3` | BAAI | 1024 | State-of-the-art open-source |
| `Nomic Embed` | Nomic | 768 | Open-source, long context |
| `CLIP` | OpenAI | 512 | Image + text in same space |

---

### 6. Reinforcement Learning (RL) Models

**What they do:** Learn to take actions in an environment to maximize a reward signal — without labeled training data

**How they work:**
```
Agent takes action → Environment gives reward/penalty → Agent updates policy
                              ↑                              ↓
                    Repeat millions of times ←──────────────┘
```

The model learns by *trying things*, not by being told the correct answer.

**Examples:**

| Model | Domain | Achievement |
|---|---|---|
| `AlphaGo / AlphaZero` | Board games (Go, Chess) | Beat world champions |
| `OpenAI Five` | Dota 2 | Beat professional human teams |
| `MuZero` | Atari + board games | Mastered 57 games without knowing rules |
| `AlphaFold 2/3` | Protein folding | Solved 50-year biology problem |
| `Gato` | Multi-task RL | 600+ tasks: games, robotics, captioning |

**Real-world uses:** Robotics, game AI, recommendation engines, trading algorithms, autonomous vehicles, drug discovery

---

### 7. Specialized / Domain Models

Fine-tuned or purpose-built models for specific industries or tasks — often smaller but more accurate than general-purpose models in their domain.

| Domain | Model | What It Does |
|---|---|---|
| **Medical** | `Med-PaLM 2` (Google) | Medical Q&A, clinical notes |
| **Medical** | `BioGPT` (Microsoft) | Biomedical literature understanding |
| **Legal** | `Harvey AI` | Legal document analysis, drafting |
| **Finance** | `BloombergGPT` | Financial data, news, analysis |
| **Science** | `Galactica` (Meta) | Scientific literature, formulas |
| **Code** | `DeepSeek Coder` | Code generation, debugging |
| **3D / CAD** | `Point-E` (OpenAI) | Text to 3D point clouds |
| **Biology** | `AlphaFold` (DeepMind) | Protein structure prediction |
| **Cybersecurity** | `Sec-BERT` | Security log analysis |

---

## Model Sizes — What the Numbers Mean

Models are described by their **parameter count** — the number of adjustable weights learned during training.

```
Nano/Tiny:   < 1B parameters   → Runs on phone, very fast, limited
Small:       1B–7B parameters  → Runs on laptop GPU, good for focused tasks
Medium:      7B–30B parameters → Needs decent GPU, strong general capability
Large:       30B–70B params    → Multi-GPU, near-frontier quality
Frontier:    100B+ parameters  → Data center scale, state-of-the-art
```

| Size Class | Example Models | Where It Runs |
|---|---|---|
| Tiny | Gemini Nano, Phi-3 Mini (3.8B) | Mobile device |
| Small | Llama 3.2 (3B), Phi-3 Small (7B) | Laptop CPU/GPU |
| Medium | Llama 3.1 (8B), Mistral (7B) | Consumer GPU |
| Large | Llama 3.1 (70B), Mixtral (47B) | Multi-GPU workstation |
| Frontier | GPT-4, Claude, Gemini Ultra | Cloud / data center |

> Bigger ≠ always better. A specialized 7B model often outperforms a 70B general model on its specific task.

---

## How Models Are Trained — Key Concepts

### Pre-training
- Train on massive, diverse data (the whole internet, books, code)
- Learns general world knowledge and language
- Expensive: GPT-4 estimated $100M+ to train

### Fine-tuning
- Take a pre-trained model and train further on a *specific* dataset
- Adapts general knowledge to a specific domain/task
- Much cheaper: can be done in hours on a single GPU

```
Pre-trained GPT → Fine-tune on legal contracts → Legal AI assistant
Pre-trained GPT → Fine-tune on medical notes  → Medical AI assistant
```

### RLHF (Reinforcement Learning from Human Feedback)
- Humans rank model outputs (A is better than B)
- Model trains to produce outputs humans prefer
- How ChatGPT, Claude, and Gemini learned to be helpful and safe

### PEFT / LoRA (Parameter-Efficient Fine-Tuning)
- Fine-tune only a small subset of weights (not the whole model)
- 100x cheaper than full fine-tuning
- Produces "adapters" you can swap in/out

---

## Closed vs. Open Models

| | Closed / Proprietary | Open Source |
|---|---|---|
| **Access** | API only | Download weights |
| **Privacy** | Data goes to provider | Fully local |
| **Cost** | Pay per token | Free (compute cost only) |
| **Capability** | Generally frontier | Catching up fast |
| **Customization** | Limited | Full control |
| **Examples** | GPT-4, Claude, Gemini | Llama 3, Mistral, Falcon |

---

## How to Access AI Models

### 1. API (Most Common)
```python
# Anthropic Claude
import anthropic
client = anthropic.Anthropic(api_key="...")
response = client.messages.create(
    model="claude-sonnet-4-6",
    messages=[{"role": "user", "content": "Explain transformers"}]
)
```

### 2. Hosted Platforms
| Platform | Models Available |
|---|---|
| `OpenAI API` | GPT-4o, o1, embeddings, DALL-E |
| `Anthropic API` | Claude 3.5, Claude 4 family |
| `Google AI Studio` | Gemini family |
| `AWS Bedrock` | Claude, Llama, Mistral, Titan |
| `Azure OpenAI` | GPT-4, embeddings (enterprise) |
| `Hugging Face` | 500,000+ open-source models |
| `Replicate` | Run any model via API |
| `Together AI` | Fast inference for open models |
| `Groq` | Ultra-fast inference (LPU hardware) |
| `Ollama` | Run models locally on your machine |

### 3. Local / Self-Hosted
```bash
# Run Llama 3 locally with Ollama
ollama run llama3.1

# Run any HuggingFace model
from transformers import pipeline
pipe = pipeline("text-generation", model="meta-llama/Llama-3.1-8B")
```

---

## Choosing the Right Model

```
What type of output do I need?
├── Text / Language  → LLM (GPT-4, Claude, Gemini, Llama)
├── Images           → Vision model or generative (DALL-E, Stable Diffusion)
├── Audio            → Whisper (speech-to-text), ElevenLabs (TTS)
├── Video            → Sora, Runway, Pika
├── Code             → Claude, Copilot, DeepSeek Coder
├── Embeddings       → text-embedding-3, BGE, Nomic
└── Specialized      → Domain-specific fine-tuned model

What are my constraints?
├── Privacy required    → Open-source + local (Llama, Mistral via Ollama)
├── Cost sensitive      → Smaller models, Groq for speed, batch APIs
├── Latency critical    → Groq, Cerebras, smaller models
├── Max accuracy        → GPT-4o, Claude Opus, Gemini Ultra
└── Custom domain       → Fine-tune a base model on your data
```

---

## TL;DR — Model Taxonomy

```
AI Models
├── Language Models (LLMs)       → GPT-4, Claude, Gemini, Llama
├── Vision Models                → YOLO, SAM, ViT, ResNet
├── Multimodal Models            → GPT-4o, Gemini 1.5, Claude 3.5
├── Generative Models
│   ├── Image                    → DALL-E 3, Midjourney, Stable Diffusion
│   ├── Audio / Music            → Suno, ElevenLabs, MusicGen
│   ├── Video                    → Sora, Runway, Pika
│   └── Code                     → Copilot, CodeLlama, DeepSeek
├── Embedding Models             → text-embedding-3, BGE, Nomic
├── Reinforcement Learning       → AlphaGo, OpenAI Five, AlphaFold
└── Specialized / Domain Models  → BloombergGPT, Med-PaLM, Harvey
```

> A model is a tool. The right tool depends on your task, data, constraints, and budget.
