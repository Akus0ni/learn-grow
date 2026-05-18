namespace ShuruApi.Domain;

public class TableReservation : ITimestamped
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Table Table { get; set; }
    public Customer Customer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}