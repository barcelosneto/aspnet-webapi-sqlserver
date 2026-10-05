namespace WorkOrders.Api.Models;

public sealed class WorkOrder
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string Status { get; set; } = "open";
    public int Priority { get; set; } = 3;
    public DateOnly DueDate { get; set; }
}
