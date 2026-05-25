using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
