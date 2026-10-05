using Microsoft.EntityFrameworkCore;
using WorkOrders.Api.Models;
namespace WorkOrders.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<WorkOrder>();
        order.Property(x => x.Title).HasMaxLength(200).IsRequired();
        order.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        order.Property(x => x.Status).HasMaxLength(20).IsRequired();
    }
}
