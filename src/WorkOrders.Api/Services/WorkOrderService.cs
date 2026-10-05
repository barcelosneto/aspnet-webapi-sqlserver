using Microsoft.EntityFrameworkCore;
using WorkOrders.Api.Data;
using WorkOrders.Api.Dtos;
using WorkOrders.Api.Models;
namespace WorkOrders.Api.Services;

public sealed class BusinessRuleException(string message) : Exception(message);

public sealed class WorkOrderService(AppDbContext db)
{
    private static WorkOrderOutput Map(WorkOrder order) => new(order.Id, order.Title, order.Description, order.Status, order.Priority, order.DueDate);

    private static void Validate(WorkOrderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Description))
            throw new BusinessRuleException("Title and description must not be blank.");
        if (input.DueDate == default)
            throw new BusinessRuleException("DueDate is required.");
    }

    public async Task<IReadOnlyList<WorkOrderOutput>> List(CancellationToken token) =>
        (await db.WorkOrders.AsNoTracking().OrderBy(x => x.Id).ToListAsync(token)).Select(Map).ToList();

    public async Task<WorkOrderOutput?> Get(int id, CancellationToken token)
    {
        var order = await db.WorkOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, token);
        return order is null ? null : Map(order);
    }

    public async Task<WorkOrderOutput> Create(WorkOrderInput input, CancellationToken token)
    {
        Validate(input);
        var order = new WorkOrder { Title = input.Title.Trim(), Description = input.Description.Trim(), Status = input.Status, Priority = input.Priority, DueDate = input.DueDate };
        db.WorkOrders.Add(order);
        await db.SaveChangesAsync(token);
        return Map(order);
    }

    public async Task<WorkOrderOutput?> Update(int id, WorkOrderInput input, CancellationToken token)
    {
        Validate(input);
        var order = await db.WorkOrders.FirstOrDefaultAsync(x => x.Id == id, token);
        if (order is null) return null;
        order.Title = input.Title.Trim();
        order.Description = input.Description.Trim();
        order.Status = input.Status;
        order.Priority = input.Priority;
        order.DueDate = input.DueDate;
        await db.SaveChangesAsync(token);
        return Map(order);
    }

    public async Task<bool> Delete(int id, CancellationToken token)
    {
        var order = await db.WorkOrders.FirstOrDefaultAsync(x => x.Id == id, token);
        if (order is null) return false;
        db.WorkOrders.Remove(order);
        await db.SaveChangesAsync(token);
        return true;
    }
}
