using System.Collections.Generic;
using System.Linq;
using Application.DTO;
using Core.Entities;

namespace Application.Helpers;

public static class OrderMapper
{
    public static List<OrderItem> ToDomainItems(this CreateOrderDto dto)
    {
        return dto.Items.Select(i => new OrderItem
        {
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();
    }

    public static OrderSummaryDto ToSummaryDto(this Order order)
    {
        return new OrderSummaryDto(
            order.Id,
            order.Type,
            order.SubTotal,
            order.Total,
            Math.Abs(order.SubTotal - order.Total),
            order.Items.Select(i => new OrderItemDto(i.Id, i.Description, i.Quantity, i.UnitPrice, i.Total)).ToList());
    }
}
