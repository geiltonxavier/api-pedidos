using System;
using System.Threading.Tasks;
using Api.Filters;
using Application.DTO;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var response = await _service.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { orderId = response.OrderId }, response);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        var order = await _service.GetOrderAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPut("{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid orderId, Guid itemId, [FromBody] UpdateItemDto dto)
    {
        var result = await _service.UpdateItemAsync(orderId, itemId, dto);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId)
    {
        var removed = await _service.RemoveItemAsync(orderId, itemId);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
