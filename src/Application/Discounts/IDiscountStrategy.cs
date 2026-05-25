using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public interface IDiscountStrategy
{
    OrderType Type { get; }
    decimal CalculateTotal(Order order);
}
