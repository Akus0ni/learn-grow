namespace ShuruApi.Domain;

public class Customer : ITimestamped
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int GuestCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}