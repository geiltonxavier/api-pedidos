using Core.Entities;
using Core.Enums;
using Microsoft.Extensions.Options;

namespace Application.Discounts;

public sealed class ExpressDiscount : IPricingStrategy
{
    private readonly decimal _multiplier;

    public ExpressDiscount(IOptions<PricingOptions> options)
    {
        _multiplier = 1 + (options.Value.ExpressSurchargePercent / 100m);
    }

    public OrderType Type => OrderType.Express;

    public decimal CalculateTotal(Order order)
    {
        return order.SubTotal * _multiplier;
    }
}
