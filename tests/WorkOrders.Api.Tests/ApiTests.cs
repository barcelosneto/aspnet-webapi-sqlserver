using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkOrders.Api.Data;
using WorkOrders.Api.Dtos;
using Xunit;

namespace WorkOrders.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public bool DatabaseUnavailable { get; init; }
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public ApiFactory() => connection.Open();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                if (DatabaseUnavailable)
                    options.UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".sqlite")};Mode=ReadOnly");
                else options.UseSqlite(connection);
            });
        });
    }
    public HttpClient CreateInitializedClient()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        return client;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }
}

public sealed class ApiTests
{
    private static WorkOrderInput Input() => new() { Title = "FICTITIOUS maintenance", Description = "FICTITIOUS database review", DueDate = new(2025, 1, 20), Priority = 2 };

    [Fact]
    public async Task FullCrudPersistsAndDeletes()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        var created = await client.PostAsJsonAsync("/api/work-orders", Input());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var order = await created.Content.ReadFromJsonAsync<WorkOrderOutput>();
        Assert.NotNull(order);
        Assert.NotNull(created.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/work-orders/{order.Id}")).StatusCode);
        var updated = await client.PutAsJsonAsync($"/api/work-orders/{order.Id}", new WorkOrderInput { Title = "FICTITIOUS updated", Description = "FICTITIOUS done", Status = "completed", DueDate = new(2025,1,20) });
        Assert.Equal("completed", (await updated.Content.ReadFromJsonAsync<WorkOrderOutput>())!.Status);
        Assert.Single((await client.GetFromJsonAsync<List<WorkOrderOutput>>("/api/work-orders"))!);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/work-orders/{order.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/work-orders/{order.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("", 3, "open", "2025-01-20")]
    [InlineData("   ", 3, "open", "2025-01-20")]
    [InlineData("FICTITIOUS", 0, "open", "2025-01-20")]
    [InlineData("FICTITIOUS", 6, "open", "2025-01-20")]
    [InlineData("FICTITIOUS", 3, "invalid", "2025-01-20")]
    [InlineData("FICTITIOUS", 3, "open", "0001-01-01")]
    public async Task InvalidInputReturnsBadRequest(string title, int priority, string status, string dueDate)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        var response = await client.PostAsJsonAsync("/api/work-orders", new { title, description = "FICTITIOUS description", priority, status, dueDate });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownResourcesAndSwagger()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/work-orders/999", Input())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/work-orders/999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }
}
