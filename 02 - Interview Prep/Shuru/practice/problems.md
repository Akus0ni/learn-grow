# Practice Problems — 90-min mock challenges

Pick 2 for Saturday. Time yourself strictly. Use the boilerplate.

After each mock, do a 10-min retro: where did you slow down? what tripped you up? Update boilerplate.

---

## Problem 1: Library Borrowing System ⭐ (easiest — start here)

**Build APIs for a small library to track books and who borrows them.**

Requirements:
- CRUD for books (title, author, ISBN, total copies, available copies).
- POST `/borrowings` to borrow a book. Decrements available copies. 409 if none available.
- POST `/borrowings/{id}/return` to return a book. Increments available. 409 if already returned.
- GET `/borrowings?userId=` to list current borrowings by user.
- GET `/books?available=true` filter.
- A borrowing has: id, bookId, userId, borrowedAt, returnedAt (nullable).

Bonus:
- Due date 14 days from borrow, `?overdue=true` filter on borrowings.
- Unit test: borrowing decrements available copies.
- Integration test: cannot borrow when 0 available → 409.

**Rubric:** schema modeling (book vs borrowing relationship), state transitions (borrow/return as state machine), 409 vs 400, transactionality of the borrow operation.

---

## Problem 2: URL Shortener ⭐⭐

**Build a tiny URL shortener with usage tracking.**

Requirements:
- POST `/links` with `{ longUrl }` → returns `{ shortCode, shortUrl }`.
- GET `/{shortCode}` → 301/302 redirect to the long URL. Increments hit count.
- GET `/links/{shortCode}/stats` → `{ longUrl, hits, createdAt }`.
- DELETE `/links/{shortCode}` → 204.
- Short codes are 6-character base62 (`[0-9a-zA-Z]`).
- Same long URL submitted twice should return the existing code, not create duplicate (idempotency).

Bonus:
- Optional custom alias (`POST /links` with `{ longUrl, alias }`) — 409 if taken.
- Expiry: optional `expiresAt`. GET on expired → 410 Gone.
- Unit test: short code generation produces valid base62.
- Integration test: redirect returns 302 with correct Location.

**Rubric:** ID strategy (random + collision check vs incremental → base62 encode), idempotency on POST, status code choice for redirect (301 permanent vs 302 found), uniqueness constraint on `shortCode`.

---

## Problem 3: Task Tracker with Tags ⭐⭐

**Build APIs for a personal task tracker. Tasks have tags (many-to-many).**

Requirements:
- CRUD for tasks: id, title, description, status (Todo/InProgress/Done), createdAt, completedAt.
- Many-to-many with tags: a task can have multiple tags, a tag can be on many tasks.
- POST `/tasks/{id}/tags` with `{ tagNames: ["urgent", "home"] }` — creates tags if they don't exist, associates them.
- DELETE `/tasks/{id}/tags/{tagName}` to remove.
- GET `/tasks?tag=urgent&status=Todo&sort=-createdAt&page=1&pageSize=20`.
- PATCH `/tasks/{id}/status` with `{ status: "Done" }`. Sets `completedAt` if moving to Done.

Bonus:
- Cannot move from Done → Todo (state machine). 409 if attempted.
- GET `/tags` with usage counts.
- Unit test: state transition rules.

**Rubric:** many-to-many with EF Core (join entity or skip-navigation), filter+sort+page composition, partial update with PATCH, state machine modeling.

---

## Problem 4: Expense Splitter ⭐⭐⭐

**Build APIs to split expenses among a group (Splitwise-lite).**

Requirements:
- POST `/groups` with `{ name, memberNames }` → creates group with members.
- POST `/groups/{id}/expenses` with `{ description, amount, paidBy, splitAmong: [memberIds] }` → equal split.
- GET `/groups/{id}/balances` → per-member net balance.
- GET `/groups/{id}/expenses` → paginated list.

Bonus:
- Unequal splits: `splitAmong` as `[{memberId, share}]` summing to 1.0.
- POST `/groups/{id}/settle` with `{ from, to, amount }` records a settlement.
- Balances should reflect settlements.
- Unit test: balance calculation with 3+ expenses.

**Rubric:** the balance calculation is the heart of this — they want to see clean logic for it. Money as decimal not float. Validation that splits sum correctly.

---

## Problem 5: Polls and Voting ⭐⭐

**Build APIs for creating polls and voting on them.**

Requirements:
- POST `/polls` with `{ question, options: ["A", "B", "C"], closesAt }`.
- POST `/polls/{id}/votes` with `{ optionId, voterId }`. One vote per voterId per poll.
- GET `/polls/{id}` → poll with options + current vote counts.
- GET `/polls?status=open|closed`.

Bonus:
- 409 if voting after `closesAt`.
- 409 if voter has already voted (idempotent: same vote again returns 200, different option returns 409).
- DELETE a vote (allow change). Then PUT to change vote.
- Unit test: double-vote rejection. Integration test: vote on closed poll → 409.

**Rubric:** unique constraint (pollId + voterId), time-based state (open/closed derived from `closesAt`), counting in SQL vs in memory.

---

## How to score yourself

After each mock, grade yourself 1–5 on each:

- [ ] Restated problem + asked clarifying questions before typing
- [ ] Sketched endpoints + schema before typing
- [ ] Validation present on at least one DTO
- [ ] Proper status codes (201 + Location, 404, 409 where relevant)
- [ ] At least one happy-path endpoint working end-to-end via Swagger
- [ ] At least one passing test (unit or integration)
- [ ] Code is layered (controller / service / data) — no business logic in controller
- [ ] Walked through what you built at the end

If you score 3+ on most of these by Sunday afternoon, you're ready.
