using ShuruApi.Domain;

namespace ShuruApi.Dtos;
public record TableDto(Guid Id, string TableNumber, int SeatingCapacity, Location Location, DateTime CreatedAt, DateTime? UpdatedAt);