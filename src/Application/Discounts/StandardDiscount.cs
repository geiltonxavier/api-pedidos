using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public class StandardDiscount : IDiscountStrategy
{
    public OrderType Type => OrderType.Standard;

    public decimal CalculateTotal(Order order)
    {
        return order.SubTotal;
    }
}
