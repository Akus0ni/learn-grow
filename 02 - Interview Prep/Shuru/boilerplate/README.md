# ShuruApi Boilerplate

A production-shaped ASP.NET Core 8 Web API starter for the Shuru 90-min coding round.

## What's inside

```
boilerplate/
├── ShuruApi.sln
├── src/ShuruApi/
│   ├── Program.cs                       (DI, EF Core, Swagger, middleware, exception handler)
│   ├── appsettings.json                 (SQLite connection string)
│   ├── Controllers/ItemsController.cs   (full CRUD — copy + rename)
│   ├── Services/ItemService.cs          (business logic; throws domain exceptions)
│   ├── Domain/Item.cs                   (entity + ITimestamped)
│   ├── Data/AppDbContext.cs             (DbContext + timestamp override)
│   ├── Dtos/                            (CreateItemRequest, UpdateItemRequest, ItemDto)
│   ├── Exceptions/                      (NotFoundException, ConflictException)
│   └── Middleware/ExceptionHandlingMiddleware.cs   (→ ProblemDetails)
└── tests/ShuruApi.Tests/
    ├── Services/ItemServiceTests.cs                (in-memory DbContext)
    └── Integration/ItemsEndpointsTests.cs          (WebApplicationFactory)
```

## First-run sanity check (do this Friday)

```bash
cd boilerplate
dotnet restore
dotnet build
dotnet test
cd src/ShuruApi
dotnet ef migrations add Initial
dotnet ef database update
dotnet run
```

Open Swagger at the printed URL. Hit `POST /items` with `{ "name": "test", "quantity": 1 }`. Expect 201 + Location header.

## On the day — the 4 commands you need

```bash
# 1. Create migration after changing entities
dotnet ef migrations add <Name> --project src/ShuruApi

# 2. Apply
dotnet ef database update --project src/ShuruApi

# 3. Run
dotnet run --project src/ShuruApi

# 4. Test
dotnet test
```

## How to use it on the day

The boilerplate gives you the **pattern**. You're not going to keep "Item". The flow:

1. **Decide your real entity** (e.g., `Book`, `Task`, `Link`).
2. **Copy `Item.cs`** → `Book.cs`. Rename type, add real properties.
3. **Add `DbSet<Book>` to AppDbContext.** Add any `OnModelCreating` config.
4. **Copy DTOs** → rename, adjust properties.
5. **Copy `ItemService.cs`** → `BookService.cs`. Adjust CRUD logic.
6. **Copy `ItemsController.cs`** → `BooksController.cs`. Adjust routes.
7. **Register service in `Program.cs`**: `builder.Services.AddScoped<IBookService, BookService>();`
8. **Migrate**: `dotnet ef migrations add AddBook && dotnet ef database update`
9. **Run.** Hit Swagger. Show it works.
10. **For additional entities**: repeat 1–8. Each one should take ~5 minutes once you're in the rhythm.

## What's already done for you

✅ EF Core + SQLite configured
✅ DbContext registered in DI
✅ Automatic `CreatedAt` / `UpdatedAt` via `ITimestamped` interface
✅ Swagger UI at `/swagger` in Development
✅ ProblemDetails error responses (RFC 7807) via global exception middleware
✅ Auto-validation 400 from DataAnnotations (no setup needed)
✅ `Program` is `public partial` so integration tests can use `WebApplicationFactory<Program>`
✅ Sample service test (in-memory DbContext)
✅ Sample integration test (WebApplicationFactory + in-memory provider swap)
✅ Layered: Controller → Service → DbContext, DTO ↔ Entity mapping

## What you might add live

- More entities (the whole point of the exercise)
- Pagination on list endpoints (the pattern: see `ListAsync` comment in `ItemService`)
- Filtering / sorting (LINQ `.Where` / `.OrderBy` based on query params)
- A second controller demonstrating a sub-resource action (e.g., `POST /books/{id}/borrow`)
- More unit tests for business rules

## Notes

- `Program` exposing `public partial class Program {}` at the bottom is intentional — required for integration tests.
- SQLite file `shuru.db` is created beside the running exe. Delete it freely between runs if migrations fight you.
- The Item entity is generic on purpose. You're meant to throw it away.
