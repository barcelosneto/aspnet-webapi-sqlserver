using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WorkOrders.Api.Tests;

public sealed class OperationalTests
{
    [Fact]
    public async Task HealthyDatabaseMakesReadinessSuccessful()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task DatabaseFailureDoesNotFailLiveness()
    {
        using var factory = new ApiFactory { DatabaseUnavailable = true };
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("sqlite", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FourthRequestReturns429WithRetryAfterAndProblemDetails()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/examples/limited")).StatusCode);
        var rejected = await client.GetAsync("/examples/limited");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(rejected.Headers.RetryAfter);
        var problem = await rejected.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(429, problem.GetProperty("status").GetInt32());
        // A política demonstrativa não bloqueia os endpoints de saúde.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
    }

    [Fact]
    public async Task ValidationErrorHasStructuredProblemDetails()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateInitializedClient();
        var response = await client.PostAsJsonAsync("/api/work-orders", new { title = "", priority = 9 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, error.GetProperty("status").GetInt32());
        Assert.True(error.TryGetProperty("errors", out var errors));
        Assert.NotEmpty(errors.EnumerateObject());
    }
}
