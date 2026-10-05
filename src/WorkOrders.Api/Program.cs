using Microsoft.EntityFrameworkCore;
using WorkOrders.Api.Data;
using WorkOrders.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();
public partial class Program;

