using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Filters;
using Application.DTO;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/orders")]
[EnableRateLimiting("fixed")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var response = await _service.CreateOrderAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { orderId = response.OrderId, version = "1.0" }, response);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken ct)
    {
        var order = await _service.GetOrderAsync(orderId, ct);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPut("{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid orderId, Guid itemId, [FromBody] UpdateItemDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateItemAsync(orderId, itemId, dto, ct);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken ct)
    {
        var removed = await _service.RemoveItemAsync(orderId, itemId, ct);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
