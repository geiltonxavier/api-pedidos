using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Discounts;
using Application.Services;
using Xunit;
using Core.Entities;
using Core.Enums;

namespace Tests;

public class OrderServiceTests
{
    private OrderService CreateService()
    {
        var strategies = new List<IDiscountStrategy>
        {
            new StandardDiscount(),
            new ExpressDiscount(),
            new SubscriptionDiscount()
        };
        var factory = new DiscountFactory(strategies);
        return new OrderService(factory);
    }

    [Fact]
    public async Task Create_StandardOrder_CalculatesSubtotalAndTotal()
    {
        var svc = CreateService();
        var items = new List<OrderItem> { new OrderItem { Quantity = 2, UnitPrice = 10m } };
        var order = await svc.CreateOrderAsync(OrderType.Standard, items);

        Assert.Equal(20m, order.SubTotal);
        Assert.Equal(20m, order.Total);
    }

    [Fact]
    public async Task Create_ExpressOrder_Applies15PercentSurcharge()
    {
        var svc = CreateService();
        var items = new List<OrderItem> { new OrderItem { Quantity = 1, UnitPrice = 100m } };
        var order = await svc.CreateOrderAsync(OrderType.Express, items);

        Assert.Equal(100m, order.SubTotal);
        Assert.Equal(115m, order.Total);
    }

    [Fact]
    public async Task Create_SubscriptionOrder_Applies10PercentDiscount()
    {
        var svc = CreateService();
        var items = new List<OrderItem> { new OrderItem { Quantity = 3, UnitPrice = 50m } };
        var order = await svc.CreateOrderAsync(OrderType.Subscription, items);

        Assert.Equal(150m, order.SubTotal);
        Assert.Equal(135m, order.Total);
    }
}
