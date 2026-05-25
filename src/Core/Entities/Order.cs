using System;
using System.Collections.Generic;
using Core.Enums;

namespace Core.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public OrderType Type { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
