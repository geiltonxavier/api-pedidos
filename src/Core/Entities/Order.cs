using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;

namespace Core.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public OrderType Type { get; init; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal SubTotal { get; private set; }
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void AddItem(OrderItem item) => _items.Add(item);

    public void AddItems(IEnumerable<OrderItem> items) => _items.AddRange(items);

    public bool RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }

    public void RecalculateSubTotal()
    {
        SubTotal = _items.Sum(i => i.Total);
    }

    public void SetTotal(decimal total)
    {
        Total = total;
    }
}
