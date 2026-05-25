using System;
using System.Collections.Generic;
using Core.Enums;

namespace Core.Entities;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public OrderType Type { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal SubTotal { get; private set; }
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void RecalculateSubTotal()
    {
        SubTotal = Items.Sum(i => i.Total);
    }

    public void SetTotal(decimal total)
    {
        Total = total;
    }
}
