using System;

namespace Core.Entities;

public class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}
