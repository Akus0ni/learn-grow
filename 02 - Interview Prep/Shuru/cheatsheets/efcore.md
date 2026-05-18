# EF Core 8 — Cheat Sheet

## Setup (already in boilerplate, but know the commands)

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef     # one-time per machine
```

## Registering DbContext (`Program.cs`)
```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));
```
Connection string in `appsettings.json`:
```json
"ConnectionStrings": { "Default": "Data Source=shuru.db" }
```

## Entity + DbContext

```csharp
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Book>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Title);
            e.HasOne(x => x.Author)
             .WithMany(a => a.Books)
             .HasForeignKey(x => x.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

## Migrations
```bash
dotnet ef migrations add Initial
dotnet ef database update
dotnet ef migrations remove           # undo last migration if not applied
```
Run from the project folder containing the csproj, or use `--project` and `--startup-project`.

## Querying

```csharp
// Read all
var books = await db.Books.AsNoTracking().ToListAsync();

// By id
var book = await db.Books.FindAsync(id);                  // returns null if missing
var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id);

// Filter + sort + page
var page = await db.Books
    .AsNoTracking()
    .Where(b => b.AuthorId == authorId)
    .OrderByDescending(b => b.CreatedAt)
    .Skip((pageNum - 1) * pageSize).Take(pageSize)
    .ToListAsync();

// With related data
var book = await db.Books.Include(b => b.Author).FirstOrDefaultAsync(...);

// Projection (faster — no tracking, only the columns you need)
var dto = await db.Books
    .Where(b => b.Id == id)
    .Select(b => new BookDto(b.Id, b.Title, b.Author.Name))
    .FirstOrDefaultAsync();
```

## Mutations
```csharp
db.Books.Add(book);
await db.SaveChangesAsync();          // book.Id is now populated

db.Books.Update(book);                // marks all properties modified
await db.SaveChangesAsync();

db.Books.Remove(book);
await db.SaveChangesAsync();
```

## Common gotchas
- **`AsNoTracking()`** on reads when you won't save changes → faster.
- **`Include`** for eager loading. Avoid lazy loading in APIs — it causes N+1.
- **Migrations from the right folder.** If `dotnet ef` complains, pass `--project src/ShuruApi --startup-project src/ShuruApi`.
- **SQLite quirks**: limited ALTER TABLE. EF Core handles most of it but if a migration fails on SQLite, you can drop the file (`shuru.db`) and re-apply — fine in an interview.
- **Concurrency**: add a `[Timestamp] public byte[] RowVersion { get; set; }` on entities that need it. Probably overkill for a 90-min interview.

## Override SaveChanges for timestamps
```csharp
public override Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var now = DateTime.UtcNow;
    foreach (var e in ChangeTracker.Entries<ITimestamped>())
    {
        if (e.State == EntityState.Added)    e.Entity.CreatedAt = now;
        if (e.State == EntityState.Modified) e.Entity.UpdatedAt = now;
    }
    return base.SaveChangesAsync(ct);
}
```
The boilerplate has this pattern.

## In-memory provider (for unit tests)
```csharp
var opts = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())   // unique per test
    .Options;
using var db = new AppDbContext(opts);
```
Needs `Microsoft.EntityFrameworkCore.InMemory` package. Boilerplate test project has it.
