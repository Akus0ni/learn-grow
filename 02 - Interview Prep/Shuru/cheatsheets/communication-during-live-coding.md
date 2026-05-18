# Communicating During Live Coding

The PDF says they care about **coding style, problem-solving, and thought process** more than finishing. Translation: how you talk while coding matters as much as what you type.

## The four big moves

### 1. Restate before designing
> "So just to make sure I have it right — you want APIs to manage a library where users can borrow books, with one copy of each book and the ability to see who has what right now. Authentication is out of scope, and the data only needs to persist for the duration of this session. Did I miss anything?"

This buys you context, surfaces ambiguity, and shows you don't dive in blind.

### 2. Design out loud before typing
> "Let me sketch the endpoints first — I'll use a comment block. So I'm thinking `GET /books`, `GET /books/{id}`, `POST /borrowings` instead of an action on the book, and `DELETE /borrowings/{id}` to return the book. For the schema, I'd model Book and Borrowing as separate entities with a foreign key. Does that match what you had in mind?"

Get a nod **before** writing 200 lines.

### 3. Narrate trade-offs
- "I'm going to use an int identity key for simplicity, though Guid would be safer if these IDs were exposed externally."
- "I could use a join table for many-to-many here, but since each borrowing has its own state — return date, due date — it really wants to be its own entity."
- "I'll skip authentication since you mentioned it's out of scope, but I'd add a `[Authorize]` filter and inject the user id at the controller level."

Trade-off awareness is what separates seniors from juniors. Even if you pick the simple option, **name** the alternative.

### 4. Flag what you'd do with more time
> "I'd normally add a global exception filter that maps domain exceptions to status codes — let me add a basic version since it's quick, and I'd extract it into middleware in a real project."

You're showing they're hiring someone who knows what 'production-ready' means even if they don't have time to do all of it.

## Things to say when stuck

- "Let me check the EF Core syntax for this real quick" — then Google it. Don't pretend.
- "I'm second-guessing this approach. Let me think out loud for 30 seconds." — silence is bad; thinking out loud is good.
- "I'll come back to that — I want to keep the main flow moving."

## Things to NEVER say

- ❌ "Sorry, I'm bad at this." → never apologize for being slow.
- ❌ Long silences (> 20 seconds).
- ❌ "I have no idea how to do this." → instead: "I have a couple of approaches in mind. The first is…"
- ❌ Blaming the problem ("this is ambiguous").

## When you finish (or run out of time)

Walk them through what you built. **You drive this**, don't wait for them to ask.

> "Okay, so I have these three endpoints working. The create flow validates the input via DataAnnotations and returns 201 with a Location header. The GET endpoints paginate. I've got one unit test on the service and one integration test on the controller. If I had more time I'd add (a) the PATCH endpoint for partial updates, (b) more validation around the borrowing state machine, and (c) extract the mapping into a profile class. Want me to walk through any part in more detail?"

That paragraph alone can change the outcome of the interview.

## A small thing that matters

When you write a method, **name it carefully on the first try**. Renaming live is fine, but interviewers notice when names are good from the start: `BorrowBookAsync(bookId, userId)` not `DoBorrow(b, u)`.

## The mindset

You're not being graded on lines of code. You're being graded on whether they'd want you in their next planning meeting. Show them.
