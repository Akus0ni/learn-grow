# REST API Design — Quick Reference

## Resource naming
- **Plural nouns**: `/books`, `/users`, `/orders` — never `/getBooks`, `/createUser`.
- **Hierarchy** when child only makes sense under parent: `/authors/{authorId}/books`.
- **Flat** when child has its own identity: `/books/{id}` + filter `?authorId=...`.
- Lowercase, hyphens for multi-word: `/order-items` not `/OrderItems`.
- No verbs in URLs. Verbs are HTTP methods.
- Actions that don't fit CRUD: `POST /orders/{id}/cancel` — a sub-resource action is acceptable.

## HTTP methods (and which are idempotent)

| Method | Use | Idempotent? | Safe? |
|---|---|---|---|
| GET | Read | Yes | Yes |
| POST | Create / non-idempotent action | No | No |
| PUT | Replace entire resource | Yes | No |
| PATCH | Partial update | No (technically) | No |
| DELETE | Remove | Yes | No |

Idempotent = same call N times has same effect as 1 call.

## Status codes you'll actually use

| Code | When |
|---|---|
| 200 OK | Successful GET / PUT / PATCH with body |
| 201 Created | Successful POST. **Include `Location` header** pointing to new resource. |
| 204 No Content | Successful DELETE, or PUT/PATCH with no body |
| 400 Bad Request | Malformed input, validation failure |
| 401 Unauthorized | No / invalid auth token |
| 403 Forbidden | Authed but not allowed |
| 404 Not Found | Resource doesn't exist |
| 409 Conflict | State conflict (duplicate key, can't transition state, optimistic concurrency) |
| 422 Unprocessable Entity | Semantically wrong (rare; many people use 400 instead — pick one and stay consistent) |
| 500 Internal Server Error | Unhandled exception. Should be rare. |

## Pagination
```
GET /books?page=1&pageSize=20&sort=-createdAt
```
Response:
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "total": 137
}
```
Or use `X-Total-Count` header for `total` and keep body as plain array. Either is fine — be consistent.

## Filtering and sorting
- Filter: `?status=active&authorId=42`
- Sort: `?sort=createdAt` (asc) or `?sort=-createdAt` (desc). Multiple: `?sort=-createdAt,title`.

## Request/response shape
- Always wrap collections in a body. Don't return bare arrays at the top level (security best practice + makes it extensible).
- Use camelCase in JSON. ASP.NET Core does this by default.
- ISO 8601 for dates: `2026-05-18T11:00:00Z`. Same default in ASP.NET Core.

## Versioning
- URL: `/v1/books` — simplest, most explicit.
- Header: `Accept: application/vnd.shuru.v1+json` — cleaner URLs.
- Pick URL for an interview. It's faster to demonstrate.

## ProblemDetails (RFC 7807) — error response shape
```json
{
  "type": "https://example.com/probs/not-found",
  "title": "Resource not found",
  "status": 404,
  "detail": "Book with id 42 does not exist",
  "instance": "/books/42"
}
```
ASP.NET Core: `Results.Problem(...)` or throw a custom exception → middleware → ProblemDetails. Boilerplate is wired for this.

## Quick mental model for endpoint design
For every resource, ask:
1. List → `GET /resource` (paginated, filterable)
2. Read one → `GET /resource/{id}`
3. Create → `POST /resource` → 201 + Location
4. Replace → `PUT /resource/{id}`
5. Partial update → `PATCH /resource/{id}` (skip if PUT is enough — simpler)
6. Delete → `DELETE /resource/{id}` → 204

Then ask: are there actions that don't fit? (`/cancel`, `/return`, `/publish`) → those are sub-resource POSTs.
