using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Discounts;
using Application.DTO;
using Application.Services;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Xunit;

namespace Tests;

public class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _store = new();

    public Order? LastAdded { get; private set; }

    public Task AddAsync(Order order)
    {
        _store[order.Id] = order;
        LastAdded = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task UpdateAsync(Order order)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }
}

public class OrderServiceTests
{
    private readonly FakeOrderRepository _repo = new();

    private OrderService CreateService()
    {
        var strategies = new List<IDiscountStrategy>
        {
            new StandardDiscount(),
            new ExpressDiscount(),
            new SubscriptionDiscount()
        };
        var factory = new DiscountFactory(strategies);
        return new OrderService(factory, _repo);
    }

    [Fact]
    public async Task CreateOrder_Standard_NoDiscount()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>
        {
            new("Produto A", 2, 10m)
        });

        var response = await svc.CreateOrderAsync(dto);

        Assert.NotEqual(Guid.Empty, response.OrderId);
        Assert.Equal(20m, _repo.LastAdded!.SubTotal);
        Assert.Equal(20m, _repo.LastAdded.Total);
    }

    [Fact]
    public async Task CreateOrder_Express_Applies15PercentSurcharge()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Express, new List<CreateItemDto>
        {
            new("Produto B", 1, 100m)
        });

        var response = await svc.CreateOrderAsync(dto);

        Assert.Equal(100m, _repo.LastAdded!.SubTotal);
        Assert.Equal(115m, _repo.LastAdded.Total);
    }

    [Fact]
    public async Task CreateOrder_Subscription_Applies10PercentDiscount()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Subscription, new List<CreateItemDto>
        {
            new("Produto C", 3, 50m)
        });

        var response = await svc.CreateOrderAsync(dto);

        Assert.Equal(150m, _repo.LastAdded!.SubTotal);
        Assert.Equal(135m, _repo.LastAdded.Total);
    }

    [Fact]
    public async Task GetOrder_ReturnsCorrectSummary()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Subscription, new List<CreateItemDto>
        {
            new("Produto D", 2, 40m)
        });

        var created = await svc.CreateOrderAsync(dto);
        var summary = await svc.GetOrderAsync(created.OrderId);

        Assert.NotNull(summary);
        Assert.Equal(80m, summary!.SubTotal);
        Assert.Equal(72m, summary.Total);
        Assert.Equal(8m, summary.DiscountValue);
        Assert.Single(summary.Items);
    }

    [Fact]
    public async Task GetOrder_NonExistentId_ReturnsNull()
    {
        var svc = CreateService();

        var summary = await svc.GetOrderAsync(Guid.NewGuid());

        Assert.Null(summary);
    }
}
