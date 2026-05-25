using Core.Entities;
using Core.Enums;

namespace Application.Discounts;

public interface IPricingStrategy
{
    OrderType Type { get; }
    decimal CalculateTotal(Order order);
}
