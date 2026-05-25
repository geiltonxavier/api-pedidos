using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public class SubscriptionDiscount : IDiscountStrategy
{
    public OrderType Type => OrderType.Subscription;

    public decimal CalculateTotal(Order order)
    {
        // Desconto de 10% (cliente assinante)
        return order.SubTotal * 0.90m;
    }
}
