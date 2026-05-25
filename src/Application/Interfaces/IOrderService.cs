using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<CreatedOrderResponse> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderSummaryDto?> GetOrderAsync(Guid id, CancellationToken ct = default);
    Task<OrderSummaryDto?> UpdateItemAsync(Guid orderId, Guid itemId, UpdateItemDto dto, CancellationToken ct = default);
    Task<bool> RemoveItemAsync(Guid orderId, Guid itemId, CancellationToken ct = default);
}
