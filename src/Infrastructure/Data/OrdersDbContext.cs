using System;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);

            b.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            b.Property(o => o.SubTotal).HasPrecision(18, 2);
            b.Property(o => o.Total).HasPrecision(18, 2);

            b.Property(o => o.RowVersion)
                .IsRowVersion();

            b.OwnsMany(o => o.Items, ib =>
            {
                ib.WithOwner().HasForeignKey("OrderId");
                ib.Property<Guid>("Id");
                ib.HasKey("Id");
                ib.Property(i => i.UnitPrice).HasPrecision(18, 2);
            });
        });
    }
}
