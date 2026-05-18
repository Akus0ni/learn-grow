using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShuruApi.Data;
using ShuruApi.Dtos;
using Xunit;

namespace ShuruApi.Tests.Integration;

public class ItemsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ItemsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(o =>
                    o.UseInMemoryDatabase($"itests-{Guid.NewGuid()}"));
            });
        });
    }

    [Fact]
    public async Task Post_ValidItem_Returns201WithLocation()
    {
        var client = _factory.CreateClient();

        var res = await client.PostAsJsonAsync("/items", new CreateItemRequest { Name = "Widget", Quantity = 3 });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
        var body = await res.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(body);
        Assert.Equal("Widget", body!.Name);
    }

    [Fact]
    public async Task Get_MissingId_Returns404()
    {
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/items/9999");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidBody_Returns400()
    {
        var client = _factory.CreateClient();

        var res = await client.PostAsJsonAsync("/items", new { name = "", quantity = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
