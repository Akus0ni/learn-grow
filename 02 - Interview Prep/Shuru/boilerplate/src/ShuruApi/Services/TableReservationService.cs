using Microsoft.EntityFrameworkCore;
using ShuruApi.Data;
using ShuruApi.Domain;
using ShuruApi.Dtos;
using ShuruApi.Exceptions;

namespace ShuruApi.Services;

public class TableReservationService : ITableReservationService
{
    private readonly AppDbContext _db;

    public TableReservationService(AppDbContext db) => _db = db;

    public async Task<TableReservationDto> CreateAsync(CreateTableReservationRequest req, CancellationToken ct = default)
    {
        var tableRes = new TableReservation { Table = req.Table, Customer = req.Customer, StartTime = req.StartTime, EndTime = req.EndTime };
        _db.TableReservations.Add(tableRes);
        await _db.SaveChangesAsync(ct);
        return ToDto(tableRes);
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TableReservationDto?> GetAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TableReservationDto>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private static TableReservationDto ToDto(TableReservation i) =>
        new(i.Id, i.StartTime, i.EndTime, i.Table, i.Customer, i.CreatedAt, i.UpdatedAt);
}