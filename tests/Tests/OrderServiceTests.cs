using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Discounts;
using Application.DTO;
using Application.Services;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Xunit;

using Microsoft.Extensions.Options;

namespace Tests;

public class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _store = new();

    public Order? LastAdded { get; private set; }

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        LastAdded = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
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
        var options = Options.Create(new PricingOptions());
        var strategies = new List<IPricingStrategy>
        {
            new StandardDiscount(),
            new ExpressDiscount(options),
            new SubscriptionDiscount(options)
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

    [Fact]
    public async Task UpdateItem_ChangesValuesAndRecalculatesTotal()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>
        {
            new("Camiseta", 1, 50m)
        });

        var created = await svc.CreateOrderAsync(dto);
        var order = await svc.GetOrderAsync(created.OrderId);
        var itemId = order!.Items[0].Id;

        var updated = await svc.UpdateItemAsync(created.OrderId, itemId, new UpdateItemDto("Camiseta G", 3, 60m));

        Assert.NotNull(updated);
        Assert.Equal(180m, updated!.SubTotal);
        Assert.Equal(180m, updated.Total);
        Assert.Equal("Camiseta G", updated.Items[0].Description);
    }

    [Fact]
    public async Task UpdateItem_NonExistentItem_ReturnsNull()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>
        {
            new("Produto", 1, 10m)
        });

        var created = await svc.CreateOrderAsync(dto);

        var result = await svc.UpdateItemAsync(created.OrderId, Guid.NewGuid(), new UpdateItemDto("X", 1, 10m));

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveItem_RemovesAndRecalculatesTotal()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Subscription, new List<CreateItemDto>
        {
            new("Item A", 1, 100m),
            new("Item B", 1, 50m)
        });

        var created = await svc.CreateOrderAsync(dto);
        var order = await svc.GetOrderAsync(created.OrderId);
        var itemToRemove = order!.Items.First(i => i.Description == "Item B").Id;

        var removed = await svc.RemoveItemAsync(created.OrderId, itemToRemove);

        Assert.True(removed);

        var updated = await svc.GetOrderAsync(created.OrderId);
        Assert.Single(updated!.Items);
        Assert.Equal(100m, updated.SubTotal);
        Assert.Equal(90m, updated.Total);
    }

    [Fact]
    public async Task RemoveItem_LastItem_ReturnsFalse()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>
        {
            new("Unico Item", 1, 50m)
        });

        var created = await svc.CreateOrderAsync(dto);
        var order = await svc.GetOrderAsync(created.OrderId);
        var itemId = order!.Items[0].Id;

        var result = await svc.RemoveItemAsync(created.OrderId, itemId);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveItem_NonExistentItem_ReturnsFalse()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>
        {
            new("Produto", 1, 10m)
        });

        var created = await svc.CreateOrderAsync(dto);

        var result = await svc.RemoveItemAsync(created.OrderId, Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateItem_Express_RecalculatesWithSurcharge()
    {
        var svc = CreateService();
        var dto = new CreateOrderDto(OrderType.Express, new List<CreateItemDto>
        {
            new("Produto", 1, 100m)
        });

        var created = await svc.CreateOrderAsync(dto);
        var order = await svc.GetOrderAsync(created.OrderId);
        var itemId = order!.Items[0].Id;

        var updated = await svc.UpdateItemAsync(created.OrderId, itemId, new UpdateItemDto("Produto Atualizado", 2, 100m));

        Assert.Equal(200m, updated!.SubTotal);
        Assert.Equal(230m, updated.Total);
    }
}
