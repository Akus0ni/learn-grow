using Microsoft.EntityFrameworkCore;
using ShuruApi.Data;
using ShuruApi.Middleware;
using ShuruApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
    var connectionString = builder.Configuration.GetConnectionString(provider)
        ?? throw new InvalidOperationException(
            $"Missing connection string 'ConnectionStrings:{provider}' for provider '{provider}'.");

    switch (provider)
    {
        case "Sqlite":
            opt.UseSqlite(connectionString);
            break;
        case "SqlServer":
            opt.UseSqlServer(connectionString);
            break;
        default:
            throw new InvalidOperationException(
                $"Unsupported Database:Provider '{provider}'. Use 'Sqlite' or 'SqlServer'.");
    }
});

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<ITableReservationService, TableReservationService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Auto-apply migrations on startup so the DB exists when you run the app.
// In production you'd run migrations as a separate step; for an interview demo this is fine.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
