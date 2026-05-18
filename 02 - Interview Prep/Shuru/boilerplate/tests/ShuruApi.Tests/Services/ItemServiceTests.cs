using Microsoft.EntityFrameworkCore;
using ShuruApi.Data;
using ShuruApi.Dtos;
using ShuruApi.Exceptions;
using ShuruApi.Services;
using Xunit;

namespace ShuruApi.Tests.Services;

public class ItemServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateAsync_ValidInput_PersistsAndReturnsDto()
    {
        await using var db = NewDb();
        var sut = new ItemService(db);

        var dto = await sut.CreateAsync(new CreateItemRequest { Name = "Widget", Quantity = 5 });

        Assert.NotEqual(0, dto.Id);
        Assert.Equal("Widget", dto.Name);
        Assert.Equal(5, dto.Quantity);
        Assert.Single(db.Items);
    }

    [Fact]
    public async Task UpdateAsync_MissingId_ThrowsNotFound()
    {
        await using var db = NewDb();
        var sut = new ItemService(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(99, new UpdateItemRequest { Name = "x", Quantity = 1 }));
    }

    [Fact]
    public async Task DeleteAsync_ExistingItem_RemovesIt()
    {
        await using var db = NewDb();
        var sut = new ItemService(db);
        var created = await sut.CreateAsync(new CreateItemRequest { Name = "x", Quantity = 1 });

        await sut.DeleteAsync(created.Id);

        Assert.Empty(db.Items);
    }
}
