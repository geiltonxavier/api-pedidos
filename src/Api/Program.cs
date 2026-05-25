using System;
using System.Linq;
using Application.Discounts;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdersDbContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// register discount strategies
builder.Services.AddSingleton<IDiscountStrategy, StandardDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, ExpressDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, SubscriptionDiscount>();
builder.Services.AddSingleton<DiscountFactory>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();

