using ShuruApi.Dtos;

namespace ShuruApi.Services;

public interface ITableService
{
    Task<IReadOnlyList<TableDto>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<TableDto?> GetAsync(int id, CancellationToken ct = default);
    Task<TableDto> CreateAsync(CreateTableRequest req, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
