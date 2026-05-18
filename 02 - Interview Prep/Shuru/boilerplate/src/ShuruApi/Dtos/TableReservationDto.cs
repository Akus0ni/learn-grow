using ShuruApi.Domain;

namespace ShuruApi.Dtos;
public record TableReservationDto(Guid Id, DateTime StartTime, DateTime EndTime, Table Table, Customer Customer, DateTime CreatedAt, DateTime? UpdatedAt);