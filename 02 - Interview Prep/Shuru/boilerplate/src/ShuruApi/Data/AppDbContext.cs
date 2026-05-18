using Microsoft.EntityFrameworkCore;
using ShuruApi.Domain;

namespace ShuruApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TableReservation> TableReservations => Set<TableReservation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Item>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Name);
        });

        b.Entity<Table>(e =>
        {
            e.Property(x => x.Id).IsRequired();
            e.Property(x => x.TableNumber).IsRequired().HasMaxLength(50);
            e.Property(x => x.SeatingCapacity).IsRequired();
            e.HasIndex(x => x.TableNumber);
        });

        b.Entity<Customer>(e =>
        {
            e.Property(x => x.Id).IsRequired();
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Name);
        });

        b.Entity<TableReservation>(e =>
        {
                e.HasOne(tr => tr.Table)
                .WithMany()
                .HasForeignKey("TableId")
                .OnDelete(DeleteBehavior.Cascade);
    
                e.HasOne(tr => tr.Customer)
                .WithMany()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<ITimestamped>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(ct);
    }
}
