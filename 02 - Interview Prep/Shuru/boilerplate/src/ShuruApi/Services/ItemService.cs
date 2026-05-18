using Microsoft.EntityFrameworkCore;
using ShuruApi.Data;
using ShuruApi.Domain;
using ShuruApi.Dtos;
using ShuruApi.Exceptions;

namespace ShuruApi.Services;

public class ItemService : IItemService
{
    private readonly AppDbContext _db;

    public ItemService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ItemDto>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        return await _db.Items
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemDto(i.Id, i.Name, i.Quantity, i.CreatedAt, i.UpdatedAt))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.Items.CountAsync(ct);

    public async Task<ItemDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return item is null ? null : ToDto(item);
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest req, CancellationToken ct = default)
    {
        var item = new Item { Name = req.Name, Quantity = req.Quantity };
        _db.Items.Add(item);
        await _db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<ItemDto> UpdateAsync(int id, UpdateItemRequest req, CancellationToken ct = default)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id, ct)
                   ?? throw new NotFoundException($"Item {id} not found.");

        item.Name = req.Name;
        item.Quantity = req.Quantity;
        await _db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id, ct)
                   ?? throw new NotFoundException($"Item {id} not found.");
        _db.Items.Remove(item);
        await _db.SaveChangesAsync(ct);
    }

    private static ItemDto ToDto(Item i) =>
        new(i.Id, i.Name, i.Quantity, i.CreatedAt, i.UpdatedAt);
}
