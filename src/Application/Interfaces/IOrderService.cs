using Application.DTO;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<CreatedOrderResponse> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderSummaryDto?> GetOrderAsync(Guid id);
    Task<OrderSummaryDto?> UpdateItemAsync(Guid orderId, Guid itemId, UpdateItemDto dto);
    Task<bool> RemoveItemAsync(Guid orderId, Guid itemId);
}
