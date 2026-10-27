using Microsoft.EntityFrameworkCore;
using WorkOrders.Api.Data;
using WorkOrders.Api.Services;
using WorkOrders.Api.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DatabaseConnectivityHealthCheck>("database", tags: ["ready"]);
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("demo", limiter =>
    {
        limiter.PermitLimit = 3;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                .ToString(System.Globalization.CultureInfo.InvariantCulture);
        await Results.Problem(statusCode: 429, title: "Too many requests")
            .ExecuteAsync(context.HttpContext);
    };
});
builder.Services.AddScoped<WorkOrderService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("Database") ?? "Data Source=workorders.db";
    var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
    if (provider == "SqlServer") options.UseSqlServer(connection);
    else if (provider == "Sqlite")
    {
        var sqlite = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connection);
        if (!Path.IsPathRooted(sqlite.DataSource) && sqlite.DataSource != ":memory:")
            sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);
        options.UseSqlite(sqlite.ToString());
    }
    else throw new InvalidOperationException("DatabaseProvider must be Sqlite or SqlServer.");
});
var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var business = error as BusinessRuleException;
    var status = business is null ? 500 : 400;
    await Results.Problem(statusCode: status, title: business?.Message ?? "Unexpected server error").ExecuteAsync(context);
}));
app.UseStatusCodePages();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapGet("/examples/limited", () => Results.Ok(new { message = "Demonstration request accepted" }))
    .RequireRateLimiting("demo");
app.Run();
public partial class Program;

