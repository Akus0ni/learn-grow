namespace ShuruApi.Domain;

public class Item : ITimestamped
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
