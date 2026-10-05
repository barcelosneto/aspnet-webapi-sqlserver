using Microsoft.AspNetCore.Mvc;
using WorkOrders.Api.Dtos;
using WorkOrders.Api.Services;
namespace WorkOrders.Api.Controllers;

[ApiController]
[Route("api/work-orders")]
public sealed class WorkOrdersController(WorkOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkOrderOutput>>> List(CancellationToken token) => Ok(await service.List(token));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderOutput>> Get(int id, CancellationToken token)
    {
        var order = await service.Get(id, token);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderOutput>> Create(WorkOrderInput input, CancellationToken token)
    {
        var order = await service.Create(input, token);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkOrderOutput>> Update(int id, WorkOrderInput input, CancellationToken token)
    {
        var order = await service.Update(id, input, token);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken token) => await service.Delete(id, token) ? NoContent() : NotFound();
}
