using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public sealed class ExpressDiscount : IDiscountStrategy
{
    public OrderType Type => OrderType.Express;

    public decimal CalculateTotal(Order order)
    {
        // Acréscimo de 15% (taxa de entrega rápida)
        return order.SubTotal * 1.15m;
    }
}
