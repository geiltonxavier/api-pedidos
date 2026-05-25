using Application.DTO;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<CreatedOrderResponse> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderSummaryDto?> GetOrderAsync(Guid id);
}
