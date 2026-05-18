# Shuru Technologies — Round 1: Backend Coding Challenge

**Interview:** Monday, 2026-05-18 · 11:00 AM · 90 minutes · screen-shared live coding
**Format:** Build several REST APIs (design + schema + DB) in your chosen stack
**Stack you're using:** ASP.NET Core 8 + EF Core + SQLite + xUnit
**Today:** Friday 2026-05-15 → you have Fri evening, Sat, Sun

---

## What they explicitly said (from the PDF)

1. Online, screen-shared. They watch you code.
2. Several APIs. **You** design the endpoints, schema, DB usage.
3. Unit tests = bonus points (explicitly called out).
4. Google is allowed. Use it.
5. They care about **coding style, problem-solving, thought process** more than completion.
6. **Prepare a boilerplate beforehand** — they literally recommend this. ← `boilerplate/` in this folder.

**Read between the lines:**
- "Several APIs" → expect a small domain (3–5 entities, ~6–10 endpoints).
- "Design the schema" → they want to see you model relationships, pick keys, think about constraints.
- "It's okay if you don't finish" → don't panic-cut corners. Show structure + 1–2 working endpoints + 1 unit test beats 5 sloppy endpoints with no tests.

---

## The 3-Day Plan

### Friday evening (today, 2–3 hrs)
1. **Open `boilerplate/`**, run it locally end-to-end. `dotnet run`. Hit Swagger. Add an item. Verify SQLite file appears. Run `dotnet test` — must pass green.
2. Re-read `boilerplate/README.md` until you can recite the 4 commands (new, add packages, migrate, run) from memory.
3. Skim `cheatsheets/rest-api-design.md` and `cheatsheets/efcore.md`.

### Saturday (4–5 hrs — the heavy day)
1. Pick **2 problems** from `practice/problems.md`. Build each end-to-end in ~75 mins. Time yourself.
2. After each, do a 10-min retro: where did you slow down? Add fixes to the boilerplate.
3. Late afternoon: read `cheatsheets/testing.md` and write 2 unit tests + 1 integration test against your Saturday code.

### Sunday (2–3 hrs — sharpen, don't cram)
1. **One** full mock at 11 AM (same time as the real interview — wakes up the right brain).
2. Re-read `cheatsheets/communication-during-live-coding.md`.
3. Polish boilerplate based on what tripped you up.
4. Stop by 5 PM. Rest.

### Monday morning (before 11 AM)
- 09:30 — coffee, light breakfast.
- 10:00 — open boilerplate, run it once, leave it running. Open Swagger, open SQLite browser, open the test runner.
- 10:30 — quick re-read of `monday-checklist.md`.
- 10:55 — join the call.

---

## What to deeply know (conceptual)

These are the things you should be able to explain out loud while typing:

### REST design
- Resource naming: nouns, plural (`/books`, not `/getBooks`).
- Verbs: GET (safe, idempotent), POST (create, not idempotent), PUT (replace, idempotent), PATCH (partial), DELETE (idempotent).
- Status codes: 200, 201 + `Location` header on POST, 204 on DELETE, 400 vs 404 vs 409 vs 422, 500.
- Nested resources: `/authors/{id}/books` when a child only makes sense under a parent.
- Pagination: `?page=1&pageSize=20`, return `total` in body or `X-Total-Count` header.
- Filtering/sorting: `?status=active&sort=-createdAt`.

### Schema design
- Pick keys: `int` identity vs `Guid`. Guid is safer for distributed/exposed IDs; int is simpler & faster. Mention the trade-off when you choose.
- Relationships: one-to-many (FK on the many side), many-to-many (join table). EF Core does both.
- Required vs optional. Unique constraints. Indexes on FKs and on columns you filter by.
- Timestamps: `CreatedAt`, `UpdatedAt` — set in `SaveChangesAsync` override. Always have them.
- Soft delete (`IsDeleted` + global query filter) — mention only if the problem implies recoverability.

### Validation
- DataAnnotations on DTOs (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`). Auto-returns 400 with details in ASP.NET Core.
- Don't validate in the controller. Don't validate in the entity. Validate at the **request DTO**.
- Business rule violations (e.g., "can't borrow a book that's already borrowed") → 409 Conflict, not 400.

### Error handling
- Use **ProblemDetails** (RFC 7807). ASP.NET Core has this built in — boilerplate is wired up.
- Don't catch-and-rethrow. Don't swallow. Let the global handler do its job.
- Custom exceptions for domain errors (`NotFoundException`, `ConflictException`) → mapped to status codes in middleware.

### Layering
```
Controller  → thin. Parse request, call service, shape response.
Service     → business logic. No HTTP knowledge.
Repository / DbContext → data access. No business rules.
DTO         ↔ Domain entity. Never expose entities directly.
```
Say this layering out loud at minute 5. It anchors the rest of your work.

### Testing
- **Unit test** services with an in-memory DbContext (or mock the repository).
- **Integration test** controllers with `WebApplicationFactory<Program>` + `Microsoft.AspNetCore.Mvc.Testing`. The boilerplate has one example.
- Arrange-Act-Assert. One assertion per test ideally. Name: `Method_State_Expected`.

---

## Files in this folder

```
Shuru/
├── README.md                                  ← you are here
├── Shuru_Coding Challenge_ Backend.pdf        ← original brief
├── boilerplate/                               ← clone-and-rename starter project
│   ├── README.md                              ← how to use it on the day
│   ├── ShuruApi.sln
│   ├── src/ShuruApi/...
│   └── tests/ShuruApi.Tests/...
├── cheatsheets/
│   ├── rest-api-design.md
│   ├── efcore.md
│   ├── testing.md
│   └── communication-during-live-coding.md
├── practice/
│   └── problems.md                            ← 5 mock problems with rubrics
└── monday-checklist.md                        ← read this Monday 10:45
```

---

## The non-negotiables for Monday

- ✅ Boilerplate runs cold in under 60 seconds.
- ✅ You can scaffold a new entity (controller + service + DbSet + migration) in under 5 minutes without looking anything up.
- ✅ You can write one passing unit test in under 3 minutes.
- ✅ You can explain your schema choices out loud, naturally.
- ✅ You start the call by **restating the problem back to them** and **sketching the API surface before typing**. This is the single highest-signal move you can make.
