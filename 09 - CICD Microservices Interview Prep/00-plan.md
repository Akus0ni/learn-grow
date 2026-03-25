# CI/CD + Microservices Interview Prep Plan

## Goal
Build a microservices example using .NET Core, MVC, Web API, and Angular — with a working CI/CD pipeline using GitHub Actions — all in one evening.

---

## Time-Boxed Plan (4-5 hours)

### Phase 1: Project Setup (30 min)
- [ ] Create a new GitHub repo: `dotnet-microservices-cicd-demo`
- [ ] Scaffold the solution structure with 3 microservices
- [ ] Initialize Angular frontend app
- [ ] Push initial commit

### Phase 2: Build Microservices (90 min)
- [ ] **ProductService** — Web API (.NET Core) with CRUD endpoints
- [ ] **OrderService** — Web API (.NET Core) with order management
- [ ] **Gateway/BFF** — MVC + Web API gateway that aggregates the two services
- [ ] Add Entity Framework Core with In-Memory database (no SQL Server needed)
- [ ] Add inter-service HTTP communication using HttpClient/IHttpClientFactory
- [ ] Add Swagger/OpenAPI to each service

### Phase 3: Angular Frontend (30 min)
- [ ] Scaffold Angular app with Angular CLI
- [ ] Create a products list component calling the Gateway API
- [ ] Create an orders component
- [ ] Add basic routing

### Phase 4: CI/CD Pipeline with GitHub Actions (60 min)
- [ ] Create `.github/workflows/ci.yml` — build + test on every push/PR
- [ ] Create `.github/workflows/cd.yml` — deploy step (simulated)
- [ ] Add unit tests for at least one service
- [ ] Add a build badge to README
- [ ] Practice triggering the pipeline by pushing changes

### Phase 5: Docker + Compose (30 min)
- [ ] Add Dockerfile for each microservice
- [ ] Add `docker-compose.yml` to run everything locally
- [ ] Add Docker build step to CI pipeline

### Phase 6: Review & Practice (30 min)
- [ ] Review cheatsheet (`01-cheatsheet.md`)
- [ ] Review concept explanations (`02-concepts-explained.md`)
- [ ] Practice explaining the architecture out loud
- [ ] Review common interview questions (`03-interview-questions.md`)

---

## Solution Architecture

```
┌─────────────────────────────────────────────────┐
│                  Angular SPA                     │
│              (http://localhost:4200)              │
└──────────────────────┬──────────────────────────┘
                       │ HTTP
┌──────────────────────▼──────────────────────────┐
│              API Gateway / BFF                   │
│          ASP.NET Core MVC + Web API              │
│              (http://localhost:5000)              │
└───────┬──────────────────────────┬──────────────┘
        │ HTTP                     │ HTTP
┌───────▼─────────┐    ┌──────────▼──────────┐
│  ProductService  │    │    OrderService      │
│  ASP.NET Core    │    │    ASP.NET Core      │
│    Web API       │    │      Web API         │
│  (port 5001)     │    │    (port 5002)       │
│  EF Core InMem   │    │    EF Core InMem     │
└─────────────────┘    └─────────────────────┘
```

```
CI/CD Pipeline (GitHub Actions)
─────────────────────────────────
Push/PR ──► Build ──► Test ──► Docker Build ──► Deploy (simulated)
```

---

## Repo Structure (Target)

```
dotnet-microservices-cicd-demo/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
├── src/
│   ├── ProductService/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Program.cs
│   │   ├── ProductService.csproj
│   │   └── Dockerfile
│   ├── OrderService/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Program.cs
│   │   ├── OrderService.csproj
│   │   └── Dockerfile
│   ├── ApiGateway/
│   │   ├── Controllers/
│   │   ├── Views/         (MVC views)
│   │   ├── Models/
│   │   ├── Program.cs
│   │   ├── ApiGateway.csproj
│   │   └── Dockerfile
│   └── angular-frontend/
│       └── (Angular CLI project)
├── tests/
│   ├── ProductService.Tests/
│   └── OrderService.Tests/
├── docker-compose.yml
├── MicroservicesDemo.sln
└── README.md
```
