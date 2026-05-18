namespace ShuruApi.Dtos;

public record ItemDto(int Id, string Name, int Quantity, DateTime CreatedAt, DateTime? UpdatedAt);
