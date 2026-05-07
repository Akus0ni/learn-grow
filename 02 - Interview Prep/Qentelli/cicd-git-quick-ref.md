# CI/CD Pipelines & Git — Quick Reference

## What is CI/CD?

| Stage | What Happens |
|-------|-------------|
| **CI** (Continuous Integration) | Code pushed → automated build + tests run |
| **CD** (Continuous Delivery) | Artifact auto-deployed to staging; manual approval for prod |
| **CD** (Continuous Deployment) | Fully automated — code goes to prod on every green build |

---

## Typical Pipeline Stages
```
Code Push → Trigger CI
         → Restore dependencies
         → Build (dotnet build)
         → Unit Tests (dotnet test)
         → Code Analysis / SAST (SonarQube)
         → Docker build + push to ECR
         → Deploy to Staging (ECS / EKS / Lambda)
         → Integration / Smoke Tests
         → Manual Approval Gate (optional)
         → Deploy to Production
         → Notification (Slack / Email)
```

---

## Tools

### GitHub Actions
```yaml
name: CI/CD
on:
  push:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal

  deploy:
    needs: build-and-test
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to AWS Lambda
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      - run: dotnet lambda deploy-function MyFunction
```

### AWS CodePipeline + CodeBuild
- **CodeCommit** — Git repo (or use GitHub)
- **CodeBuild** — runs `buildspec.yml` (build + test)
- **CodeDeploy** — deploys to EC2 / ECS / Lambda
- **CodePipeline** — orchestrates all stages

```yaml
# buildspec.yml
version: 0.2
phases:
  build:
    commands:
      - dotnet restore
      - dotnet build -c Release
      - dotnet test
      - dotnet publish -c Release -o ./publish
artifacts:
  files:
    - '**/*'
  base-directory: publish
```

---

## Docker Basics (often asked)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApi.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

Multi-stage build keeps final image small (no SDK, just runtime).

---

## Git — Key Commands & Concepts

### Daily Workflow
```bash
git clone <url>
git checkout -b feature/my-feature   # create + switch branch
git add .
git commit -m "feat: add order processing"
git push origin feature/my-feature
# → open Pull Request → code review → merge
```

### Merge vs Rebase
```bash
# Merge — creates a merge commit, preserves full history
git merge main

# Rebase — rewrites commits on top of main, linear history
git rebase main
```
- Use **merge** for feature branches going into main (preserves context)
- Use **rebase** to clean up local commits before pushing

### Common Scenarios
```bash
# Undo last commit (keep changes staged)
git reset --soft HEAD~1

# Discard all local changes
git checkout -- .

# Stash work in progress
git stash
git stash pop

# Cherry-pick a specific commit
git cherry-pick <commit-hash>

# See who changed a line
git blame src/Services/OrderService.cs
```

### Branching Strategy (GitFlow)
```
main          ← production code, always deployable
develop       ← integration branch
feature/*     ← new features (branch from develop)
release/*     ← stabilization before prod
hotfix/*      ← emergency fixes off main
```

**Trunk-based development** (simpler): everyone commits to `main` via short-lived feature branches, CI runs on every PR.

---

## Key Talking Points

**"How do you ensure quality in CI/CD?"**
- Automated unit + integration tests gate every merge
- Code coverage thresholds (e.g., >80%) enforced in pipeline
- Static analysis (SonarQube, Roslyn analyzers)
- Branch protection rules — no direct push to main, PR reviews required
- Environment-specific configs via env vars / Secrets Manager, never checked in

**"How do you handle secrets in pipelines?"**
- Never commit secrets to Git
- Use GitHub Actions Secrets / AWS Secrets Manager / Parameter Store
- Rotate credentials regularly
- Use IAM Roles for AWS actions in pipelines (no access keys if possible)
