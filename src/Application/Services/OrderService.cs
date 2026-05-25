using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Discounts;
using Application.DTO;
using Application.Helpers;
using Application.Interfaces;
using Core.Entities;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly DiscountFactory _factory;

    public OrderService(DiscountFactory factory, IOrderRepository repo)
    {
        _factory = factory;
        _repo = repo;
    }

    public async Task<CreatedOrderResponse> CreateOrderAsync(CreateOrderDto dto)
    {
        var items = dto.ToDomainItems();
        var order = new Order
        {
            Type = dto.Type,
            Items = items
        };

        order.RecalculateSubTotal();

        var strategy = _factory.GetStrategy(dto.Type);
        order.SetTotal(strategy.CalculateTotal(order));

        await _repo.AddAsync(order);

        return new CreatedOrderResponse(order.Id);
    }

    public async Task<OrderSummaryDto?> GetOrderAsync(Guid id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order?.ToSummaryDto();
    }
}
