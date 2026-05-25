using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public sealed class StandardDiscount : IPricingStrategy
{
    public OrderType Type => OrderType.Standard;

    public decimal CalculateTotal(Order order)
    {
        return order.SubTotal;
    }
}
