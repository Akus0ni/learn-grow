# Unit + Integration Testing — Quick Reference

The PDF explicitly calls out tests as a bonus. Even **one** good test is a strong signal.

## xUnit basics

```csharp
public class BookServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidInput_PersistsBook()
    {
        // Arrange
        var db = InMemoryDb();
        var sut = new BookService(db);

        // Act
        var result = await sut.CreateAsync(new CreateBookRequest("Dune", authorId: 1));

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Dune", result.Title);
    }
}
```

Naming: `Method_State_ExpectedOutcome` — easy to scan failing tests.

## Theory + InlineData (parameterized)
```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
[InlineData(null)]
public async Task CreateAsync_InvalidTitle_Throws(string title)
{
    var sut = new BookService(InMemoryDb());
    await Assert.ThrowsAsync<ValidationException>(
        () => sut.CreateAsync(new CreateBookRequest(title, 1)));
}
```

## In-memory DbContext for service tests
```csharp
private static AppDbContext InMemoryDb()
{
    var opts = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new AppDbContext(opts);
}
```
Unique name per test → no cross-test pollution.

## Integration tests with `WebApplicationFactory`

```csharp
public class BooksEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BooksEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureServices(services =>
            {
                // swap real DB for in-memory
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(o =>
                    o.UseInMemoryDatabase("integration-tests"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task POST_Books_Returns201WithLocation()
    {
        var res = await _client.PostAsJsonAsync("/books",
            new { title = "Dune", authorId = 1 });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
    }
}
```

Requires `Program` to be public-ish. Add to bottom of `Program.cs`:
```csharp
public partial class Program {}
```
Boilerplate already has it.

## What to test (in priority order, for 90 min)
1. **Happy path** for create — proves the wiring works.
2. **Validation** — invalid input returns 400.
3. **Not found** — GET/PUT/DELETE on missing id returns 404.
4. **Conflict** — domain rule violation returns 409 (if applicable).
5. **Idempotency** — DELETE twice doesn't blow up.

## Mocking (Moq) — usually unnecessary at this level
If you need it: `dotnet add package Moq`
```csharp
var repo = new Mock<IBookRepository>();
repo.Setup(r => r.GetAsync(1)).ReturnsAsync(new Book { Id = 1 });
var sut = new BookService(repo.Object);
```
But honestly, with `UseInMemoryDatabase` you rarely need Moq. **Skip Moq if you can** — fewer concepts to juggle live.

## Test project structure (boilerplate has this)
```
tests/ShuruApi.Tests/
├── ShuruApi.Tests.csproj         (xunit + Microsoft.AspNetCore.Mvc.Testing)
├── Services/
│   └── ItemServiceTests.cs
└── Integration/
    └── ItemsEndpointsTests.cs
```

Run: `dotnet test` from solution root.
