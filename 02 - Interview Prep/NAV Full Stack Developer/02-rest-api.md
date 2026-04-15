# REST API Design — Interview Q&A

---

## Core Concepts

**Q: What is REST? What are its key constraints?**

A: REST (Representational State Transfer) is an architectural style for distributed systems. Key constraints:

1. **Client-Server** — Separation of concerns. UI and data storage are decoupled.
2. **Stateless** — Each request contains all information needed. Server stores no session state.
3. **Cacheable** — Responses must define whether they can be cached.
4. **Uniform Interface** — Consistent way to interact (resource-based URIs, standard HTTP methods).
5. **Layered System** — Client doesn't know if it's talking to end server or intermediary (proxy, load balancer).
6. **Code on Demand** (optional) — Server can send executable code to client.

---

**Q: What are the HTTP methods and when do you use each?**

| Method | Purpose | Idempotent | Safe |
|---|---|---|---|
| `GET` | Retrieve resource | ✅ | ✅ |
| `POST` | Create resource | ❌ | ❌ |
| `PUT` | Replace entire resource | ✅ | ❌ |
| `PATCH` | Partial update | ❌ | ❌ |
| `DELETE` | Remove resource | ✅ | ❌ |
| `OPTIONS` | Get allowed methods | ✅ | ✅ |

- **Idempotent** — Multiple identical requests produce the same result as one.
- **Safe** — Does not modify server state.

---

**Q: What are the common HTTP status codes?**

| Code | Meaning | When to use |
|---|---|---|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST (include Location header) |
| 204 | No Content | Successful DELETE or action with no body |
| 400 | Bad Request | Invalid input, validation failure |
| 401 | Unauthorized | Not authenticated |
| 403 | Forbidden | Authenticated but not authorized |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate resource, version conflict |
| 422 | Unprocessable Entity | Validation errors (semantic errors) |
| 500 | Internal Server Error | Unexpected server failure |
| 503 | Service Unavailable | Server overloaded / down |

---

**Q: How do you design a RESTful API for a resource like "Orders"?**

```
GET    /api/orders          → list all orders
GET    /api/orders/{id}     → get specific order
POST   /api/orders          → create order
PUT    /api/orders/{id}     → replace order
PATCH  /api/orders/{id}     → partial update
DELETE /api/orders/{id}     → delete order

GET    /api/orders/{id}/items   → get items for an order (nested resource)
```

**Naming conventions:**
- Use nouns, not verbs (`/orders`, not `/getOrders`)
- Plural nouns for collections
- Lowercase with hyphens (`/fund-transfers`, not `/FundTransfers`)
- Nest related resources up to 2 levels

---

**Q: What is versioning in REST APIs and how do you implement it?**

A: API versioning prevents breaking changes for existing clients when the API evolves.

**Common approaches:**

1. **URI versioning** (most common, easy to see)
   ```
   /api/v1/orders
   /api/v2/orders
   ```

2. **Header versioning**
   ```
   GET /api/orders
   api-version: 2.0
   ```

3. **Query string**
   ```
   /api/orders?api-version=2.0
   ```

In ASP.NET Core, use `Microsoft.AspNetCore.Mvc.Versioning` package.

---

**Q: What is the difference between authentication and authorization?**

A:
- **Authentication** — Who are you? Verifying identity (JWT, API key, OAuth).
- **Authorization** — What can you do? Checking permissions after identity is confirmed (roles, policies, claims).

In ASP.NET Core:
- `[Authorize]` — Requires authentication
- `[Authorize(Roles = "Admin")]` — Requires specific role
- `[Authorize(Policy = "CanReadReports")]` — Custom policy

---

**Q: What is JWT? How does it work?**

A: JSON Web Token — a compact, self-contained token for transmitting claims between parties.

**Structure:** `header.payload.signature`

```
eyJhbGciOiJIUzI1NiJ9.  ← header (alg + type)
eyJzdWIiOiIxMjM0NSJ9.  ← payload (claims: sub, exp, roles...)
SflKxwRJSMeKKF2QT4fw   ← signature (HMAC of header+payload+secret)
```

**Flow:**
1. Client sends credentials → Server validates → Returns JWT
2. Client stores JWT (localStorage or httpOnly cookie)
3. Client sends JWT in `Authorization: Bearer <token>` header
4. Server validates signature + expiry → Grants access

> **From your resume:** You implemented JWT auth at IKS Health, resolving critical security vulnerabilities.

---

**Q: What is CORS and why is it needed?**

A: Cross-Origin Resource Sharing — browser security mechanism that restricts HTTP requests from one origin to another.

Example: Frontend on `http://localhost:3000` calling API on `http://localhost:5000` is cross-origin.

In ASP.NET Core:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://myapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowFrontend");
```

---

**Q: What is the difference between REST and GraphQL?**

| | REST | GraphQL |
|---|---|---|
| Endpoints | Multiple (one per resource) | Single endpoint |
| Data fetching | Fixed response shape | Client specifies exact fields |
| Over-fetching | Common | Avoided |
| Under-fetching | Requires multiple calls | Single query handles it |
| Versioning | Required | Not needed (evolve schema) |
| Use case | Standard APIs | Complex, flexible data needs |

---

**Q: How do you handle errors in a REST API consistently?**

A: Return structured error responses:

```json
{
  "status": 400,
  "title": "Validation failed",
  "traceId": "abc123",
  "errors": {
    "Amount": ["Amount must be greater than 0"],
    "Currency": ["Currency is required"]
  }
}
```

In ASP.NET Core, use `ProblemDetails` (RFC 7807) — built-in with `app.UseExceptionHandler()` and `AddProblemDetails()`.

---

**Q: What is pagination in APIs and how do you implement it?**

A: Pagination prevents returning massive datasets. Two main approaches:

**Offset pagination:**
```
GET /api/orders?page=2&pageSize=20
```
Response includes:
```json
{
  "data": [...],
  "totalCount": 500,
  "page": 2,
  "pageSize": 20,
  "totalPages": 25
}
```

**Cursor pagination:** Uses a cursor (last seen ID) instead of page number. Better for large/real-time datasets.

---

**Q: What is the Richardson Maturity Model?**

A: A way to grade REST API maturity:

- **Level 0** — One URI, one HTTP method (RPC style, SOAP)
- **Level 1** — Multiple URIs (resources), but only GET/POST
- **Level 2** — Uses correct HTTP verbs + status codes ← *Most APIs live here*
- **Level 3** — HATEOAS: responses include links to related actions

---

## In .NET Core Context

**Q: How do you create a REST API controller in ASP.NET Core?**

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _service.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

**Key attributes:**
- `[ApiController]` — Enables automatic model validation, binding source inference
- `[Route]` — Defines URL template
- `ControllerBase` — No View support (use `Controller` for MVC with Views)
