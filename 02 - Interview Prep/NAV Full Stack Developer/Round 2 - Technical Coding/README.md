# NAV Round 2 — Technical Coding Interview

> **Date**: Tomorrow | **Feedback from Round 1**: Work on Design Patterns + SQL  
> **Format**: Live coding on a laptop — they may give you code to debug OR ask you to write from scratch

---

## What to Expect (Based on Research)

| Area | What They Test |
|------|---------------|
| **Design Patterns** | Can you implement patterns correctly in C#? Do you know *when* to use which pattern? |
| **SQL** | Complex queries, CTEs, window functions, logical puzzles with JOINs |
| **C# Logic** | Algorithms, data structures, clean code, SOLID in practice |
| **Debugging** | Find bugs in existing code — common gotchas in C# |
| **Problem Solving** | Talk through your approach BEFORE coding |

---

## Study Order for Today

| Priority | File | Time |
|----------|------|------|
| 1 | `01-design-patterns-coding.md` | 90 min |
| 2 | `02-sql-coding-challenges.md` | 90 min |
| 3 | `03-csharp-logical-thinking.md` | 60 min |
| 4 | `00-round2-cheat-sheet.md` | 15 min (read right before interview) |

---

## Interview Strategy

### For Coding Questions
1. **Read the problem twice** — repeat it back in your own words
2. **Ask clarifying questions** — edge cases? null inputs? performance constraints?
3. **Talk out loud** — "I'm thinking Repository pattern here because..."
4. **Write the interface first**, then the implementation
5. **Test with an example** before you say you're done

### For Design Pattern Questions
- Always answer: *What problem does it solve? → When do I use it? → Code example*
- Mention trade-offs — they love when you say "but the downside is..."
- Connect to real work: "In my Energy Exemplar project I used this for..."

### For SQL Questions
- Write the query step by step: FROM → WHERE → GROUP BY → HAVING → SELECT
- Think out loud about indexes and performance
- If stuck, start with a simpler version and build up

---

## Key Patterns They Care About (Based on Round 1 Feedback)

```
Creational:  Factory, Abstract Factory, Singleton, Builder
Structural:  Repository, Decorator, Adapter, Facade
Behavioral:  Strategy, Observer, Command (MediatR), Template Method
Architectural: CQRS, Unit of Work, Dependency Injection
```

---

## Files in This Folder

- `00-round2-cheat-sheet.md` — Read this last, right before the interview
- `01-design-patterns-coding.md` — Full coding exercises with solutions
- `02-sql-coding-challenges.md` — SQL puzzles with step-by-step solutions
- `03-csharp-logical-thinking.md` — Algorithms + C# logic problems

---

## Quick Confidence Boosters

- You already have **6+ years of .NET experience** — you've used these patterns
- Energy Exemplar: large-scale .NET systems = you know real-world patterns
- IKS Health: fast learning, complex integrations
- eGain: database migrations, performance work
- **You know this stuff — today is about showing it clearly**
