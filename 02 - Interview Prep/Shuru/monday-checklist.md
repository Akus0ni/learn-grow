# Monday Morning Checklist — read at 10:45

## Environment (do this by 10:30)
- [ ] `boilerplate/` open in VS / Rider / VS Code
- [ ] Terminal at `src/ShuruApi/` ready
- [ ] `dotnet run` — confirm Swagger loads at https://localhost:xxxx/swagger
- [ ] DB Browser for SQLite open (or `dotnet ef` ready)
- [ ] `dotnet test` — confirm green
- [ ] Browser tab: Microsoft Docs (EF Core), one for ASP.NET Core
- [ ] Close: Slack, email, anything that pings
- [ ] Phone face-down, silent
- [ ] Water nearby

## Mental warm-up (10:45–10:55)
- [ ] Read `cheatsheets/communication-during-live-coding.md` once
- [ ] Out loud: "GET 200, POST 201 + Location, PUT 200/204, DELETE 204, NotFound 404, Conflict 409, Validation 400"
- [ ] Out loud: "Controller thin, service has the logic, DbContext does data"

## When the call starts (first 5 minutes)
1. Greet. Confirm screen share works on their side.
2. Wait for the problem.
3. **Restate the problem back in your own words.** Ask clarifying questions:
   - "Is X a single user or multi-tenant?"
   - "Do I need authentication, or assume an authenticated user?"
   - "What happens if [edge case]?"
   - "Do you want me to focus on breadth or depth if I run out of time?"
4. **Sketch the API surface before typing.** Open notepad or comment block. List endpoints + methods + status codes. Get a nod from the interviewer.
5. **Sketch the schema.** Entities + key relationships. Get a nod.
6. Now start typing.

## Order of work
1. Rename solution + project (or skip — they don't care about the name).
2. Define entities. Add to `AppDbContext`.
3. `dotnet ef migrations add Initial && dotnet ef database update`.
4. Build the **happy path** of ONE endpoint end-to-end. POST or GET — pick the easiest. Hit it from Swagger. Show it works.
5. **Now you have a demo.** Everything from here is upside.
6. Add the rest of the endpoints. Add validation. Add error handling.
7. Write ONE unit test. Even one signals you can. Two is better.
8. If time: pagination, filtering, a clean README.

## Time markers
- **15 min in:** one entity migrated, one endpoint working in Swagger. If not → you're stuck on plumbing. Tell them out loud and ask if you can simplify.
- **45 min in:** core CRUD done. Now move to tests + edge cases.
- **75 min in:** stop adding features. Polish what exists. Write the test if you haven't.
- **85 min in:** stop. Walk through what you built. Be honest about what's missing and what you'd do with more time.

## If you get stuck
- **Don't go silent.** Narrate: "I'm thinking about whether to model this as a join table or a one-to-many because…"
- Google freely. They explicitly said it's fine.
- If a feature is fighting you, **skip it**. Note it in a comment: `// TODO: would add X here — running out of time`.

## Worst-case lines (have these ready)
- "Let me note that and come back — I want to keep momentum on the main flow first."
- "I'd normally write a test here; given time I'll add one at the end if we have room."
- "I'm not 100% on the EF Core syntax for this — let me check the docs quickly."

You've got this.
