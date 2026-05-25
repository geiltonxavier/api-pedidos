using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Core.Entities;
using Core.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _db;

    public OrderRepository(OrdersDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        try
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException(
                $"O pedido '{order.Id}' foi modificado por outro processo. Consulte novamente e tente de novo.");
        }
    }
}
