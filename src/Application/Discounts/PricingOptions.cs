namespace Application.Discounts;

public sealed class PricingOptions
{
    public decimal ExpressSurchargePercent { get; set; } = 15;
    public decimal SubscriptionDiscountPercent { get; set; } = 10;
}
