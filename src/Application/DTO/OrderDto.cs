using Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.DTO;

public record CreateItemDto(string Description, int Quantity, decimal UnitPrice);
public record CreateOrderDto(OrderType Type, List<CreateItemDto> Items);
public record OrderItemDto(string Description, int Quantity, decimal UnitPrice, decimal Total);
public record OrderSummaryDto(Guid Id, OrderType Type, decimal SubTotal, decimal Total, decimal DiscountValue, List<OrderItemDto> Items);
public record CreatedOrderResponse(Guid OrderId);
