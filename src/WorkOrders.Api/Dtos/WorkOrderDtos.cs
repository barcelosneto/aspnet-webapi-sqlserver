using System.ComponentModel.DataAnnotations;
namespace WorkOrders.Api.Dtos;

public sealed class WorkOrderInput
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }
    [Required, StringLength(2000, MinimumLength = 1)]
    public required string Description { get; init; }
    [Required, RegularExpression("^(open|in_progress|completed)$")]
    public string Status { get; init; } = "open";
    [Range(1, 5)]
    public int Priority { get; init; } = 3;
    public DateOnly DueDate { get; init; }
}

public sealed record WorkOrderOutput(int Id, string Title, string Description, string Status, int Priority, DateOnly DueDate);
