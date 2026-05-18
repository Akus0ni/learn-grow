namespace ShuruApi.Domain;

public class Table : ITimestamped
{
    public Guid Id { get; set; }
    public string TableNumber { get; set; } = default!;
    public int SeatingCapacity { get; set; }

    public Location Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum Location
{
    Indoor,
    Outdoor
}
