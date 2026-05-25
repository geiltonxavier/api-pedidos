using System;
using System.Linq;
using Api.Filters;
using Api.Middlewares;
using Application.Discounts;
using Application.Interfaces;
using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
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

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Application.Validators.CreateOrderDtoValidator>();

builder.Services.AddDbContext<OrdersDbContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// register discount strategies
builder.Services.AddSingleton<IDiscountStrategy, StandardDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, ExpressDiscount>();
builder.Services.AddSingleton<IDiscountStrategy, SubscriptionDiscount>();
builder.Services.AddSingleton<DiscountFactory>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IdempotencyFilter>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orders API");
    });
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

