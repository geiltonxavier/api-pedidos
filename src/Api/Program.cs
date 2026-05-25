using System;
using System.Linq;
using Api.Filters;
using Api.Middlewares;
using Application.Discounts;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrdersDbContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// register discount strategies
builder.Services.AddSingleton<IDiscountStrategy, StandardDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, ExpressDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, SubscriptionDiscount>();
builder.Services.AddSingleton<DiscountFactory>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IdempotencyFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orders API");
    });
}

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.MapControllers();

app.Run();

