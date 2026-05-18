using ShuruApi.Dtos;

namespace ShuruApi.Services;

public interface ITableReservationService
{
    Task<IReadOnlyList<TableReservationDto>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<TableReservationDto?> GetAsync(int id, CancellationToken ct = default);
    Task<TableReservationDto> CreateAsync(CreateTableReservationRequest req, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
