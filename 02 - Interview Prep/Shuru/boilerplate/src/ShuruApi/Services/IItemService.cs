using ShuruApi.Dtos;

namespace ShuruApi.Services;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<ItemDto?> GetAsync(int id, CancellationToken ct = default);
    Task<ItemDto> CreateAsync(CreateItemRequest req, CancellationToken ct = default);
    Task<ItemDto> UpdateAsync(int id, UpdateItemRequest req, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
