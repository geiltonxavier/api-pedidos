using Core.Entities;
using Core.Enums;
using Microsoft.Extensions.Options;

namespace Application.Discounts;

public sealed class SubscriptionDiscount : IPricingStrategy
{
    private readonly decimal _multiplier;

    public SubscriptionDiscount(IOptions<PricingOptions> options)
    {
        _multiplier = 1 - (options.Value.SubscriptionDiscountPercent / 100m);
    }

    public OrderType Type => OrderType.Subscription;

    public decimal CalculateTotal(Order order)
    {
        return order.SubTotal * _multiplier;
    }
}
