using Microsoft.EntityFrameworkCore;
using ShuruApi.Data;
using ShuruApi.Domain;
using ShuruApi.Dtos;
using ShuruApi.Exceptions;

namespace ShuruApi.Services;

public class TableService : ITableService
{
    private readonly AppDbContext _db;

    public TableService(AppDbContext db) => _db = db;

    public async Task<TableDto> CreateAsync(CreateTableRequest req, CancellationToken ct = default)
    {
        var table = new Table { Id = Guid.NewGuid(), TableNumber = req.TableNumber, SeatingCapacity = req.SeatingCapacity, Location = req.Location};
        _db.Tables.Add(table);
        await _db.SaveChangesAsync(ct);
        return ToDto(table);
    }

    private static TableDto ToDto(Table i) =>
        new(i.Id, i.TableNumber, i.SeatingCapacity, i.Location, i.CreatedAt, i.UpdatedAt);

    Task<IReadOnlyList<TableDto>> ITableService.ListAsync(int page, int pageSize, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    Task<TableDto?> ITableService.GetAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}