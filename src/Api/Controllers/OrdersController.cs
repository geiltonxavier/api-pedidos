using System;
using System.Threading.Tasks;
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
}
