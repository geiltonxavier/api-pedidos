using Core.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
}
