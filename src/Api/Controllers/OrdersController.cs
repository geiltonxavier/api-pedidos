using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Filters;
using Application.DTO;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/orders")]
[EnableRateLimiting("fixed")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [ProducesResponseType(typeof(CreatedOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var response = await _service.CreateOrderAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { orderId = response.OrderId, version = "1.0" }, response);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken ct)
    {
        var order = await _service.GetOrderAsync(orderId, ct);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPut("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(OrderSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
